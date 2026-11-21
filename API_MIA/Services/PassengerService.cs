using AutoMapper;
using ЛР3_МіА._АПІшка.models;

public interface IPassengerService
{
    Task<List<PassengerDTO>> GetAllPassengersAsync();
    Task<PassengerDTO?> GetPassengerByIdAsync(string id);
    Task CreatePassengerAsync(PassengerCreateDTO createDto);
    Task UpdateAsync(string id, PassengerUpdateDTO updateDto);
    Task DeletePassengerAsync(string id);
}

public class PassengerService : IPassengerService
{
    private readonly IPassengerRepository _passengerRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public PassengerService(IPassengerRepository passengerRepository, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _passengerRepository = passengerRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<PassengerDTO>> GetAllPassengersAsync()
    {
        var passengers = await _passengerRepository.GetAllAsync();
        return _mapper.Map<List<PassengerDTO>>(passengers);
    }

    public async Task<PassengerDTO?> GetPassengerByIdAsync(string id)
    {
        var passenger = await _passengerRepository.GetByIdAsync(id);
        return passenger == null ? null : _mapper.Map<PassengerDTO>(passenger);
    }

    public async Task CreatePassengerAsync(PassengerCreateDTO createDto)
    {
        var passenger = _mapper.Map<Passenger>(createDto);
        passenger.PasswordHash = _passwordHasher.HashPassword(createDto.Password);
        passenger.Role = Roles.Passenger;
        passenger.Balance = createDto.Balance;

        await _passengerRepository.CreateAsync(passenger);
    }

    public async Task UpdateAsync(string id, PassengerUpdateDTO updateDto)
    {
        var passenger = await _passengerRepository.GetByIdAsync(id);
        if (passenger == null)
            throw new Exception($"Пасажир з ідентифікатором {id} не знайдений.");

        _mapper.Map(updateDto, passenger); 
        await _passengerRepository.UpdateAsync(passenger);
    }

    public async Task DeletePassengerAsync(string id) =>
        await _passengerRepository.DeleteAsync(id);
}