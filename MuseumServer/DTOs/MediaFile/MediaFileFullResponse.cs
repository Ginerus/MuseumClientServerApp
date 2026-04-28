namespace MuseumServer.DTOs
{
    public class MediaFileFullResponse
    {
        public int MediaFileId { get; set; } // Автоинкремент
        public string Title { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public string ?Description {  get; set; } = string.Empty;

        // Минимальная информация об отделе
        public DepartmentInfo? Department { get; set; }
    }
}
