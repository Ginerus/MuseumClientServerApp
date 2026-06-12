using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MuseumClient.Models
{
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

        // Для группировки
        public string DepartmentName => Department?.Name ?? "Без отдела";
    }
}
