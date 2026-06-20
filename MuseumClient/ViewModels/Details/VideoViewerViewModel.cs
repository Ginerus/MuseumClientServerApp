using LibVLCSharp.Shared;
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


        private LibVLC _libVLC;

        private Media? _media;

        public MediaPlayer MediaPlayer { get; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string DepartmentName { get; set; } = "";

        private int _volume = 100;

        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;

                if (MediaPlayer != null)
                    MediaPlayer.Volume = value;

                OnPropertyChanged(nameof(Volume));
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

        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public double Position
        {
            get => MediaPlayer.Position * 100;
            set
            {
                MediaPlayer.Position = (float)(value / 100);
                OnPropertyChanged(nameof(Position));
            }
        }

        public RelayCommand TogglePlayCommand { get; }

        public RelayCommand DownloadCommand { get; }

        public VideoViewerViewModel(int id)
        {
            _id = id;

            var config = new ConfigService().Server;

            _apiService =
                new ApiService(
                    config,
                    AuthService.Instance());

            Core.Initialize();

            _libVLC = new LibVLC();

            MediaPlayer =
                new MediaPlayer(_libVLC);

            DownloadCommand =
                new RelayCommand(async _ =>
                    await DownloadAsync());

            TogglePlayCommand =
                new RelayCommand(async _ =>
                {
                    if (MediaPlayer.IsPlaying)
                    {
                        await Task.Run(() =>
                        {
                            MediaPlayer.Pause();
                        });
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            MediaPlayer.Play();
                        });
                    }

                    IsPlaying = MediaPlayer.IsPlaying;
                    OnPropertyChanged(nameof(IsPlaying));
                });

            _ = LoadVideoAsync();

        }

        public async Task LoadVideoAsync()
        {
            try
            {
                IsLoading = true;

                var response =
                    await _apiService
                    .GetAsync<MediaFileSingleResponse>(
                        $"MediaFile/{_id}");

                if (response?.Data != null)
                {
                    Title = response.Data.Title;
                    Description = response.Data.Description;
                    DepartmentName =
                        response.Data.Department?.Name ?? "";

                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(DepartmentName));
                }

                var bytes =
                    await _apiService.GetBytesAsync(
                        $"MediaFile/stream/{_id}?size=video");

                var path =
                    Path.Combine(
                        Path.GetTempPath(),
                        $"museum_{_id}.mp4");

                File.WriteAllBytes(
                    path,
                    bytes);

                _media =
                    new Media(
                        _libVLC,
                        path,
                        FromType.FromPath);

                MediaPlayer.Play(_media);

                MediaPlayer.PositionChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(Position));
                };

                MediaPlayer.Playing += (s, e) =>
                {
                    IsPlaying = true;
                    OnPropertyChanged(nameof(IsPlaying));
                };

                MediaPlayer.Paused += (s, e) =>
                {
                    IsPlaying = false;
                    OnPropertyChanged(nameof(IsPlaying));
                };

            }
            catch (Exception ex)
            {
                Description = ex.Message;

                OnPropertyChanged(nameof(Description));
            }
            finally
            {
                IsLoading = false;
            }

        }

        private async Task DownloadAsync()
        {
            var bytes =
                await _apiService.GetBytesAsync(
                    $"MediaFile/stream/{_id}?size=video");

            var dialog =
                new SaveFileDialog
                {
                    FileName = $"{Title}.mp4",
                    Filter = "MP4 (*.mp4)|*.mp4"
                };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllBytes(
                    dialog.FileName,
                    bytes);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            MediaPlayer.Stop();
            _media?.Dispose();
            MediaPlayer.Dispose();
            _libVLC.Dispose();
        }

    }
}