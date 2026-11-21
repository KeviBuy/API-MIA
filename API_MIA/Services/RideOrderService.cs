using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ЛР3_МіА._АПІшка.models;

public interface IRideOrderService
{
    Task<List<RideOrder>> GetAllAsync();
    Task<RideOrder?> GetByIdAsync(string id);
    Task<object> CreateAsync(RideOrder rideOrder);
    Task UpdateAsync(string id, RideOrder updatedRideOrder);
    Task DeleteAsync(string id);

    Task<object> AcceptRideOrderAsync(string rideOrderId, string driverId);
    Task<object> DriverArrivedAsync(string rideOrderId);
    Task<object> CompleteRideOrderAsync(string rideOrderId);
    Task<object> CancelRideOrderAsync(string rideOrderId, bool canceledByPassenger);

    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    // Нові методи для Завдання 2 (обчислення та агрегація за допомогою LINQ)
    Task<double> CalculateTotalEarningsByDriverAsync(string driverId);
    Task<double> CalculateAverageCompletedPriceAsync();
    Task<RideOrder> GetLongestRideByPassengerAsync(string passengerId);
    Task<Dictionary<RideStatus, int>> GetOrdersCountByStatusAsync();
}

public class RideOrderService : IRideOrderService
{
    private readonly IRideOrderRepository _rideOrderRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IPassengerRepository _passengerRepository;

    public RideOrderService(IRideOrderRepository rideOrderRepository, IDriverRepository driverRepository, IPassengerRepository passengerRepository)
    {
        _rideOrderRepository = rideOrderRepository;
        _driverRepository = driverRepository;
        _passengerRepository = passengerRepository;
    }

    public async Task<List<RideOrder>> GetAllAsync() =>
        await _rideOrderRepository.GetAllAsync();

    public async Task<RideOrder?> GetByIdAsync(string id) =>
        await _rideOrderRepository.GetByIdAsync(id);

    public async Task<object> CreateAsync(RideOrder rideOrder)
    {
        var passenger = await _passengerRepository.GetByIdAsync(rideOrder.PassengerId);
        if (passenger == null)
            throw new Exception("Пасажира не знайдено");

        if (passenger.HasUnpaidDebts)
            throw new Exception("Неможливо створити замовлення: у пасажира є заборгованість.");

        var activeRides = await _rideOrderRepository.GetAllAsync();
        bool hasActiveRide = activeRides.Any(r => r.PassengerId == rideOrder.PassengerId &&
            (r.Status == RideStatus.Searching || r.Status == RideStatus.DriverAssigned || r.Status == RideStatus.InProgress));
        if (hasActiveRide)
            throw new Exception("У пасажира вже є активна поїздка.");

        // price
        rideOrder.BasePrice = 40.0 + (rideOrder.DistanceKm * 12.0);
        rideOrder.FinalPrice = rideOrder.BasePrice * rideOrder.SurgeMultiplier;
        rideOrder.Status = RideStatus.Searching;

        // search drivers
        var eligibleDrivers = await _driverRepository.GetAllAsync();
        eligibleDrivers = eligibleDrivers.Where(d =>
            d.Status == DriverStatus.Online &&
            d.Class == rideOrder.RequestedClass &&
            CalculateDistance(rideOrder.PickupLatitude, rideOrder.PickupLongitude, d.CurrentLatitude, d.CurrentLongitude) <= 3.0
        ).ToList();

        await _rideOrderRepository.CreateAsync(rideOrder);
        return new { Order = rideOrder, NotifiedDriversCount = eligibleDrivers.Count };
    }

    public async Task UpdateAsync(string id, RideOrder updatedRideOrder)
    {
        var existingOrder = await _rideOrderRepository.GetByIdAsync(updatedRideOrder.Id);
        if (existingOrder == null)
            throw new Exception("Замовлення не знайдено");
        if (id != updatedRideOrder.Id)
            throw new Exception("Ідентифікатори не співпадають!");
        await _rideOrderRepository.UpdateAsync(updatedRideOrder);
    }

