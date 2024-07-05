namespace FlightDocsSystem_v3.Models
{
    public class AddUsersToFlightDto
    {
        public int FlightId { get; set; }
        public List<int> UserIds { get; set; }
        public string Role { get; set; } = null!;
    }
}
