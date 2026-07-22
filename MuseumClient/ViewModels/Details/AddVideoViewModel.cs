using Microsoft.Win32;
using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MuseumClient.ViewModels.Details
{
    public class AddVideoViewModel : INotifyPropertyChanged
    {
        private readonly ContentHubViewModel _hub;
        private readonly ApiService _apiService;

        public ObservableCollection<DepartmentDto> Departments { get; } = new();

        private DepartmentDto? _selectedDepartment;
        public DepartmentDto? SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                _selectedDepartment = value;
                DepartmentId = value?.DepartmentId ?? 0;
                OnPropertyChanged(nameof(SelectedDepartment));
            }
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private int _departmentId;
        public int DepartmentId
        {
            get => _departmentId;
            set
            {
                _departmentId = value;
                OnPropertyChanged(nameof(DepartmentId));
            }
        }

        private string? _selectedFilePath;
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        }

        // Индикация процесса загрузки на сервер
        private bool _isUploading;
        public bool IsUploading
        {
            get => _isUploading;
            set
            {
                _isUploading = value;
                OnPropertyChanged(nameof(IsUploading));
            }
        }

        private double _uploadProgress;
        public double UploadProgress
        {
            get => _uploadProgress;
            set
            {
                _uploadProgress = value;
                OnPropertyChanged(nameof(UploadProgress));
            }
        }

        public RelayCommand SelectFileCommand { get; }
        public RelayCommand SaveCommand { get; }

        public AddVideoViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance());

            _ = LoadDepartmentsAsync();

            SelectFileCommand = new RelayCommand(_ =>
            {
                SelectFile();
                return Task.CompletedTask;
            });

            SaveCommand = new RelayCommand(_ => SaveAsync());
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var result = await _apiService.GetAsync<DepartmentsResponse>("Department");

                Departments.Clear();

                if (result?.Data == null)
                    return;

                foreach (var item in result.Data)
                {
                    Departments.Add(item);
                }
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки отделов:\n{ex.Message}");
            }
        }

        private void SelectFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Видео (*.mp4;*.mov;*.avi)|*.mp4;*.mov;*.avi"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedFilePath = dialog.FileName;
            }
        }

        private async Task SaveAsync()
        {
            if (IsUploading)
                return; // защита от повторного клика во время загрузки

            try
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    InfoService.Show("Введите название видео");
                    return;
                }

                if (DepartmentId == 0)
                {
                    InfoService.Show("Выберите отдел");
                    return;
                }

                if (string.IsNullOrEmpty(SelectedFilePath))
                {
                    InfoService.Show("Выберите видеофайл");
                    return;
                }

                IsUploading = true;
                UploadProgress = 0;

                using var content = new MultipartFormDataContent();

                content.Add(
                    new StringContent(Title, Encoding.UTF8),
                    "Title");

                content.Add(
                    new StringContent(Description ?? string.Empty, Encoding.UTF8),
                    "Description");

                content.Add(
                    new StringContent(DepartmentId.ToString(), Encoding.UTF8),
                    "DepartmentId");

                var bytes = await File.ReadAllBytesAsync(SelectedFilePath);

                var extension = Path.GetExtension(SelectedFilePath).ToLower();

                var type = extension switch
                {
                    ".mp4" => "video/mp4",
                    ".mov" => "video/quicktime",
                    ".avi" => "video/x-msvideo",
                    _ => "application/octet-stream"
                };

                // IProgress<T> репортит обратно на UI-поток автоматически
                // (захватывает текущий SynchronizationContext в момент создания)
                var progress = new Progress<double>(p => UploadProgress = p);

                var file = new ProgressableByteArrayContent(bytes, progress);

                file.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(type);

                content.Add(
                    file,
                    "File",
                    Path.GetFileName(SelectedFilePath));

                await _apiService.PostMultipartAsync<MediaFileSingleResponse>("MediaFile", content);

                InfoService.Show("Видео успешно загружено");

                _hub.ShowVideos();
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки видео:\n{ex.Message}");
            }
            finally
            {
                IsUploading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}