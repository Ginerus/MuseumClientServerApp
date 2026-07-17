using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MuseumClient.Models
{
    public class DepartmentDetailsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public DepartmentDetailsDto? Data { get; set; }
    }

    public class DepartmentDetailsDto
    {
        [JsonPropertyName("department")]
        public DepartmentDto? Department { get; set; }
        [JsonPropertyName("exhibits")]
        public List<DepartmentExhibitInfo> Exhibits { get; set; } = new();
        [JsonPropertyName("mediaFiles")]
        public List<DepartmentMediaInfo> MediaFiles { get; set; } = new();
        [JsonPropertyName("documents")]
        public List<DepartmentDocumentInfo> Documents { get; set; } = new();
    }

    public class DepartmentExhibitInfo
    {
        [JsonPropertyName("exhibitId")]
        public int ExhibitId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class DepartmentMediaInfo
    {
        [JsonPropertyName("mediaFileId")]
        public int MediaFileId { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    public class DepartmentDocumentInfo
    {
        [JsonPropertyName("documentId")]
        public int DocumentId { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("fileType")]
        public string FileType { get; set; } = string.Empty;
    }
}