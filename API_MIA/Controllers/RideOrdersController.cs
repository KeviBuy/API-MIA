namespace ЛР3_МіА._АПІшка.Controllers
{
    using FluentValidation;
    using Microsoft.AspNetCore.Mvc;
    using ЛР3_МіА._АПІшка.models;
    using Microsoft.AspNetCore.Authorization;


    [ApiController]
    [Route("api/[controller]")]
    public class RideOrdersController : ControllerBase
    {
        private readonly IValidator<RideOrder> _validator;

        private readonly IRideOrderService _rideOrderService;

        public RideOrdersController(IValidator<RideOrder> validator, IRideOrderService rideOrderService)
        {
            _validator = validator;
            _rideOrderService = rideOrderService;
        }

        [Authorize(Roles = "Admin,Driver")]
        [HttpGet]
        public async Task<ActionResult> GetAll() => Ok(await _rideOrderService.GetAllAsync());

        [Authorize(Roles = "Admin,Driver,Passenger")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(string id)
        {
            var order = await _rideOrderService.GetByIdAsync(id);
            return order == null ? NotFound("Замовлення не знайдено") : Ok(order);
        }

        //Створення замовлення та радіальний пошук найближчого водія
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] RideOrder order)
        {
            var validation = await _validator.ValidateAsync(order);
            if (!validation.IsValid)
             return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

            try
            {
                var result = await _rideOrderService.CreateAsync(order);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Прийняття поїздки водієм та навігація до клієнта
        [Authorize(Roles = "Admin,Driver")]
        [HttpPost("{orderId}/accept")]
        public async Task<IActionResult> AcceptRide(string orderId, [FromQuery] string driverId)
        {
            try
            {
                var result = await _rideOrderService.AcceptRideOrderAsync(orderId, driverId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Driver")]
        [HttpPost("{orderId}/arrived")]
        public async Task<IActionResult> DriverArrived(string orderId)
        {
            try
            {
                var result = await _rideOrderService.DriverArrivedAsync(orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //Завершення поїздки або компенсація за скасування
        [Authorize(Roles = "Admin,Driver")]
        [HttpPost("{orderId}/complete")]
        public async Task<IActionResult> CompleteRide(string orderId)
        {
            try
            {
                var result = await _rideOrderService.CompleteRideOrderAsync(orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Passenger,Driver")]
        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelRide(string orderId, [FromQuery] bool cancelledByPassenger)
        {
            if(User.IsInRole("Passenger"))
                cancelledByPassenger = true;
            try
            {
                var result = await _rideOrderService.CancelRideOrderAsync(orderId, cancelledByPassenger);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Passenger")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RideOrder updatedRideOrder)
        {
            try
            {
                await _rideOrderService.UpdateAsync(id, updatedRideOrder);
                return Ok(updatedRideOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _rideOrderService.DeleteAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
