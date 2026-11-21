using MongoDB.Driver;
using ЛР3_МіА._АПІшка.models;

public interface IRideOrderRepository
{
    Task<List<RideOrder>> GetAllAsync();
    Task<RideOrder?> GetByIdAsync(string id);
    Task CreateAsync(RideOrder rideOrder);
    Task UpdateAsync(RideOrder rideOrder);
    Task DeleteAsync(string id);
}

public class RideOrderRepository : IRideOrderRepository
{
    private readonly IMongoCollection<RideOrder> _rideOrdersCollection;

    public RideOrderRepository(IMongoDatabase database)
    {
        _rideOrdersCollection = database.GetCollection<RideOrder>("RideOrders");
    }

    public async Task<List<RideOrder>> GetAllAsync() =>
        await _rideOrdersCollection.Find(ro => true).ToListAsync();

    public async Task<RideOrder?> GetByIdAsync(string id) =>
        await _rideOrdersCollection.Find(ro => ro.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(RideOrder rideOrder) =>
        await _rideOrdersCollection.InsertOneAsync(rideOrder);

    public async Task UpdateAsync(RideOrder updatedRideOrder) =>
        await _rideOrdersCollection.ReplaceOneAsync(ro => ro.Id == updatedRideOrder.Id, updatedRideOrder);

    public async Task DeleteAsync(string id) =>
        await _rideOrdersCollection.DeleteOneAsync(ro => ro.Id == id);
}