    public async Task DeleteAsync(string id)
    {
        var existingOrder = await _rideOrderRepository.GetByIdAsync(id);
        if (existingOrder == null)
            throw new Exception("Замовлення не знайдено");
        await _rideOrderRepository.DeleteAsync(id);
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var r = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public async Task<object> AcceptRideOrderAsync(string rideOrderId, string driverId)
    {
        var rideOrder = await _rideOrderRepository.GetByIdAsync(rideOrderId.ToString());
        if (rideOrder == null)
            throw new Exception("Замовлення не знайдено");

        if (rideOrder.Status != RideStatus.Searching)
            throw new Exception("Замовлення не доступне для прийняття");

        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null)
            throw new Exception("Водія не знайдено");

        if (driver.Status != DriverStatus.Online)
            throw new Exception("Водій не доступний для прийняття замовлення");

        var passenger = await _passengerRepository.GetByIdAsync(rideOrder.PassengerId);
        if (passenger == null || (passenger.Balance - passenger.BlockedBalance) < rideOrder.FinalPrice)
            throw new Exception("Недостатньо вільних коштів на балансі пасажира для холдування");

        rideOrder.DriverId = driverId;
        rideOrder.Status = RideStatus.DriverAssigned;
        rideOrder.AcceptedAt = DateTime.UtcNow;

        driver.Status = DriverStatus.Busy;

        await _rideOrderRepository.UpdateAsync(rideOrder);
        await _driverRepository.UpdateAsync(driver);

        return new { Message = "Замовлення прийнято водієм. Кошти заблоковано на балансі пасажира.", RideOrder = rideOrder, Driver = driver };
    }

    public async Task<object> DriverArrivedAsync(string rideOrderId)
    {
        var rideOrder = await _rideOrderRepository.GetByIdAsync(rideOrderId);
        if (rideOrder == null)
            throw new Exception("Замовлення не знайдено");

        if (rideOrder.Status != RideStatus.DriverAssigned)
            throw new Exception("Статус замовлення не дозволяє відзначити прибуття водія");

        rideOrder.Status = RideStatus.Arrived;
        rideOrder.ArrivedAt = DateTime.UtcNow;
        await _rideOrderRepository.UpdateAsync(rideOrder);

        return new { Message = "Водій прибув на точку. Активовано 3 хвилини безкоштовного очікування.", RideOrder = rideOrder };
    }

    public async Task<object> CompleteRideOrderAsync(string rideOrderId)
    {
        var rideOrder = await _rideOrderRepository.GetByIdAsync(rideOrderId);
        if (rideOrder == null)
            throw new Exception("Замовлення не знайдено");

        if (rideOrder.Status != RideStatus.InProgress && rideOrder.Status != RideStatus.Arrived)
            throw new Exception("Статус замовлення не дозволяє завершити поїздку");

        var passenger = await _passengerRepository.GetByIdAsync(rideOrder.PassengerId);
        var driver = rideOrder.DriverId != null ? await _driverRepository.GetByIdAsync(rideOrder.DriverId) : null;

        if (passenger == null || driver == null)
            throw new Exception("Пасажир або водій не знайдені");

        double extraWaitingTimeFee = 0.0;
        if (rideOrder.Status == RideStatus.Arrived && rideOrder.ArrivedAt.HasValue)
        {
            var waitingTime = (DateTime.UtcNow - rideOrder.ArrivedAt.Value).TotalMinutes;
            if (waitingTime > 3.0)
                extraWaitingTimeFee = Math.Ceiling(waitingTime - 3.0) * 3.0;
        }

        double totalToPay = rideOrder.FinalPrice + extraWaitingTimeFee;

        passenger.Balance -= totalToPay;
        passenger.BlockedBalance -= rideOrder.FinalPrice;

        driver.Status = DriverStatus.Online;
        rideOrder.CompletedAt = DateTime.UtcNow;
        rideOrder.FinalPrice += extraWaitingTimeFee;
        rideOrder.Status = RideStatus.Completed;

        await _passengerRepository.UpdateAsync(passenger);
        await _driverRepository.UpdateAsync(driver);
        await _rideOrderRepository.UpdateAsync(rideOrder);

        return new { Message = "Поїздка завершена. Кошти списано з балансу пасажира.", RideOrder = rideOrder, Passenger = passenger, Driver = driver };
    }

