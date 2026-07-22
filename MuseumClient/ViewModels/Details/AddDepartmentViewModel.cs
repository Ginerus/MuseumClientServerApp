using Microsoft.Win32;
using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels.Details
{
    public class AddDepartmentViewModel : INotifyPropertyChanged
    {
        private readonly ContentHubViewModel _hub;
        private readonly ApiService _apiService;

        // null => создание нового отдела, иначе редактируем существующий
        private readonly int? _editingId;

        public bool IsEditMode => _editingId.HasValue;

        public string HeaderTitle => IsEditMode ? "Редактировать отдел" : "Новый отдел";
        public string SaveButtonText => IsEditMode ? "Сохранить" : "Добавить";

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

        private string? _selectedImagePath;
        public string? SelectedImagePath
        {
            get => _selectedImagePath;
            set
            {
                _selectedImagePath = value;
                OnPropertyChanged(nameof(SelectedImagePath));
            }
        }

        private BitmapImage? _image;
        public BitmapImage? Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }

        public RelayCommand SelectImageCommand { get; }
        public RelayCommand SaveCommand { get; }

        // Создание нового отдела
        public AddDepartmentViewModel(ContentHubViewModel hub)
            : this(hub, null)
        {
        }

        // Создание ИЛИ редактирование (departmentId != null)
        public AddDepartmentViewModel(ContentHubViewModel hub, int? departmentId)
        {
            _hub = hub;
            _editingId = departmentId;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance());

            SelectImageCommand = new RelayCommand(_ =>
            {
                SelectImage();
                return Task.CompletedTask;
            });

            SaveCommand = new RelayCommand(_ => SaveAsync());

            if (IsEditMode)
            {
                _ = LoadExistingDepartmentAsync();
            }
        }

        private async Task LoadExistingDepartmentAsync()
        {
            if (_editingId == null)
                return;

            try
            {
                var response = await _apiService
                    .GetAsync<DepartmentDetailsResponse>($"Department/{_editingId}");

                var data = response?.Data?.Department;

                if (data == null)
                {
                    InfoService.Show("Не удалось загрузить данные отдела");
                    return;
                }

                Name = data.Name;
                Description = data.Description ?? "";

                // подгружаем текущую картинку для превью
                try
                {
                    var bytes = await _apiService.GetBytesAsync($"Department/image/{_editingId}");

                    var bitmap = new BitmapImage();

                    using (var ms = new MemoryStream(bytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    Image = bitmap;
                }
                catch
                {
                    // текущей картинки может не быть — не критично
                }
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки отдела:\n{ex.Message}");
            }
        }

        private void SelectImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedImagePath = dialog.FileName;

                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                bitmap.Freeze();

                Image = bitmap;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    InfoService.Show("Введите название отдела");
                    return;
                }

                using var content = new MultipartFormDataContent();

                content.Add(
                    new StringContent(Name, Encoding.UTF8),
                    "Name");

                content.Add(
                    new StringContent(Description ?? string.Empty, Encoding.UTF8),
                    "Description");

                if (!string.IsNullOrEmpty(SelectedImagePath))
                {
                    var bytes = await File.ReadAllBytesAsync(SelectedImagePath);

                    var file = new ByteArrayContent(bytes);

                    var extension = Path.GetExtension(SelectedImagePath).ToLower();

                    var type = extension switch
                    {
                        ".png" => "image/png",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        _ => "application/octet-stream"
                    };

                    file.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(type);

                    content.Add(
                        file,
                        "Image",
                        Path.GetFileName(SelectedImagePath));
                }
                // если файл не выбран заново — поле Image не отправляется,
                // сервер (DepartmentService.UpdateAsync) оставит текущую картинку

                if (IsEditMode)
                {
                    await _apiService.PutMultipartAsync<DepartmentResponse>(
                        $"Department/{_editingId}",
                        content);

                    InfoService.Show("Отдел успешно обновлён");
                }
                else
                {
                    await _apiService.PostMultipartAsync<DepartmentResponse>(
                        "Department",
                        content);

                    InfoService.Show("Отдел успешно создан");
                }

                _hub.ShowDepartments();
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка сохранения:\n{ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}