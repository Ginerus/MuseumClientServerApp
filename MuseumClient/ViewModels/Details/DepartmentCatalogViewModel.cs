using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using MuseumClient.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels.Details
{
    public class DepartmentCatalogViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly ContentHubViewModel _hub;

        public int DepartmentId { get; }
        public string DepartmentName { get; }

        // Отфильтрованные коллекции по отделу
        public ObservableCollection<ExhibitDto> Exhibits { get; } = new();
        public ObservableCollection<DocumentDto> Articles { get; } = new();

        // ВАЖНО: MediaFileDto не содержит DepartmentId, поэтому фильтрация
        // по отделу для картинок/видео сейчас невозможна на клиенте.
        // Пока показываем ВСЕ медиафайлы без фильтра.
        // TODO: добавить DepartmentId в MediaFileDto / эндпоинт MediaFile?departmentId=
        public ObservableCollection<MediaFileDto> Images { get; } = new();
        public ObservableCollection<MediaFileDto> Videos { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        private string _selectedSection = "All";
        public string SelectedSection
        {
            get => _selectedSection;
            set { _selectedSection = value; OnPropertyChanged(nameof(SelectedSection)); }
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand SelectSectionCommand { get; }
        public RelayCommand OpenExhibitCommand { get; }
        public RelayCommand OpenArticleCommand { get; }
        public RelayCommand OpenImageCommand { get; }
        public RelayCommand OpenVideoCommand { get; }

        public DepartmentCatalogViewModel(ContentHubViewModel hub, int departmentId, string departmentName)
        {
            _hub = hub;
            DepartmentId = departmentId;
            DepartmentName = departmentName;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            LoadCommand = new RelayCommand(async _ => await LoadAllAsync());

            SelectSectionCommand = new RelayCommand(param =>
            {
                if (param is string section)
                    SelectedSection = section;

                return Task.CompletedTask;
            });

            OpenExhibitCommand = new RelayCommand(param =>
            {
                if (param is ExhibitDto exhibit)
                    _hub.ShowExhibit(exhibit.ExhibitId);

                return Task.CompletedTask;
            });

            OpenArticleCommand = new RelayCommand(param =>
            {
                if (param is DocumentDto doc)
                    _hub.ShowDocument(doc.DocumentId, doc.FileType);

                return Task.CompletedTask;
            });

            OpenImageCommand = new RelayCommand(param =>
            {
                if (param is MediaFileDto media)
                    _hub.ShowIllustration(media.MediaFileId);

                return Task.CompletedTask;
            });

            OpenVideoCommand = new RelayCommand(param =>
            {
                if (param is MediaFileDto media)
                    _hub.ShowVideo(media.MediaFileId);

                return Task.CompletedTask;
            });
        }

        public async Task LoadAllAsync()
        {
            try
            {
                IsLoading = true;

                await Task.WhenAll(
                    LoadExhibitsAsync(),
                    LoadArticlesAsync(),
                    LoadMediaAsync()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки каталога отдела: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadExhibitsAsync()
        {
            var response = await _apiService.GetAsync<ExhibitsResponse>("Exhibit");

            Exhibits.Clear();

            var filtered = response?.Data?
                .Where(e => e.Department?.DepartmentId == DepartmentId) ?? Enumerable.Empty<ExhibitDto>();

            foreach (var exhibit in filtered)
                Exhibits.Add(exhibit);

            _ = LoadThumbnailsAsync(Exhibits, e => e.ExhibitId, (e, img) => e.ThumbnailImage = img,
                id => $"Exhibit/thumbnail/{id}");
        }

        private async Task LoadArticlesAsync()
        {
            var response = await _apiService.GetAsync<DocumentsResponse>("Document");

            Articles.Clear();

            var filtered = response?.Data?
                .Where(d => d.Department?.DepartmentId == DepartmentId) ?? Enumerable.Empty<DocumentDto>();

            foreach (var doc in filtered)
                Articles.Add(doc);
        }

        private async Task LoadMediaAsync()
        {
            var response = await _apiService.GetAsync<MediaFilesResponse>("MediaFile");

            Images.Clear();
            Videos.Clear();

            if (response?.Data != null)
            {
                var filtered = response.Data.Where(m => m.DepartmentId == DepartmentId);

                foreach (var item in filtered)
                {
                    if (item.MediaType == "image")
                        Images.Add(item);
                    else if (item.MediaType == "video")
                        Videos.Add(item);
                }
            }

            _ = LoadThumbnailsAsync(Images, m => m.MediaFileId, (m, img) => m.ThumbnailImage = img,
                id => $"MediaFile/stream/{id}?size=thumb");

            _ = LoadThumbnailsAsync(Videos, m => m.MediaFileId, (m, img) => m.ThumbnailImage = img,
                id => $"MediaFile/stream/{id}?size=thumb");
        }

        private async Task LoadThumbnailsAsync<T>(
            ObservableCollection<T> items,
            Func<T, int> getId,
            Action<T, BitmapImage> setImage,
            Func<int, string> buildUrl)
        {
            foreach (var item in items)
            {
                try
                {
                    var bytes = await _apiService.GetBytesAsync(buildUrl(getId(item)));
                    setImage(item, ToBitmapImage(bytes));
                }
                catch
                {
                    // можно поставить placeholder позже
                }
            }
        }

        private static BitmapImage ToBitmapImage(byte[] bytes)
        {
            var image = new BitmapImage();

            using (var ms = new MemoryStream(bytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
            }

            return image;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}