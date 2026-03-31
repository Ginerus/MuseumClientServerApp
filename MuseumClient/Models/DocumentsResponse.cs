using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace MuseumClient.Models
{
    public class DocumentsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<DocumentDto> Data { get; set; } = new();
    }

    public class DocumentDto
    {
        [JsonPropertyName("documentId")]
        public int DocumentId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("fileType")]
        public string FileType { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public DepartmentDto? Department { get; set; }

        // 🔥 Для группировки
        public string DepartmentName => Department?.Name ?? "Без отдела";
    }

    public class DepartmentDto
    {
        [JsonPropertyName("departmentId")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}