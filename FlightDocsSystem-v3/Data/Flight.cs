using System;
using System.Collections.Generic;

namespace FlightDocsSystem_v3.Data
{
    public partial class Flight
    {
        public Flight()
        {
            Documents = new HashSet<Document>();
            UserFlights = new HashSet<UserFlight>();
        }

        public int FlightId { get; set; }
        public DateTime DepatureDate { get; set; }
        public string AircraftType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }

        public virtual Airport? ArrivalAirport { get; set; } = null!;
        public virtual Airport? DepartureAirport { get; set; } = null!;
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<UserFlight> UserFlights { get; set; }
    }
}
