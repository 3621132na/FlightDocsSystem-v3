namespace FlightDocsSystem_v3.Models
{
    public class AirportViewModel
    {
        public string AirportName { get; set; }
        public int RunwayCount { get; set; }
        public string RunwayType { get; set; }
        public bool IsOperational { get; set; }
        public string AirportLevel { get; set; }
        public string? Notes { get; set; }
    }
}
