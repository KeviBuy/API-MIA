using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using ЛР3_МіА._АПІшка.models;

namespace ЛР3_МіА._АПІшка.Tests.Task2_ServiceMoqTests;

public class RideOrderServiceMoqTests
{
    private readonly Mock<IRideOrderRepository> _mockRideRepo;
    private readonly Mock<IDriverRepository> _mockDriverRepo;
    private readonly Mock<IPassengerRepository> _mockPassengerRepo;
    private readonly RideOrderService _service;

    public RideOrderServiceMoqTests()
    {
        _mockRideRepo = new Mock<IRideOrderRepository>();
        _mockDriverRepo = new Mock<IDriverRepository>();
        _mockPassengerRepo = new Mock<IPassengerRepository>();

        _service = new RideOrderService(
            _mockRideRepo.Object,
            _mockDriverRepo.Object,
            _mockPassengerRepo.Object
        );
    }

    // 1. Тестування LINQ-агрегацій та нових аналітичних методів

    [Fact]
    public async Task CalculateTotalEarningsByDriverAsync_WhenDriverHasCompletedRides_ReturnsEightyPercentOfSum()
    {
        // Arrange
        var driverId = "driver-001";
        var driver = new Driver { Id = driverId, FullName = "Олександр Водій", Status = DriverStatus.Online };
        _mockDriverRepo.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);

        var orders = new List<RideOrder>
        {
            new() { Id = "1", DriverId = driverId, Status = RideStatus.Completed, FinalPrice = 100.0 },
            new() { Id = "2", DriverId = driverId, Status = RideStatus.Completed, FinalPrice = 200.0 },
            new() { Id = "3", DriverId = driverId, Status = RideStatus.Cancelled, FinalPrice = 150.0 }, // Не враховується
            new() { Id = "4", DriverId = "other-driver", Status = RideStatus.Completed, FinalPrice = 500.0 } // Інший водій
        };
        _mockRideRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        // Act
        var earnings = await _service.CalculateTotalEarningsByDriverAsync(driverId);

        // Assert: (100 + 200) * 0.8 = 240.0
        earnings.Should().Be(240.0);
        _mockDriverRepo.Verify(r => r.GetByIdAsync(driverId), Times.Once);
        _mockRideRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CalculateTotalEarningsByDriverAsync_WhenDriverHasNoCompletedRides_ReturnsZero()
    {
        // Arrange
        var driverId = "driver-002";
        _mockDriverRepo.Setup(r => r.GetByIdAsync(driverId))
            .ReturnsAsync(new Driver { Id = driverId });
        _mockRideRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<RideOrder>());

        // Act
        var earnings = await _service.CalculateTotalEarningsByDriverAsync(driverId);

