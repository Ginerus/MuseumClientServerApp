using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class MediaFileSingleResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("data")]
        public MediaFileDetailDto? Data { get; set; }
    }

    public class MediaFileDetailDto
    {
        [JsonPropertyName("mediaFileId")]
        public int MediaFileId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("department")]
        public DepartmentDto? Department { get; set; }
    }

    public class DepartmentDto
    {
        [JsonPropertyName("departmentId")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}