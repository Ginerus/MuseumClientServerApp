namespace MuseumServer.DTOs
{
    public class CreateMediaFileRequest
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
    }
}