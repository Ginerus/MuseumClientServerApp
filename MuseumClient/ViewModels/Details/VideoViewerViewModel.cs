using LibVLCSharp.Shared;
using Microsoft.Win32;
using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MuseumClient.ViewModels.Details
{
    public class VideoViewerViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ApiService _apiService;
        private readonly int _id;

        private readonly LibVLC _libVLC;
        private Media? _media;

        public LibVLCSharp.Shared.MediaPlayer MediaPlayer { get; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string DepartmentName { get; set; } = "";


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

        private CancellationTokenSource? _volumeCts;

        private int _volume = 100;

        public int Volume
        {
            get => _volume;
            set
            {
                if (value < 0)
                    value = 0;

                if (value > 100)
                    value = 100;


                _volume = value;

                OnPropertyChanged(nameof(Volume));
                OnPropertyChanged(nameof(VolumeIcon));


                _volumeCts?.Cancel();

                _volumeCts = new CancellationTokenSource();

                var token = _volumeCts.Token;


                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(80, token);


                        if (token.IsCancellationRequested)
                            return;


                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (MediaPlayer == null)
                                return;


                            MediaPlayer.Volume = value;


                            if (value > 0 && MediaPlayer.Mute)
                            {
                                MediaPlayer.Mute = false;

                                _muted = false;

                                OnPropertyChanged(nameof(Muted));
                                OnPropertyChanged(nameof(VolumeIcon));
                            }
                        });

                    }
                    catch
                    {

                    }

                }, token);
            }
        }

        private bool _muted;

        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;

                if (MediaPlayer != null)
                {
                    MediaPlayer.Mute = value;
                }

                OnPropertyChanged(nameof(Muted));
                OnPropertyChanged(nameof(VolumeIcon));
            }
        }

        public string VolumeIcon
        {
            get
            {
                if (Muted || Volume == 0)
                    return "🔇";

                if (Volume <= 50)
                    return "🔉";

                return "🔊";
            }
        }

        private bool _isSeeking;

        public RelayCommand SeekStartCommand { get; }
        public RelayCommand SeekEndCommand { get; }

        public double Position
        {
            get
            {
                if (MediaPlayer.Length <= 0)
                    return 0;

                return MediaPlayer.Position * 100;
            }
            set
            {
                MediaPlayer.Position =
                    (float)(value / 100);

                OnPropertyChanged(nameof(Position));
            }
        }
        public string CurrentTime
        {
            get
            {
                return TimeSpan
                    .FromMilliseconds(MediaPlayer.Time)
                    .ToString(@"mm\:ss");
            }
        }
        public string Duration
        {
            get
            {
                return TimeSpan
                    .FromMilliseconds(MediaPlayer.Length)
                    .ToString(@"mm\:ss");
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


        public RelayCommand TogglePlayCommand { get; }

        public RelayCommand DownloadCommand { get; }

        public RelayCommand MuteCommand { get; }

        public RelayCommand FullscreenCommand { get; }

        public VideoViewerViewModel(int id)
        {
            _id = id;

            var config =
                new ConfigService()
                .Server;

            _apiService =
                new ApiService(
                    config,
                    AuthService.Instance());

            Core.Initialize();


            _libVLC =
                new LibVLC();

            MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

            Volume = 100;

            TogglePlayCommand =
                new RelayCommand(async _ =>
                {

                    if (MediaPlayer.IsPlaying)
                    {
                        MediaPlayer.Pause();
                    }
                    else
                    {
                        MediaPlayer.Play();
                    }

                    await Task.CompletedTask;

                });

            MuteCommand =
                new RelayCommand(async _ =>
                {

                    Muted = !Muted;

                    await Task.CompletedTask;

                });



            FullscreenCommand =
                new RelayCommand(async _ =>
                {

                    IsFullscreen = !IsFullscreen;

                    await Task.CompletedTask;

                });

            DownloadCommand =
                new RelayCommand(async _ =>
                {
                    await DownloadAsync();
                });

            SeekStartCommand =
                new RelayCommand(async _ =>
                {
                    _isSeeking = true;
                    await Task.CompletedTask;
                });


            SeekEndCommand =
                new RelayCommand(async _ =>
                {
                    _isSeeking = false;
                    await Task.CompletedTask;
                });

            MediaPlayer.Playing += (s, e) =>
            {
                IsPlaying = true;
            };

            MediaPlayer.Paused += (s, e) =>
            {
                IsPlaying = false;
            };

            MediaPlayer.PositionChanged += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {

                    if (!_isSeeking)
                    {
                        OnPropertyChanged(nameof(Position));
                    }

                    OnPropertyChanged(nameof(CurrentTime));

                });
            };

            MediaPlayer.LengthChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(Duration));
            };

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

                    Title =
                        response.Data.Title;

                    Description =
                        response.Data.Description;

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
            }
            catch (Exception ex)
            {

                Description =
                    ex.Message;

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

        public void Dispose()
        {

            MediaPlayer.Stop();

            _media?.Dispose();

            MediaPlayer.Dispose();

            _libVLC.Dispose();

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