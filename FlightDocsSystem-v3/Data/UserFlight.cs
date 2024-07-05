using System;
using System.Collections.Generic;

namespace FlightDocsSystem_v3.Data
{
    public partial class UserFlight
    {
        public int UserId { get; set; }
        public int FlightId { get; set; }

        public virtual Flight Flight { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
