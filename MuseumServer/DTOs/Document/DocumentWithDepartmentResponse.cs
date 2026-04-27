namespace MuseumServer.DTOs
{
    public class DocumentWithDepartmentResponse
    {
        public int DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;

        public DepartmentInfo? Department { get; set; }
    }
}