        // Assert
        earnings.Should().Be(0.0);
    }

    [Fact]
    public async Task CalculateTotalEarningsByDriverAsync_WhenDriverNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var driverId = "non-existent-driver";
        _mockDriverRepo.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync((Driver?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CalculateTotalEarningsByDriverAsync(driverId));

        _mockRideRepo.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CalculateTotalEarningsByDriverAsync_WhenDriverIdInvalid_ThrowsArgumentException(string? invalidId)
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CalculateTotalEarningsByDriverAsync(invalidId!));

        _mockDriverRepo.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CalculateAverageCompletedPriceAsync_WhenCompletedOrdersExist_ReturnsCorrectAverage()
    {
        // Arrange
        var orders = new List<RideOrder>
        {
            new() { Id = "1", Status = RideStatus.Completed, FinalPrice = 120.0 },
            new() { Id = "2", Status = RideStatus.Completed, FinalPrice = 180.0 },
            new() { Id = "3", Status = RideStatus.Searching, FinalPrice = 50.0 }
        };
        _mockRideRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        // Act
        var average = await _service.CalculateAverageCompletedPriceAsync();

        // Assert: (120 + 180) / 2 = 150.0
        average.Should().Be(150.0);
        _mockRideRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CalculateAverageCompletedPriceAsync_WhenCollectionIsEmpty_ReturnsZero()
    {
        // Arrange (граничний випадок: порожня колекція даних)
        _mockRideRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RideOrder>());

        // Act
        var average = await _service.CalculateAverageCompletedPriceAsync();

        // Assert
        average.Should().Be(0.0);
    }

    [Fact]
    public async Task GetLongestRideByPassengerAsync_WhenPassengerHasMultipleRides_ReturnsRideWithMaxDistance()
    {
        // Arrange
        var passengerId = "pass-100";
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId))
            .ReturnsAsync(new Passenger { Id = passengerId, FullName = "Кирило" });

        var shortest = new RideOrder { Id = "r1", PassengerId = passengerId, DistanceKm = 2.4 };
        var longest = new RideOrder { Id = "r2", PassengerId = passengerId, DistanceKm = 18.5 };
        var medium = new RideOrder { Id = "r3", PassengerId = passengerId, DistanceKm = 7.1 };

        _mockRideRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<RideOrder> { shortest, longest, medium });

        // Act
        var result = await _service.GetLongestRideByPassengerAsync(passengerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("r2");
        result.DistanceKm.Should().Be(18.5);

        _mockPassengerRepo.Verify(r => r.GetByIdAsync(passengerId), Times.Once);
        _mockRideRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetLongestRideByPassengerAsync_WhenPassengerHasNoRides_ThrowsInvalidOperationException()
    {
        // Arrange
        var passengerId = "pass-empty";
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId))
            .ReturnsAsync(new Passenger { Id = passengerId });
        _mockRideRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<RideOrder>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetLongestRideByPassengerAsync(passengerId));
    }

    // 2. Тестування бізнес-правил створення, прийняття та скасування поїздки

    [Fact]
    public async Task CreateAsync_WhenPassengerValidAndNoActiveRides_CalculatesPriceAndCallsRepository()
    {
        // Arrange
        var passengerId = "pass-01";
        var passenger = new Passenger { Id = passengerId, HasUnpaidDebts = false };
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(passenger);
        _mockRideRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RideOrder>());

        // Водій поблизу в межах 3 км
        var nearbyDriver = new Driver
        {
            Id = "d-near",
            Status = DriverStatus.Online,
            Class = CarClass.Standard,
            CurrentLatitude = 50.4500,
            CurrentLongitude = 30.5230
        };
        _mockDriverRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Driver> { nearbyDriver });

        var newOrder = new RideOrder
        {
            PassengerId = passengerId,
            PickupLatitude = 50.4501,
            PickupLongitude = 30.5231,
            DistanceKm = 10.0,
            RequestedClass = CarClass.Standard,
            SurgeMultiplier = 1.5
        };

        // Act
        var result = await _service.CreateAsync(newOrder);

        // Assert: BasePrice = 40 + (10 * 12) = 160; FinalPrice = 160 * 1.5 = 240
        newOrder.BasePrice.Should().Be(160.0);
        newOrder.FinalPrice.Should().Be(240.0);
        newOrder.Status.Should().Be(RideStatus.Searching);

        _mockRideRepo.Verify(r => r.CreateAsync(newOrder), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPassengerHasUnpaidDebts_ThrowsExceptionAndNeverCreatesOrder()
    {
        // Arrange
        var passengerId = "pass-debt";
        var debtor = new Passenger { Id = passengerId, HasUnpaidDebts = true };
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(debtor);

        var order = new RideOrder { PassengerId = passengerId };

        // Act
        Func<Task> act = async () => await _service.CreateAsync(order);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*заборгованість*");

        _mockRideRepo.Verify(r => r.CreateAsync(It.IsAny<RideOrder>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenPassengerAlreadyHasActiveRide_ThrowsException()
    {
        // Arrange
        var passengerId = "pass-busy";
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId))
            .ReturnsAsync(new Passenger { Id = passengerId, HasUnpaidDebts = false });

        var existingActiveRide = new RideOrder
        {
            Id = "active-1",
            PassengerId = passengerId,
            Status = RideStatus.InProgress
        };
        _mockRideRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<RideOrder> { existingActiveRide });

        var order = new RideOrder { PassengerId = passengerId };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(order));
        ex.Message.Should().Contain("активна поїздка");

        _mockRideRepo.Verify(r => r.CreateAsync(It.IsAny<RideOrder>()), Times.Never);
    }

    [Fact]
    public async Task AcceptRideOrderAsync_WhenOrderSearchingAndPassengerHasBalance_AssignsDriverAndUpdatesRepositories()
    {
        // Arrange
        var orderId = "order-10";
        var driverId = "driver-10";
        var passengerId = "pass-10";

        var order = new RideOrder
        {
            Id = orderId,
            PassengerId = passengerId,
            Status = RideStatus.Searching,
            FinalPrice = 150.0
        };
        var driver = new Driver { Id = driverId, Status = DriverStatus.Online };
        var passenger = new Passenger { Id = passengerId, Balance = 500.0, BlockedBalance = 0.0 };

        _mockRideRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockDriverRepo.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(passenger);

        // Act
        var result = await _service.AcceptRideOrderAsync(orderId, driverId);

        // Assert
        order.Status.Should().Be(RideStatus.DriverAssigned);
        order.DriverId.Should().Be(driverId);
        driver.Status.Should().Be(DriverStatus.Busy);

        _mockRideRepo.Verify(r => r.UpdateAsync(It.Is<RideOrder>(o => o.Id == orderId && o.DriverId == driverId)), Times.Once);
        _mockDriverRepo.Verify(r => r.UpdateAsync(It.Is<Driver>(d => d.Id == driverId && d.Status == DriverStatus.Busy)), Times.Once);
    }

    [Fact]
    public async Task CancelRideOrderAsync_WhenCanceledByPassengerWithDriverAssigned_ChargesPenaltyFee()
    {
        // Arrange
        var orderId = "order-cancel";
        var driverId = "d-cancel";
        var passengerId = "p-cancel";

        var order = new RideOrder
        {
            Id = orderId,
            PassengerId = passengerId,
            DriverId = driverId,
            Status = RideStatus.DriverAssigned,
            FinalPrice = 200.0
        };
        var driver = new Driver { Id = driverId, Status = DriverStatus.Busy };
        var passenger = new Passenger { Id = passengerId, Balance = 300.0, BlockedBalance = 200.0 };

        _mockRideRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockDriverRepo.Setup(r => r.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(passenger);

        // Act
        var result = await _service.CancelRideOrderAsync(orderId, cancelByPassenger: true);

        // Assert: стягується штраф 40 грн, баланс стає 300 - 40 = 260
        passenger.Balance.Should().Be(260.0);
        order.Status.Should().Be(RideStatus.Cancelled);
        driver.Status.Should().Be(DriverStatus.Online);

        _mockPassengerRepo.Verify(r => r.UpdateAsync(passenger), Times.Once);
        _mockDriverRepo.Verify(r => r.UpdateAsync(driver), Times.Once);
        _mockRideRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }
}
