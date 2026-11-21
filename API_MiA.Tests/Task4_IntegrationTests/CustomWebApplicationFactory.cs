using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;
using ЛР3_МіА._АПІшка.models;

namespace ЛР3_МіА._АПІшка.Tests.Task4_IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private EphemeralMongoRunner? _mongoRunner;
    public IMongoDatabase Database { get; private set; } = null!;
    public IMongoClient MongoClient { get; private set; } = null!;
    public string DatabaseName { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _mongoRunner = EphemeralMongoRunner.Start();
        MongoClient = new MongoClient(_mongoRunner.ConnectionString);
        DatabaseName = "IntegrationTestDb_" + Guid.NewGuid().ToString("N");
        Database = MongoClient.GetDatabase(DatabaseName);
        await Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var mongoClientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoClient));
            if (mongoClientDescriptor != null)
                services.Remove(mongoClientDescriptor);

            var mongoDatabaseDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoDatabase));
            if (mongoDatabaseDescriptor != null)
                services.Remove(mongoDatabaseDescriptor);

            services.AddSingleton<IMongoClient>(_ => MongoClient);
            services.AddScoped<IMongoDatabase>(_ => Database);
        });
    }
    public async Task ResetDatabaseAsync()
    {
        await Database.DropCollectionAsync("RideOrders");
        await Database.DropCollectionAsync("Passengers");
        await Database.DropCollectionAsync("drivers");
    }
    public System.Net.Http.HttpClient CreateClientWithAuth(string id = "test-user-id", string email = "user@example.com", Roles role = Roles.Admin)
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        var jwtGenerator = scope.ServiceProvider.GetRequiredService<JwtTokenGenerator>();
        var token = jwtGenerator.GenerateToken(id, email, role);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public new async Task DisposeAsync()
    {
        try
        {
            if (Database != null)
            {
                await MongoClient.DropDatabaseAsync(DatabaseName);
            }
        }
        catch { }

        _mongoRunner?.Dispose();
        await base.DisposeAsync();
    }
}
