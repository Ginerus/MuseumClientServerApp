using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MuseumClient.Models
{
    public class MediaFileDto : INotifyPropertyChanged
    {
        [JsonPropertyName("mediaFileId")]
        public int MediaFileId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; } = string.Empty;

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

        private string? _previewTempPath;

        public string? PreviewTempPath
        {
            get => _previewTempPath;
            set
            {
                _previewTempPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewTempPath)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}