using System;
using System.Collections.Generic;

namespace FlightDocsSystem_v3.Data
{
    public partial class Document
    {
        public int DocumentId { get; set; }
        public int FlightId { get; set; }
        public string DocumentType { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool CanEdit { get; set; }
        public string FilePath { get; set; } = null!;

        public virtual User CreatedByNavigation { get; set; } = null!;
        public virtual Flight Flight { get; set; } = null!;
        public virtual User? UpdatedByNavigation { get; set; }
    }
}
