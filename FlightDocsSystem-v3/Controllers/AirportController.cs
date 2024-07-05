using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using FlightDocsSystem_v3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightDocsSystem_v3.Controllers
{
    [Authorize(Roles = "Admin, GO")]
    [Route("api/[controller]")]
    [ApiController]
    public class AirportController : ControllerBase
    {
        private readonly IAirportService _airportService;

        public AirportController(IAirportService airportService)
        {
            _airportService = airportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAirports()
        {
            var airports = await _airportService.GetAllAirports();
            return Ok(airports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAirportById(int id)
        {
            var airport = await _airportService.GetAirportById(id);
            if (airport == null)
                return NotFound();
            return Ok(airport);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAirport(Airport airport)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var createdAirport = await _airportService.CreateAirport(airport);
            return Ok(createdAirport);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAirport(int id, AirportViewModel airport)
        {
            try
            {
                var updatedAirport = await _airportService.UpdateAirport(id, airport);
                return Ok(updatedAirport);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirport(int id)
        {
            try
            {
                var result = await _airportService.DeleteAirport(id);
                if (result)
                    return Ok(new { message = "Airport deleted successfully." });
                else
                    return NotFound(new { message = "Airport not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
