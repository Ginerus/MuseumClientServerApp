using MuseumServer.Models;
using System.Text.Json.Serialization;

namespace MuseumServer.DTOs
{
    public class ExhibitFullResponse
    {
        public int ExhibitId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Materials { get; set; }
        public bool IsPermanent { get; set; } = true;

        // Минимальная информация об отделе
        public DepartmentInfo? Department { get; set; }
    }
}
