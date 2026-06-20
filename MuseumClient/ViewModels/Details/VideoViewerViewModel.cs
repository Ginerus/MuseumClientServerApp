using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace MuseumClient.ViewModels.Details
{
    public class VideoViewerViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private readonly int _id;

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string DepartmentName { get; set; } = "";

        private string? _videoPath;
        public string? VideoPath
        {
            get => _videoPath;
            set
            {
                _videoPath = value;
                OnPropertyChanged(nameof(VideoPath));
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

        public VideoViewerViewModel(int id)
        {
            _id = id;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            DownloadCommand = new RelayCommand(async _ => await DownloadAsync());

            _ = LoadVideoAsync();
        }

        public async Task LoadVideoAsync()
        {
            try
            {
                IsLoading = true;

                var response =
                    await _apiService.GetAsync<MediaFileSingleResponse>(
                        $"MediaFile/{_id}");

                if (response?.Data != null)
                {
                    Title = response.Data.Title;
                    Description = response.Data.Description;
                    DepartmentName = response.Data.Department?.Name ?? "";

                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(DepartmentName));
                }

                var bytesTask =
                    _apiService.GetBytesAsync(
                        $"MediaFile/stream/{_id}?size=video");

                var completed = await Task.WhenAny(
                    bytesTask,
                    Task.Delay(15000));

                if (completed != bytesTask)
                {
                    throw new Exception("Превышено время загрузки видео");
                }

                var bytes = await bytesTask;

                var tempPath = Path.Combine(
                    Path.GetTempPath(),
                    $"museum_video_{_id}.mp4");

                File.WriteAllBytes(tempPath, bytes);

                VideoPath = tempPath;
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
                var bytes =
                    await _apiService.GetBytesAsync(
                        $"MediaFile/stream/{_id}?size=video");

                var dialog = new SaveFileDialog
                {
                    FileName = $"{Title}_{_id}",
                    Filter = "MP4 (*.mp4)|*.mp4|All files (*.*)|*.*",
                    DefaultExt = ".mp4",
                    AddExtension = true
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(dialog.FileName, bytes);
                }
            }
            catch
            {
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}