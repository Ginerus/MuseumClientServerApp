using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class DepartmentResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public DepartmentDto? Data { get; set; }
    }
}