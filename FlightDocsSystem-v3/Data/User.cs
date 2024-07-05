using System;
using System.Collections.Generic;

namespace FlightDocsSystem_v3.Data
{
    public partial class User
    {
        public User()
        {
            DocumentCreatedByNavigations = new HashSet<Document>();
            DocumentUpdatedByNavigations = new HashSet<Document>();
            UserFlights = new HashSet<UserFlight>();
        }

        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Role { get; set; }

        public virtual ICollection<Document> DocumentCreatedByNavigations { get; set; }
        public virtual ICollection<Document> DocumentUpdatedByNavigations { get; set; }
        public virtual ICollection<UserFlight> UserFlights { get; set; }
    }
}
