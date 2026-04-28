using MuseumClient.Services;
using MuseumClient.Commands;
using MuseumClient.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels
{
    public class MediaVideosViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public ObservableCollection<MediaFileDto> Videos { get; } = new();

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

        public RelayCommand LoadVideosCommand { get; }
        public RelayCommand OpenVideoCommand { get; }

        public MediaVideosViewModel()
        {
            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            LoadVideosCommand = new RelayCommand(async _ => await LoadVideosAsync());
            OpenVideoCommand = new RelayCommand(async p => await OpenVideo(p));
        }

        public async Task LoadVideosAsync()
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
                }

                SetupCollectionView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки видео: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SetupCollectionView()
        {
            VideosView = CollectionViewSource.GetDefaultView(Videos);
        }

        private async Task OpenVideo(object? parameter)
        {
            if (parameter is not MediaFileDto video)
                return;

            MessageBox.Show($"Видео:\n{video.Title}\nID: {video.MediaFileId}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}