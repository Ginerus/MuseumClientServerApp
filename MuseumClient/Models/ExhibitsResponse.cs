using System.Text.Json.Serialization;
using System.Windows.Media;

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

        public ImageSource ThumbnailImage { get; set; }
        public string DepartmentName => Department?.Name ?? "Без отдела";
    }
}