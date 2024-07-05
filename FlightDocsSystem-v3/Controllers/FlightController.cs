using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using FlightDocsSystem_v3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightDocsSystem_v3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightController(IFlightService flightService)
        {
            _flightService = flightService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFlights()
        {
            try
            {
                var user = HttpContext.User;
                var flights = await _flightService.GetAllFlights(user);
                return Ok(flights);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(int id)
        {
            try
            {
                var user = HttpContext.User;
                var flight = await _flightService.GetFlightById(id, user);
                if (flight == null)
                    return NotFound();
                return Ok(flight);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpPost("create-flight")]
        public async Task<IActionResult> CreateFlight(Flight flight)
        {
            try
            {
                var createdFlight = await _flightService.CreateFlight(flight);
                return Ok(createdFlight);
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return BadRequest(new { message = ex.Message, innerException = innerExceptionMessage });
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpPut("update-flight/{id}")]
        public async Task<IActionResult> UpdateFlight(int id,FlightViewModel model)
        {
            try
            {
                var updatedFlight = await _flightService.UpdateFlight(id, model);
                return Ok(updatedFlight);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpPut("update-status-flight/{id}")]
        public async Task<IActionResult> UpdateStatusFlight(int id)
        {
            try
            {
                var updatedFlight = await _flightService.UpdateStatusFlight(id);
                return Ok(updatedFlight);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpDelete("delete-flight/{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            try
            {
                var result = await _flightService.DeleteFlight(id);
                return Ok(new { success = result, message = "Flight deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpPost("add-users-to-flight")]
        public async Task<IActionResult> AddUsersToFlight(AddUsersToFlightDto dto)
        {
            try
            {
                await _flightService.AddUsersToFlight(dto);
                return Ok(new { message = "Users added to flight successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
