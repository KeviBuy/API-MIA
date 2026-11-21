using System.Security.Claims;
using ЛР3_МіА._АПІшка.models;

public interface IAuthService
{
    Task<object?> LoginAsync(string email, string password);
    Task<object?> RefreshTokenAsync(string accessToken, string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IPassengerRepository _passengerRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IPassengerRepository passengerRepository, 
        IDriverRepository driverRepository, 
        IPasswordHasher passwordHasher, 
        JwtTokenGenerator jwtTokenGenerator)
    {
        _passengerRepository = passengerRepository;
        _driverRepository = driverRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<object?> LoginAsync(string email, string password)
    {
        var passengers = await _passengerRepository.GetAllAsync();
        var passenger = passengers.FirstOrDefault(p => p.Email == email);

        if (passenger != null && _passwordHasher.Verify(password, passenger.PasswordHash))
        {
            var token = _jwtTokenGenerator.GenerateToken(passenger.Id, passenger.Email, passenger.Role);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            passenger.RefreshToken = refreshToken;
            passenger.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _passengerRepository.UpdateAsync(passenger);

            return new { Token = token, RefreshToken = refreshToken, Role = Roles.Passenger.ToString() };
        }


        var drivers = await _driverRepository.GetAllAsync();
        var driver = drivers.FirstOrDefault(d => d.Email == email);

        if (driver != null && _passwordHasher.Verify(password, driver.PasswordHash))
        {
            var token = _jwtTokenGenerator.GenerateToken(driver.Id, driver.Email, driver.Role);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            driver.RefreshToken = refreshToken;
            driver.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _driverRepository.UpdateAsync(driver);

            return new { Token = token, RefreshToken = refreshToken, Role = Roles.Driver.ToString() };
        }

        return null;
    }

    public async Task<object?> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return null;

        var id = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (id == null) return null;

        
        var passenger = await _passengerRepository.GetByIdAsync(id);
        if (passenger != null && passenger.RefreshToken == refreshToken && passenger.RefreshTokenExpiryTime > DateTime.UtcNow)
        {
            var newAccessToken = _jwtTokenGenerator.GenerateToken(passenger.Id, passenger.Email, passenger.Role);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            passenger.RefreshToken = newRefreshToken;
            passenger.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _passengerRepository.UpdateAsync(passenger);

            return new { Token = newAccessToken, RefreshToken = newRefreshToken };
        }

        
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver != null && driver.RefreshToken == refreshToken && driver.RefreshTokenExpiryTime > DateTime.UtcNow)
        {
            var newAccessToken = _jwtTokenGenerator.GenerateToken(driver.Id, driver.Email, driver.Role);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            driver.RefreshToken = newRefreshToken;
            driver.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _driverRepository.UpdateAsync(driver);

            return new { Token = newAccessToken, RefreshToken = newRefreshToken };
        }

        return null;
    }
}