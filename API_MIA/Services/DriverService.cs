using AutoMapper;
using ЛР3_МіА._АПІшка.models;

public interface IDriverService
{
    Task<List<DriverDto>> GetAllDriversAsync();
    Task<DriverDto?> GetDriverByIdAsync(string id);
    Task CreateAsync(DriverCreateDto createDto);
    Task UpdateAsync(string id, DriverUpdateDto updateDto);
    Task DeleteAsync(string id);
}

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public DriverService(IDriverRepository driverRepository, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _driverRepository = driverRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<DriverDto>> GetAllDriversAsync()
    {
        var drivers = await _driverRepository.GetAllAsync();
        return _mapper.Map<List<DriverDto>>(drivers);
    }

    public async Task<DriverDto?> GetDriverByIdAsync(string id)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null)
            throw new Exception($"Водій з ідентифікатором {id} не знайдений.");
            
        return _mapper.Map<DriverDto>(driver);
    }

    public async Task CreateAsync(DriverCreateDto createDto)
    {
        var driver = _mapper.Map<Driver>(createDto);
        driver.PasswordHash = _passwordHasher.HashPassword(createDto.Password);
        driver.Role = Roles.Driver;
        driver.Status = DriverStatus.Offline;

        await _driverRepository.CreateAsync(driver);
    }

    public async Task UpdateAsync(string id, DriverUpdateDto updateDto)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null)
            throw new Exception($"Водій з ідентифікатором {id} не знайдений.");

        _mapper.Map(updateDto, driver); 
        await _driverRepository.UpdateAsync(driver);
    }

    public async Task DeleteAsync(string id)
    {
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver == null)
            throw new Exception($"Водій з ідентифікатором {id} не знайдений.");

        await _driverRepository.DeleteAsync(driver.Id);
    }
}