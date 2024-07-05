using System.ComponentModel.DataAnnotations;

namespace FlightDocsSystem_v3.Models
{
    public class DocumentCreateDto
    {
        public string DocumentType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public IFormFile File { get; set; }
    }
}
