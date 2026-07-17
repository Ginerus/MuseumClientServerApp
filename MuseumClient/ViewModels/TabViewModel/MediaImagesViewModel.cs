using MuseumClient.Services;
using MuseumClient.Commands;
using MuseumClient.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels
{
    public class MediaImagesViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public ObservableCollection<MediaFileDto> Images { get; } = new();

        private ICollectionView? _imagesView;
        public ICollectionView? ImagesView
        {
            get => _imagesView;
            private set
            {
                _imagesView = value;
                OnPropertyChanged(nameof(ImagesView));
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

        // Команды
        public RelayCommand LoadImagesCommand { get; }
        public RelayCommand OpenImageCommand { get; }
        public RelayCommand CreateImageCommand { get; }

        private readonly ContentHubViewModel _hub;

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

        public MediaImagesViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            _auth = AuthService.Instance();

            // Команды
            LoadImagesCommand = new RelayCommand(async _ => await LoadImagesAsync());
            OpenImageCommand = new RelayCommand(async p => await OpenImage(p));
            CreateImageCommand = new RelayCommand(async _ => await OpenCreateImage());

            _auth.AuthChanged += OnAuthChanged;

            CanEdit = _auth.IsAdmin;
        }

        private void OnAuthChanged()
        {
            CanEdit = _auth.IsAdmin;
        }

        public async Task LoadImagesAsync()
        {
            try
            {
                IsLoading = true;

                var response = await _apiService.GetAsync<MediaFilesResponse>("MediaFile");

                Images.Clear();

                if (response?.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        if (item.MediaType == "image")
                            Images.Add(item);
                    }

                    _ = LoadThumbnailsAsync();
                }

                SetupCollectionView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображений: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SetupCollectionView()
        {
            ImagesView = CollectionViewSource.GetDefaultView(Images);

            if (ImagesView != null)
            {
                using (ImagesView.DeferRefresh())
                {
                    ImagesView.GroupDescriptions.Clear();
                    ImagesView.GroupDescriptions.Add(
                        new PropertyGroupDescription(nameof(MediaFileDto.DepartmentName))
                    );
                }
            }
        }

        private async Task OpenImage(object? parameter)
        {
            if (parameter is not MediaFileDto image)
                return;

            _hub.ShowIllustration(image.MediaFileId);
        }

        private async Task LoadThumbnailsAsync()
        {
            foreach (var imageItem in Images)
            {
                try
                {
                    var bytes = await _apiService.GetBytesAsync(
                        $"MediaFile/stream/{imageItem.MediaFileId}?size=thumb"
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

                    imageItem.ThumbnailImage = image;
                }
                catch
                {
                }
            }
        }

        private async Task OpenCreateImage()
        {
            _hub.ShowCreateImage();

            await Task.CompletedTask;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}