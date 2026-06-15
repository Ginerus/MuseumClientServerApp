using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MuseumClient.Models
{
    public class ExhibitResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public ExhibitDetailsDto? Data { get; set; }
    }

    public class ExhibitDetailsDto
    {
        [JsonPropertyName("exhibitId")]
        public int ExhibitId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("materials")]
        public string Materials { get; set; } = string.Empty;

        [JsonPropertyName("isPermanent")]
        public bool IsPermanent { get; set; }

        [JsonPropertyName("department")]
        public DepartmentDto? Department { get; set; }

        public string DepartmentName => Department?.Name ?? "Без отдела";
    }
}
