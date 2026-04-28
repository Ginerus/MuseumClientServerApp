using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace MuseumClient.Models
{
    public class DepartmentsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<DepartmentDto> Data { get; set; } = new();
    }
}