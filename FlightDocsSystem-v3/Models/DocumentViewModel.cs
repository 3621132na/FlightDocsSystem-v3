namespace FlightDocsSystem_v3.Models
{
    public class DocumentViewModel
    {
        public string DocumentType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FilePath { get; set; }
    }
}
