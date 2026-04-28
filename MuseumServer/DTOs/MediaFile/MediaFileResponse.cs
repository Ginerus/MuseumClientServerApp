namespace MuseumServer.DTOs
{
    public class MediaFileResponse
    {
        public int MediaFileId { get; set; } // Автоинкремент
        public string MediaType { get; set; } = string.Empty;
    }
}