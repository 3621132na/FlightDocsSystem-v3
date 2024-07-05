using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;

namespace FlightDocsSystem_v3.Services
{
    public interface IAirportService
    {
        Task<IEnumerable<Airport>> GetAllAirports();
        Task<Airport> GetAirportById(int id);
        Task<Airport> CreateAirport(Airport airport);
        Task<Airport> UpdateAirport(int id, AirportViewModel airport);
        Task<bool> DeleteAirport(int id);
    }
}
