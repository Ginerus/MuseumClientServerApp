using Microsoft.AspNetCore.Http;

namespace MuseumServer.DTOs
{
    public class CreateDocumentRequest
    {
        public string Title { get; set; } = string.Empty;

        public IFormFile File { get; set; } = null!;

        public int? ExhibitId { get; set; }
        public int? DepartmentId { get; set; }
    }
}