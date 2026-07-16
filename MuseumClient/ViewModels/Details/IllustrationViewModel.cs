using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

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

        private bool _isFullscreen = true;
        public bool IsFullscreen
        {
            get => _isFullscreen;
            set
            {
                _isFullscreen = value;

                OnPropertyChanged(nameof(IsFullscreen));
                OnPropertyChanged(nameof(FullscreenMargin));
            }
        }

        public Thickness FullscreenMargin
        {
            get
            {
                return IsFullscreen
                    ? new Thickness(0)
                    : new Thickness(20, 0, 20, 20);
            }
        }

        public RelayCommand DownloadCommand { get; }
        public RelayCommand FullscreenCommand { get; }

        public IllustrationViewModel(int id)
        {
            _id = id;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            DownloadCommand = new RelayCommand(async _ => await DownloadAsync());

            FullscreenCommand = new RelayCommand(async _ =>
            {
                IsFullscreen = !IsFullscreen;
                await Task.CompletedTask;
            });

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


                var bytesTask = _apiService.GetBytesAsync($"MediaFile/stream/{_id}");

                var completed = await Task.WhenAny(
                    bytesTask,
                    Task.Delay(10000)
                );

                if (completed != bytesTask)
                {
                    throw new Exception("Превышено время загрузки изображения");
                }


                var bytes = await bytesTask;


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
            catch (Exception ex)
            {
                Title = "Ошибка загрузки";
                Description = ex.Message;

                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Description));
            }
            finally
            {
                IsLoading = false;
            }
        }
        private async Task DownloadAsync()
        {
            try
            {
                var bytes = await _apiService.GetBytesAsync($"MediaFile/stream/{_id}");

                var dialog = new SaveFileDialog
                {
                    FileName = $"{Title}_{_id}",
                    Filter = "Image files (*.jpg)|*.jpg|PNG (*.png)|*.png|All files (*.*)|*.*",
                    DefaultExt = ".jpg",
                    AddExtension = true
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(dialog.FileName, bytes);
                }
            }
            catch
            {
                // можно потом заменить на нормальный UI toast
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}