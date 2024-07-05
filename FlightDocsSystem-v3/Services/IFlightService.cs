using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using System.Security.Claims;

namespace FlightDocsSystem_v3.Services
{
    public interface IFlightService
    {
        Task<IEnumerable<Flight>> GetAllFlights(ClaimsPrincipal user);
        Task<Flight> GetFlightById(int id, ClaimsPrincipal user);
        Task<Flight> CreateFlight(Flight flight);
        Task<Flight> UpdateFlight(int id, FlightViewModel model);
        Task<bool> UpdateStatusFlight(int id);
        Task<bool> DeleteFlight(int id);
        Task AddUsersToFlight(AddUsersToFlightDto dto);
        Task<IEnumerable<Flight>> SearchFlightsByFlightId(int id);
        Task<IEnumerable<Flight>> SearchFlightsByDate(DateTime departureDate);
        Task<IEnumerable<Document>> SearchDocumentsByFlightIdAndName(int id, string documentName);
    }
}
