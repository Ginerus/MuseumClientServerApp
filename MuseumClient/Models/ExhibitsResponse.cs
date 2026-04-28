using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MuseumClient.Models
{
    public class ExhibitsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<ExhibitDto> Data { get; set; } = new();
    }

    public class ExhibitDto : INotifyPropertyChanged
    {
        [JsonPropertyName("exhibitId")]
        public int ExhibitId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("department")]
        public DepartmentDto? Department { get; set; }

        public string DepartmentName => Department?.Name ?? "Без отдела";

        private ImageSource? _thumbnailImage;

        public ImageSource? ThumbnailImage
        {
            get => _thumbnailImage;
            set
            {
                _thumbnailImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThumbnailImage)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}