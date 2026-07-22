using MuseumClient.Services;
using MuseumClient.Commands;
using MuseumClient.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels
{
    public class DepartmentsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public ObservableCollection<DepartmentDto> Departments { get; } = new();

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

        public RelayCommand LoadCommand { get; }
        public RelayCommand OpenDepartmentCommand { get; }
        public RelayCommand CreateDepartmentCommand { get; }

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
        private readonly ContentHubViewModel _hub;

        public RelayCommand EditDepartmentCommand { get; }
        public RelayCommand DeleteDepartmentCommand { get; }


        public DepartmentsViewModel(ContentHubViewModel hub)
        {
            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            _auth = AuthService.Instance();

            LoadCommand = new RelayCommand(async _ => await LoadDepartmentsAsync());
            OpenDepartmentCommand = new RelayCommand(async param => await OpenDepartment(param));
            CreateDepartmentCommand = new RelayCommand(async _ => _hub.ShowCreateDepartment());

            EditDepartmentCommand = new RelayCommand(async param => await EditDepartment(param));
            DeleteDepartmentCommand = new RelayCommand(async param => await DeleteDepartment(param));

            _auth.AuthChanged += OnAuthChanged;

            CanEdit = _auth.IsAdmin; // начальное состояние
            _hub = hub;
        }

        private void OnAuthChanged()
        {
            CanEdit = _auth.IsAdmin;
        }

        // Загрузка списка отделов
        public async Task LoadDepartmentsAsync()
        {
            try
            {
                IsLoading = true;

                var response = await _apiService.GetAsync<DepartmentsResponse>("Department");

                Departments.Clear();

                if (response?.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        Departments.Add(item);
                    }

                    // Загрузка картинок
                    _ = LoadThumbnailsAsync();
                }
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки отделов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Клик по отделу
        private async Task OpenDepartment(object? parameter)
        {
            if (parameter is not DepartmentDto dept)
                return;

            _hub.ShowDepartmentCatalog(dept.DepartmentId, dept.Name);

            await Task.CompletedTask;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task EditDepartment(object? parameter)
        {
            if (parameter is not DepartmentDto dept)
                return;

            _hub.ShowEditDepartment(dept.DepartmentId);

            await Task.CompletedTask;
        }

        private async Task DeleteDepartment(object? parameter)
        {
            if (parameter is not DepartmentDto dept)
                return;

            DepartmentDetailsResponse? details;

            try
            {
                details = await _apiService.GetAsync<DepartmentDetailsResponse>($"Department/{dept.DepartmentId}");
            }
            catch (Exception ex)
            {
                InfoService.Show($"Не удалось проверить содержимое отдела:\n{ex.Message}");
                return;
            }

            var exhibitsCount = details?.Data?.Exhibits?.Count ?? 0;
            var mediaCount = details?.Data?.MediaFiles?.Count ?? 0;
            var documentsCount = details?.Data?.Documents?.Count ?? 0;

            if (exhibitsCount > 0 || mediaCount > 0 || documentsCount > 0)
            {
                var lines = new List<string>();

                if (details?.Data?.Exhibits?.Count > 0)
                {
                    lines.Add($"Экспонаты ({details.Data.Exhibits.Count}):");

                    foreach (var exhibit in details.Data.Exhibits)
                        lines.Add($"    • {exhibit.Name}");

                    lines.Add(string.Empty);
                }

                if (details?.Data?.MediaFiles?.Count > 0)
                {
                    lines.Add($"Медиафайлы ({details.Data.MediaFiles.Count}):");

                    foreach (var media in details.Data.MediaFiles)
                        lines.Add($"    • {media.Title}");

                    lines.Add(string.Empty);
                }

                if (details?.Data?.Documents?.Count > 0)
                {
                    lines.Add($"Статьи ({details.Data.Documents.Count}):");

                    foreach (var document in details.Data.Documents)
                        lines.Add($"    • {document.Title}.{document.FileType}");

                    lines.Add(string.Empty);
                }

                InfoService.Show(
                    $"Нельзя удалить отдел \"{dept.Name}\", потому что он содержит:\n\n" +
                    string.Join("\n", lines).TrimEnd() +
                    "\n\nСначала перенесите или удалите эти объекты.");

                return;
            }

            var confirmed = ConfirmService.ConfirmDelete(dept.Name);

            if (!confirmed)
                return;

            var success = await _apiService.DeleteAsync($"Department/{dept.DepartmentId}");

            if (success)
            {
                Departments.Remove(dept);
            }
            else
            {
                InfoService.Show("Не удалось удалить отдел.");
            }
        }

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Загрузка картинок с token
        private async Task LoadThumbnailsAsync()
        {
            foreach (var dept in Departments)
            {
                try
                {
                    var bytes = await _apiService.GetBytesAsync($"Department/image/{dept.DepartmentId}");

                    var image = new BitmapImage();

                    using (var ms = new MemoryStream(bytes))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        image.Freeze();
                    }

                    dept.ThumbnailImage = image;
                }
                catch
                {
                    // можно поставить placeholder позже
                }
            }
        }
    }
}