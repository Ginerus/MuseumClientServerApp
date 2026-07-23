using System.ComponentModel.DataAnnotations;

namespace MuseumServer.Models
{
    public class MuseumInfo
    {
        [Key]
        public int MuseumInfoId { get; set; }
        public string? Description { get; set; }
        public string AdminPasswordHash { get; set; } = null!;
    }
}