using FluentValidation;
using AutoMapper;
using System.Text.Json.Serialization;
using ЛР3_МіА._АПІшка.Validators;
using ЛР3_МіА._АПІшка.models;
using MongoDB.Driver;
using RideHailingApi.Controllers;
using ЛР3_МіА._АПІшка.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Конфігурація MongoDB
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
var mongoDatabaseName = builder.Configuration["MongoDbSettings:DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddScoped<IMongoDatabase>(sp => 
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

// Налаштування JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtTokenGenerator>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
    };
});

builder.Services.AddAuthorization();

// Сервіси, репозиторії та інструменти
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddScoped<IValidator<Passenger>, PassengerValidator>();
builder.Services.AddScoped<IValidator<RideOrder>, RideOrderValidator>();

builder.Services.AddScoped<IRideOrderService, RideOrderService>();
builder.Services.AddScoped<IPassengerService, PassengerService>();
builder.Services.AddScoped<IDriverService, DriverService>();

builder.Services.AddScoped<IRideOrderRepository, RideOrderRepository>();
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();

builder.Services.AddScoped<DriversController>();
builder.Services.AddScoped<PassengersController>();
builder.Services.AddScoped<RideOrdersController>();
builder.Services.AddScoped<AuthController>();

// Документація Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Включаем Swagger для тестирования API через UI

app.UseSwagger();
app.UseSwaggerUI();


// Порядок middleware має значення! Спеціально для безпеки:
app.UseAuthentication(); // 1. Спочатку розпізнати хто це (JWT)
app.UseAuthorization();  // 2. Потім перевірити чи має право доступу

app.MapControllers();

app.Run();

// Дозволяє WebApplicationFactory<Program> у тестах бачити клас Program
public partial class Program { }
