using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;

namespace FlightDocsSystem_v3.Services
{
    public class AirportService:IAirportService
    {
        private readonly FlightDocsSystemContext _context;

        public AirportService(FlightDocsSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Airport>> GetAllAirports()
        {
            return await _context.Airports.ToListAsync();
        }

        public async Task<Airport> GetAirportById(int id)
        {
            return await _context.Airports.FindAsync(id);
        }

        public async Task<Airport> CreateAirport(Airport airport)
        {
            _context.Airports.Add(airport);
            await _context.SaveChangesAsync();
            return airport;
        }

        public async Task<Airport> UpdateAirport(int id,AirportViewModel airport)
        {
            var existingAirport = await _context.Airports.FindAsync(id);
            if (existingAirport == null)
                throw new Exception("Airport not found");
            existingAirport.AirportName = airport.AirportName;
            existingAirport.RunwayCount = airport.RunwayCount;
            existingAirport.RunwayType = airport.RunwayType;
            existingAirport.IsOperational = airport.IsOperational;
            existingAirport.AirportLevel = airport.AirportLevel;
            existingAirport.Notes = airport.Notes;
            _context.Airports.Update(existingAirport);
            await _context.SaveChangesAsync();
            return existingAirport;
        }

        public async Task<bool> DeleteAirport(int id)
        {
            var airport = await _context.Airports.FindAsync(id);
            if (airport == null)
                return false;
            _context.Airports.Remove(airport);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
