namespace MuseumClient.Models
{
    public class MediaFilesResponse
    {
        public string Status { get; set; } = string.Empty;
        public List<MediaFileDto> Data { get; set; } = new();
    }
}