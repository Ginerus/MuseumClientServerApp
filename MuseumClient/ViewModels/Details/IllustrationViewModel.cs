using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels.Details
{
    public class IllustrationViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private int _id;

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string DepartmentName { get; set; } = "";

        private BitmapImage? _image;
        public BitmapImage? Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public RelayCommand DownloadCommand { get; }

        public IllustrationViewModel(int id)
        {
            _id = id;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            DownloadCommand = new RelayCommand(async _ => await DownloadAsync());

            _ = LoadIllustrationAsync();
        }

        public async Task LoadIllustrationAsync()
        {
            try
            {
                IsLoading = true;

                var response = await _apiService.GetAsync<MediaFileSingleResponse>($"MediaFile/{_id}");

                if (response?.Data != null)
                {
                    Title = response.Data.Title;
                    Description = response.Data.Description;
                    DepartmentName = response.Data.Department?.Name ?? "";

                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(DepartmentName));
                }

                var bytes = await _apiService.GetBytesAsync($"MediaFile/stream/{_id}");

                var image = new BitmapImage();

                using (var ms = new MemoryStream(bytes))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();
                }

                Image = image;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DownloadAsync()
        {
            var bytes = await _apiService.GetBytesAsync($"MediaFile/stream/{_id}");

            var fileName = $"{Title}_{_id}.jpg";
            File.WriteAllBytes(fileName, bytes);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}