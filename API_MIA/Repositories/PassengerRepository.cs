using ЛР3_МіА._АПІшка.models;
using MongoDB.Driver;

public interface IPassengerRepository
{
    Task<List<Passenger>> GetAllAsync();
    Task<Passenger?> GetByIdAsync(string id);
    Task CreateAsync(Passenger passenger);
    Task UpdateAsync(Passenger passenger);
    Task DeleteAsync(string id);
}

public class PassengerRepository : IPassengerRepository
{
    private readonly IMongoCollection<Passenger> _passengersCollection;

    public PassengerRepository(IMongoDatabase database)
    {
        _passengersCollection = database.GetCollection<Passenger>("Passengers");
    }

    public async Task<List<Passenger>> GetAllAsync() =>
        await _passengersCollection.Find(_ => true).ToListAsync();
    
    public async Task<Passenger?> GetByIdAsync(string id) =>
        await _passengersCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    
    public async Task CreateAsync(Passenger passenger) =>
        await _passengersCollection.InsertOneAsync(passenger);
    
    public async Task UpdateAsync(Passenger passenger) =>
        await _passengersCollection.ReplaceOneAsync(p => p.Id == passenger.Id, passenger);
    
    public async Task DeleteAsync(string id) =>
        await _passengersCollection.DeleteOneAsync(p => p.Id == id);
    
}