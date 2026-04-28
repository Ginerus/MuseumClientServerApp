using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class ExhibitsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<ExhibitDto> Data { get; set; } = new();
    }

    public class ExhibitDto
    {
        [JsonPropertyName("exhibitId")]
        public int ExhibitId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public DepartmentDto? Department { get; set; }

        // 🔥 URL для миниатюры (формируем на клиенте)
        public string ThumbnailUrl => $"/api/Exhibit/thumbnail/{ExhibitId}";

        public string DepartmentName => Department?.Name ?? "Без отдела";
    }
}