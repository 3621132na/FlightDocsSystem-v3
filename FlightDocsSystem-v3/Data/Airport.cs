using System;
using System.Collections.Generic;

namespace FlightDocsSystem_v3.Data
{
    public partial class Airport
    {
        public Airport()
        {
            FlightArrivalAirports = new HashSet<Flight>();
            FlightDepartureAirports = new HashSet<Flight>();
        }

        public int AirportId { get; set; }
        public string AirportName { get; set; } = null!;
        public string AirportCode { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int RunwayCount { get; set; }
        public string RunwayType { get; set; } = null!;
        public bool IsOperational { get; set; }
        public string AirportLevel { get; set; } = null!;
        public string? Notes { get; set; }

        public virtual ICollection<Flight> FlightArrivalAirports { get; set; }
        public virtual ICollection<Flight> FlightDepartureAirports { get; set; }
    }
}
