using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ЛР3_МіА._АПІшка.models;

namespace RideHailingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PassengersController : ControllerBase
{
    private readonly IPassengerService _passengerService;

    public PassengersController(IPassengerService passengerService)
    {
        _passengerService = passengerService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<PassengerDTO>>> GetAll() => Ok(await _passengerService.GetAllPassengersAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<PassengerDTO>> GetById(string id)
    {
        var passenger = await _passengerService.GetPassengerByIdAsync(id);
        return passenger == null ? NotFound("Пасажира не знайдено") : Ok(passenger);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PassengerCreateDTO createDto)
    {
        await _passengerService.CreatePassengerAsync(createDto);
        return Ok("Пасажира успішно створено.");
    }

    [Authorize(Roles = "Admin,Passenger")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] PassengerUpdateDTO updateDto)
    {
        try
        {
            await _passengerService.UpdateAsync(id, updateDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var passenger = await _passengerService.GetPassengerByIdAsync(id);
        if (passenger == null) return NotFound("Пасажира не знайдено");
        
        await _passengerService.DeletePassengerAsync(id);
        return NoContent();
    }
}