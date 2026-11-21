using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ЛР3_МіА._АПІшка.models;

public interface IDriverRepository
{
    Task<List<Driver>> GetAllAsync();
    Task<Driver?> GetByIdAsync(string id);
    Task CreateAsync(Driver driver);
    Task UpdateAsync(Driver updatedDriver);
    Task DeleteAsync(string id);
}

public class DriverRepository : IDriverRepository
{
    private readonly IMongoCollection<Driver> _driversCollection;

    public DriverRepository(IMongoDatabase database)
    {
        _driversCollection = database.GetCollection<Driver>("drivers");
    }

    public async Task<List<Driver>> GetAllAsync() =>
        await _driversCollection.Find(d => true).ToListAsync();

    public async Task<Driver?> GetByIdAsync(string id) =>
        await _driversCollection.Find(d => d.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Driver driver) =>
        await _driversCollection.InsertOneAsync(driver);

    public async Task UpdateAsync(Driver updatedDriver) =>
        await _driversCollection.ReplaceOneAsync(d => d.Id == updatedDriver.Id, updatedDriver);

    public async Task DeleteAsync(string id) =>
        await _driversCollection.DeleteOneAsync(d => d.Id == id);
}
