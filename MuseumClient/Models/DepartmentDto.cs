using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MuseumClient.Models
{
    public class DepartmentDto : INotifyPropertyChanged
    {
        [JsonPropertyName("departmentId")]
        public int DepartmentId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

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
