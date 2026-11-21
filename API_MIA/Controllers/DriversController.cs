using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ЛР3_МіА._АПІшка.models;

namespace RideHailingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<DriverDto>>> GetAll() => Ok(await _driverService.GetAllDriversAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<DriverDto>> GetById(string id)
    {
        try
        {
            var driver = await _driverService.GetDriverByIdAsync(id);
            return Ok(driver);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DriverCreateDto createDto)
    {
        await _driverService.CreateAsync(createDto);
        return Ok("Водія успішно створено.");
    }

    [Authorize(Roles = "Admin,Driver")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] DriverUpdateDto updateDto)
    {
        try
        {
            await _driverService.UpdateAsync(id, updateDto);
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
        try
        {
            await _driverService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}