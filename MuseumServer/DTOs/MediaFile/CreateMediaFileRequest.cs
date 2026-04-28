namespace MuseumServer.DTOs
{
    public class CreateMediaFileRequest
    {
        public string Title { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
    }
}