    public async Task<object> CancelRideOrderAsync(string rideOrderId, bool cancelByPassenger)
    {
        var rideOrder = await _rideOrderRepository.GetByIdAsync(rideOrderId);
        if (rideOrder == null)
            throw new Exception("Замовлення не знайдено");

        if (rideOrder.Status == RideStatus.Completed || rideOrder.Status == RideStatus.Cancelled)
            throw new Exception("Скасування неможливе з поточного стану");

        var passenger = await _passengerRepository.GetByIdAsync(rideOrder.PassengerId);
        var driver = rideOrder.DriverId != null ? await _driverRepository.GetByIdAsync(rideOrder.DriverId) : null;

        if (passenger == null)
            throw new Exception("Пасажир не знайдений");

        if (driver != null)
            driver.Status = DriverStatus.Online;

        if (cancelByPassenger && rideOrder.Status == RideStatus.DriverAssigned)
            passenger.Balance -= 40.0;

        passenger.BlockedBalance -= rideOrder.FinalPrice;

        rideOrder.Status = RideStatus.Cancelled;
        rideOrder.CompletedAt = DateTime.UtcNow;

        await _passengerRepository.UpdateAsync(passenger);
        if (driver != null)
            await _driverRepository.UpdateAsync(driver);
        await _rideOrderRepository.UpdateAsync(rideOrder);

        return new { Message = (cancelByPassenger && rideOrder.Status == RideStatus.DriverAssigned) ? "Поїздку скасовано. З пасажира стягнуто штраф 40 грн на користь водія." : "Поїздку скасовано.", RideOrder = rideOrder, Passenger = passenger, Driver = driver };
    }

    #region Нові методи обчислень та агрегацій за допомогою LINQ (Завдання 2)

    public async Task<double> CalculateTotalEarningsByDriverAsync(string driverId)
    {
        if (string.IsNullOrWhiteSpace(driverId))
            throw new ArgumentException("Ідентифікатор водія не може бути порожнім.", nameof(driverId));

        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null)
            throw new KeyNotFoundException($"Водія з ідентифікатором '{driverId}' не знайдено.");

        var allOrders = await _rideOrderRepository.GetAllAsync();
        var driverOrders = allOrders
            .Where(o => o.DriverId == driverId && o.Status == RideStatus.Completed)
            .ToList();

        if (!driverOrders.Any())
            return 0.0;

        // Комісія водія становить 80% від вартості
        return Math.Round(driverOrders.Sum(o => o.FinalPrice * 0.8), 2);
    }

    public async Task<double> CalculateAverageCompletedPriceAsync()
    {
        var allOrders = await _rideOrderRepository.GetAllAsync();
        var completedOrders = allOrders
            .Where(o => o.Status == RideStatus.Completed)
            .ToList();

        if (!completedOrders.Any())
            return 0.0;

        return Math.Round(completedOrders.Average(o => o.FinalPrice), 2);
    }

    public async Task<RideOrder> GetLongestRideByPassengerAsync(string passengerId)
    {
        if (string.IsNullOrWhiteSpace(passengerId))
            throw new ArgumentException("Ідентифікатор пасажира не може бути порожнім.", nameof(passengerId));

        var passenger = await _passengerRepository.GetByIdAsync(passengerId);
        if (passenger == null)
            throw new KeyNotFoundException($"Пасажира з ідентифікатором '{passengerId}' не знайдено.");

        var allOrders = await _rideOrderRepository.GetAllAsync();
        var passengerOrders = allOrders
            .Where(o => o.PassengerId == passengerId)
            .ToList();

        if (!passengerOrders.Any())
            throw new InvalidOperationException("У пасажира немає жодної зареєстрованої поїздки.");

        return passengerOrders.OrderByDescending(o => o.DistanceKm).First();
    }

    public async Task<Dictionary<RideStatus, int>> GetOrdersCountByStatusAsync()
    {
        var allOrders = await _rideOrderRepository.GetAllAsync();
        return allOrders
            .GroupBy(o => o.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    #endregion
}
