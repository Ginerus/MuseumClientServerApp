using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MuseumClient.Models
{
    public class DocumentsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<DocumentDto> Data { get; set; } = new();
    }

    public class DocumentResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public DocumentDto Data { get; set; } = new();
    }
}