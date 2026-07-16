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
    public class AddArticleViewModel : INotifyPropertyChanged
    {
        private readonly ContentHubViewModel _hub;
        private readonly ApiService _apiService;

        public ObservableCollection<DepartmentDto> Departments { get; } = new();
        public ObservableCollection<ExhibitDto> Exhibits { get; } = new();

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

        private ExhibitDto? _selectedExhibit;
        public ExhibitDto? SelectedExhibit
        {
            get => _selectedExhibit;
            set
            {
                _selectedExhibit = value;
                ExhibitId = value?.ExhibitId ?? 0;
                OnPropertyChanged(nameof(SelectedExhibit));
            }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
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

        private int _exhibitId;
        public int ExhibitId
        {
            get => _exhibitId;
            set
            {
                _exhibitId = value;
                OnPropertyChanged(nameof(ExhibitId));
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

        public RelayCommand SelectFileCommand { get; }
        public RelayCommand SaveCommand { get; }

        public AddArticleViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance());

            _ = LoadDepartmentsAsync();
            _ = LoadExhibitsAsync();

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
                MessageBox.Show($"Ошибка загрузки отделов:\n{ex.Message}");
            }
        }

        private async Task LoadExhibitsAsync()
        {
            try
            {
                var result = await _apiService.GetAsync<ExhibitsResponse>("Exhibit");

                Exhibits.Clear();

                if (result?.Data == null)
                    return;

                foreach (var item in result.Data)
                {
                    Exhibits.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки экспонатов:\n{ex.Message}");
            }
        }

        private void SelectFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Документы (*.pdf;*.doc;*.docx;*.txt;*.md;*.ppt;*.pptx)|" +
                         "*.pdf;*.doc;*.docx;*.txt;*.md;*.ppt;*.pptx"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedFilePath = dialog.FileName;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("Введите название статьи");
                    return;
                }

                if (DepartmentId == 0)
                {
                    MessageBox.Show("Выберите отдел");
                    return;
                }

                if (string.IsNullOrEmpty(SelectedFilePath))
                {
                    MessageBox.Show("Выберите файл статьи");
                    return;
                }

                var extension = Path.GetExtension(SelectedFilePath).ToLower();

                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".md", ".ppt", ".pptx" };

                if (Array.IndexOf(allowedExtensions, extension) < 0)
                {
                    MessageBox.Show(
                        "Недопустимый тип файла. Разрешены: pdf, doc, docx, txt, md, ppt, pptx");
                    return;
                }

                using var content = new MultipartFormDataContent();

                content.Add(
                    new StringContent(Name, Encoding.UTF8),
                    "Title");

                if (SelectedExhibit != null)
                {
                    content.Add(
                        new StringContent(ExhibitId.ToString(), Encoding.UTF8),
                        "ExhibitId");
                }

                content.Add(
                    new StringContent(DepartmentId.ToString(), Encoding.UTF8),
                    "DepartmentId");

                var bytes = await File.ReadAllBytesAsync(SelectedFilePath);

                var file = new ByteArrayContent(bytes);

                var type = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".txt" => "text/plain",
                    ".md" => "text/markdown",
                    ".ppt" => "application/vnd.ms-powerpoint",
                    ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    _ => "application/octet-stream"
                };

                file.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(type);

                content.Add(
                    file,
                    "File",
                    Path.GetFileName(SelectedFilePath));

                await _apiService.PostMultipartAsync<DocumentResponse>("Document", content);

                MessageBox.Show("Статья успешно создана");

                _hub.ShowArticles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания:\n{ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}