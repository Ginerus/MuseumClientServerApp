using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels
{
    public class MediaVideosViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly ContentHubViewModel _hub;

        // Коллекция видео
        public ObservableCollection<MediaFileDto> Videos { get; } = new();

        // View (если позже понадобится группировка/сортировка)
        private ICollectionView? _videosView;
        public ICollectionView? VideosView
        {
            get => _videosView;
            private set
            {
                _videosView = value;
                OnPropertyChanged(nameof(VideosView));
            }
        }

        // Загрузка
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

        // Команды
        public RelayCommand LoadCommand { get; }
        public RelayCommand OpenVideoCommand { get; }
        public RelayCommand DeleteVideoCommand { get; }
        public RelayCommand CreateVideoCommand { get; }

        private bool _canEdit;
        public bool CanEdit
        {
            get => _canEdit;
            set
            {
                _canEdit = value;
                OnPropertyChanged(nameof(CanEdit));
            }
        }

        private readonly AuthService _auth;

        public MediaVideosViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            _auth = AuthService.Instance();

            // Команды
            LoadCommand = new RelayCommand(async _ => await LoadVideosListAsync());
            OpenVideoCommand = new RelayCommand(async param => await OpenVideo(param));
            DeleteVideoCommand = new RelayCommand(async param => await DeleteVideo(param));
            CreateVideoCommand = new RelayCommand(async _ => await OpenCreateVideo());

            _auth.AuthChanged += OnAuthChanged;

            CanEdit = _auth.IsAdmin;
        }

        private void OnAuthChanged()
        {
            CanEdit = _auth.IsAdmin;
        }

        // Загрузка списка видео
        public async Task LoadVideosListAsync()
        {
            try
            {
                IsLoading = true;

                var response = await _apiService.GetAsync<MediaFilesResponse>("MediaFile");

                Videos.Clear();

                if (response?.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        if (item.MediaType == "video")
                            Videos.Add(item);
                    }

                    _ = LoadThumbnailsAsync();
                }

                SetupCollectionView();
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки видео: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SetupCollectionView()
        {
            VideosView = CollectionViewSource.GetDefaultView(Videos);

            if (VideosView != null)
            {
                using (VideosView.DeferRefresh())
                {
                    VideosView.GroupDescriptions.Clear();
                    VideosView.GroupDescriptions.Add(
                        new PropertyGroupDescription(nameof(MediaFileDto.DepartmentName))
                    );
                }
            }
        }

        // клик по видео
        private async Task OpenVideo(object? parameter)
        {
            if (parameter is not MediaFileDto video)
                return;

            _hub.ShowVideo(video.MediaFileId);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async Task LoadThumbnailsAsync()
        {
            foreach (var video in Videos)
            {
                try
                {
                    var bytes = await _apiService.GetBytesAsync(
                        $"MediaFile/stream/{video.MediaFileId}?size=thumb"
                    );

                    var image = new BitmapImage();

                    using (var ms = new MemoryStream(bytes))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        image.Freeze();
                    }

                    video.ThumbnailImage = image;
                }
                catch
                {
                    // можно поставить дефолтную картинку
                }
            }
        }

        private async Task DeleteVideo(object? parameter)
        {
            if (parameter is not MediaFileDto video)
                return;

            var confirmed = ConfirmService.ConfirmDelete(video.Title);

            if (!confirmed)
                return;

            var success = await _apiService.DeleteAsync($"MediaFile/{video.MediaFileId}");

            if (success)
            {
                Videos.Remove(video);
            }
            else
            {
                InfoService.Show("Не удалось удалить видео.");
            }
        }

        private async Task OpenCreateVideo()
        {
            _hub.ShowCreateVideo();
            await Task.CompletedTask;
        }
    }
}