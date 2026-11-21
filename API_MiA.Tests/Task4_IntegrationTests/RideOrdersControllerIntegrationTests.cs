using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using ЛР3_МіА._АПІшка.models;

namespace ЛР3_МіА._АПІшка.Tests.Task4_IntegrationTests;

public class RideOrdersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly IMongoCollection<RideOrder> _ordersCollection;
    private readonly IMongoCollection<Passenger> _passengersCollection;
    private readonly IMongoCollection<Driver> _driversCollection;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public RideOrdersControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _ordersCollection = factory.Database.GetCollection<RideOrder>("RideOrders");
        _passengersCollection = factory.Database.GetCollection<Passenger>("Passengers");
        _driversCollection = factory.Database.GetCollection<Driver>("drivers");

        _client = factory.CreateClientWithAuth(role: Roles.Admin);
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    //1. Сценарій створення

    [Fact]
    public async Task CreateOrder_WhenDataValid_ReturnsOkAndPersistsOrderInMongoDb()
    {
        // 1. Arrange
        var passengerId = ObjectId.GenerateNewId().ToString();
        var passenger = new Passenger
        {
            Id = passengerId,
            FullName = "Кирило Пасажир",
            PhoneNumber = "+380991112233",
            Balance = 500.0,
            BlockedBalance = 0.0,
            HasUnpaidDebts = false
        };
        await _passengersCollection.InsertOneAsync(passenger);

        var driver = new Driver
        {
            Id = ObjectId.GenerateNewId().ToString(),
            FullName = "Іван Водій",
            Status = DriverStatus.Online,
            Class = CarClass.Standard,
            CurrentLatitude = 50.4500,
            CurrentLongitude = 30.5230
        };
        await _driversCollection.InsertOneAsync(driver);

        var newOrder = new RideOrder
        {
            PassengerId = passengerId,
            PickupLatitude = 50.4501,
            PickupLongitude = 30.5231,
            DropoffLatitude = 50.4600,
            DropoffLongitude = 30.5300,
            DistanceKm = 5.0,
            RequestedClass = CarClass.Standard,
            SurgeMultiplier = 1.2
        };

        // 2. Act
        var response = await _client.PostAsJsonAsync("/api/RideOrders/create-order", newOrder);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var savedOrders = await _ordersCollection.Find(o => o.PassengerId == passengerId).ToListAsync();
        savedOrders.Should().HaveCount(1);

        var persistedOrder = savedOrders[0];
        persistedOrder.Status.Should().Be(RideStatus.Searching);
        // BasePrice = 40 + (5 * 12) = 100; FinalPrice = 100 * 1.2 = 120
        persistedOrder.BasePrice.Should().Be(100.0);
        persistedOrder.FinalPrice.Should().Be(120.0);
    }

    [Fact]
    public async Task CreateOrder_WhenDistanceLessThanMinimum_ReturnsBadRequest_AndDoesNotSaveToMongoDb()
    {
        // 1. Arrange: некоректна дистанція (0.1 км < мінімум 0.5 км)
        var invalidOrder = new RideOrder
        {
            PassengerId = ObjectId.GenerateNewId().ToString(),
            PickupLatitude = 50.4500,
            PickupLongitude = 30.5230,
            DropoffLatitude = 50.4510,
            DropoffLongitude = 30.5235,
            DistanceKm = 0.1,
            RequestedClass = CarClass.Standard,
            SurgeMultiplier = 1.0
        };

        // 2. Act
        var response = await _client.PostAsJsonAsync("/api/RideOrders/create-order", invalidOrder);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Мінімальна дистанція");

        var count = await _ordersCollection.CountDocumentsAsync(FilterDefinition<RideOrder>.Empty);
        count.Should().Be(0);
    }

    // 2. Сценарій отримання: Знайдено (200 OK) та не знайдено (404 Not Found)

    [Fact]
    public async Task GetById_WhenOrderExists_ReturnsOkWithPersistedData()
    {
        // 1. Arrange: прямий запис у тестову БД
        var orderId = ObjectId.GenerateNewId().ToString();
        var existingOrder = new RideOrder
        {
            Id = orderId,
            PassengerId = "passenger-existing",
            DistanceKm = 8.5,
            Status = RideStatus.Searching,
            FinalPrice = 142.0
        };
        await _ordersCollection.InsertOneAsync(existingOrder);

        // 2. Act
        var response = await _client.GetAsync($"/api/RideOrders/{orderId}");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultOrder = await response.Content.ReadFromJsonAsync<RideOrder>(JsonOptions);
        resultOrder.Should().NotBeNull();
        resultOrder!.Id.Should().Be(orderId);
        resultOrder.DistanceKm.Should().Be(8.5);
    }

    [Fact]
    public async Task GetById_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // 1. Arrange: неіснуючий ідентифікатор
        var nonExistentId = ObjectId.GenerateNewId().ToString();

        // 2. Act
        var response = await _client.GetAsync($"/api/RideOrders/{nonExistentId}");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // 3. Сценарій прийняття замовлення: Зміна статусу в БД

    [Fact]
    public async Task AcceptRide_WhenValidConditions_UpdatesOrderAndDriverStatusInMongoDb()
    {
        // 1. Arrange
        var passengerId = ObjectId.GenerateNewId().ToString();
        var driverId = ObjectId.GenerateNewId().ToString();
        var orderId = ObjectId.GenerateNewId().ToString();

        var passenger = new Passenger
        {
            Id = passengerId,
            FullName = "Петро",
            Balance = 300.0,
            BlockedBalance = 0.0
        };
        var driver = new Driver
        {
            Id = driverId,
            FullName = "Сергій",
            Status = DriverStatus.Online
        };
        var order = new RideOrder
        {
            Id = orderId,
            PassengerId = passengerId,
            Status = RideStatus.Searching,
            FinalPrice = 150.0
        };

        await _passengersCollection.InsertOneAsync(passenger);
        await _driversCollection.InsertOneAsync(driver);
        await _ordersCollection.InsertOneAsync(order);

        // 2. Act
        var response = await _client.PostAsync($"/api/RideOrders/{orderId}/accept?driverId={driverId}", null);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedOrder = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
        updatedOrder.Should().NotBeNull();
        updatedOrder.Status.Should().Be(RideStatus.DriverAssigned);
        updatedOrder.DriverId.Should().Be(driverId);

        var updatedDriver = await _driversCollection.Find(d => d.Id == driverId).FirstOrDefaultAsync();
        updatedDriver.Should().NotBeNull();
        updatedDriver.Status.Should().Be(DriverStatus.Busy);
    }

    // 4. Сценарій видалення: Повне видалення з MongoDB

    [Fact]
    public async Task Delete_WhenOrderExists_RemovesDocumentFromMongoDb()
    {
        // 1. Arrange
        var orderId = ObjectId.GenerateNewId().ToString();
        var order = new RideOrder { Id = orderId, PassengerId = "to-be-deleted" };
        await _ordersCollection.InsertOneAsync(order);

        var countBefore = await _ordersCollection.CountDocumentsAsync(o => o.Id == orderId);
        countBefore.Should().Be(1);

        // 2. Act
        var response = await _client.DeleteAsync($"/api/RideOrders/{orderId}");

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var countAfter = await _ordersCollection.CountDocumentsAsync(o => o.Id == orderId);
        countAfter.Should().Be(0);
    }
}
