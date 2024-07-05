using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace FlightDocsSystem_v3.Services
{
    public class FlightService : IFlightService
    {
        private readonly FlightDocsSystemContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        public FlightService(FlightDocsSystemContext dbContext, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Flight>> GetAllFlights(ClaimsPrincipal user)
        {
            var userRole = GetUserRole(user);

            if (userRole == "Admin"||userRole=="GO")
            {
                var flights = await _dbContext.Flights.ToListAsync();
                return await _dbContext.Flights.ToListAsync();
            }
            else if (userRole == "Pilot" || userRole == "Crew")
            {
                var userId = GetUserId(user);
                var userFlights = await _dbContext.UserFlights
                                                  .Include(uf => uf.Flight)
                                                  .Where(uf => uf.UserId == userId)
                                                  .Select(uf => uf.Flight)
                                                  .ToListAsync();

                return userFlights.Select(f => new Flight
                {
                    FlightId = f.FlightId,
                    DepatureDate = f.DepatureDate,
                    AircraftType = f.AircraftType,
                    Status = f.Status,
                    DepartureAirportId = f.DepartureAirportId,
                    ArrivalAirportId = f.ArrivalAirportId,
                });
            }
            else
                throw new UnauthorizedAccessException("Access denied");
        }

        public async Task<Flight> GetFlightById(int id, ClaimsPrincipal user)
        {
            var userRole = GetUserRole(user);
            if (userRole == "Admin")
            {
                var flight = await _dbContext.Flights
                                          .Include(f => f.UserFlights)
                                          .ThenInclude(uf => uf.User)
                                          .FirstOrDefaultAsync(f => f.FlightId == id);
                if (flight == null)
                    return null;
                return flight;
            }
            else if (userRole == "Pilot" || userRole == "Crew")
            {
                var userId = GetUserId(user);
                var userFlight = await _dbContext.UserFlights
                                                  .Include(uf => uf.Flight)
                                                  .ThenInclude(f => f.UserFlights)
                                                  .ThenInclude(uf => uf.User)
                                                  .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.FlightId == id);
                if (userFlight == null)
                    return null;
                var flight = new Flight
                {
                    FlightId = userFlight.Flight.FlightId,
                    DepatureDate = userFlight.Flight.DepatureDate,
                    AircraftType = userFlight.Flight.AircraftType,
                    Status = userFlight.Flight.Status,
                    UserFlights = userFlight.Flight.UserFlights.Select(uf => new UserFlight
                    {
                        UserId = uf.UserId,
                        User = new User
                        {
                            UserId = uf.User.UserId,
                            Username = uf.User.Username,
                            Role = uf.User.Role
                        }
                    }).ToList()
                };
                return flight;
            }
            else
                throw new UnauthorizedAccessException("Access denied");
        }

        public async Task<Flight> CreateFlight(Flight flight)
        {
            flight.Status = "Chưa khởi hành";
            _dbContext.Flights.Add(flight);
            await _dbContext.SaveChangesAsync();
            return flight;
        }
        public async Task<Flight> UpdateFlight(int id, FlightViewModel model)
        {
            var flight = await _dbContext.Flights.FindAsync(id);
            if (flight == null)
                throw new Exception("Flight not found");
            if (flight.Status == "Đã khởi hành" || flight.Status == "Đã hạ cánh")
                throw new Exception("Cannot edit the flight because it has already landed");
            flight.DepatureDate = model.DepatureDate;
            _dbContext.Flights.Update(flight);
            return flight;
        }
        public async Task<bool> UpdateStatusFlight(int id)
        {
            var flight = await _dbContext.Flights
                                 .Include(f => f.UserFlights)
                                 .ThenInclude(uf => uf.User)
                                 .FirstOrDefaultAsync(f => f.FlightId == id);
            if (flight == null)
                throw new Exception("Flight not found");
            switch (flight.Status)
            {
                case "Chưa khởi hành":
                    flight.Status = "Đã khởi hành";
                    break;
                case "Đã khởi hành":
                    flight.Status = "Đã hạ cánh";
                    foreach (var userFlight in flight.UserFlights)
                    {
                        userFlight.User.Role = null;
                    }
                    break;
                case "Đã hạ cánh":
                    throw new Exception("Cannot update status because the flight has already landed");
                default:
                    throw new Exception("Unknown flight status");
            }
            _dbContext.Flights.Update(flight);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteFlight(int id)
        {
            var flight = await _dbContext.Flights.FindAsync(id);
            if (flight == null)
                throw new Exception("Flight not found");
            if (flight.Status == "Đã khởi hành" || flight.Status == "Đã hạ cánh")
                throw new Exception("Cannot delete the flight because it has already departed or landed");
            _dbContext.Flights.Remove(flight);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task AddUsersToFlight(AddUsersToFlightDto dto)
        {
            var flight = await _dbContext.Set<Flight>()
                                       .Include(f => f.UserFlights)
                                       .FirstOrDefaultAsync(f => f.FlightId == dto.FlightId);
            if (flight == null)
                throw new Exception("Flight not found");
            if (flight.Status == "Đã khởi hành" || flight.Status == "Đã hạ cánh")
                throw new Exception("Cannot add users to the flight because it has already departed or landed");
            foreach (var userId in dto.UserIds)
            {
                var user = await _dbContext.Set<User>().FindAsync(userId);
                if (user == null)
                    throw new Exception($"User with ID {userId} not found");
                user.Role = dto.Role;
                var userFlight = new UserFlight
                {
                    FlightId = dto.FlightId,
                    UserId = userId
                };
                _dbContext.Set<UserFlight>().Add(userFlight);
            }
            await _dbContext.SaveChangesAsync();
        }
        private string GetUserRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        private int GetUserId(ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        public async Task<IEnumerable<Flight>> SearchFlightsByFlightId(int flightId)
        {
            var flights = await _dbContext.Flights
                .Where(f => f.FlightId == flightId)
                .ToListAsync();
            return flights;
        }
        public async Task<IEnumerable<Flight>> SearchFlightsByDate(DateTime departureDate)
        {
            var flights = await _dbContext.Flights
                .Where(f => f.DepatureDate.Date == departureDate.Date)
                .ToListAsync();
            return flights;
        }
        public async Task<IEnumerable<Document>> SearchDocumentsByFlightIdAndName(int id, string documentName)
        {
            var documents = await _dbContext.Documents
                .Include(d => d.Flight)
                .Where(d => d.FlightId == id && d.Title.Contains(documentName))
                .ToListAsync();
            return documents;
        }
    }
}
