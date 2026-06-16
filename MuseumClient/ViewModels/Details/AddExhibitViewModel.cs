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
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels.Details
{
    public class AddExhibitViewModel : INotifyPropertyChanged
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


        public AddExhibitViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance());

            _ = LoadDepartmentsAsync();


            SelectImageCommand =
                new RelayCommand(_ =>
                {
                    SelectImage();
                    return Task.CompletedTask;
                });


            SaveCommand =
                new RelayCommand(_ => SaveAsync());
        }


        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var result =
                    await _apiService
                    .GetAsync<DepartmentsResponse>("Department");


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
                MessageBox.Show(
                    $"Ошибка загрузки отделов:\n{ex.Message}");
            }
        }



        public RelayCommand SelectImageCommand { get; }

        public RelayCommand SaveCommand { get; }



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



        private string _materials = "";

        public string Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged(nameof(Materials));
            }
        }



        private bool _isPermanent;

        public bool IsPermanent
        {
            get => _isPermanent;
            set
            {
                _isPermanent = value;
                OnPropertyChanged(nameof(IsPermanent));
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



        private void SelectImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter =
                "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
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
                    MessageBox.Show("Введите название экспоната");
                    return;
                }


                if (DepartmentId == 0)
                {
                    MessageBox.Show("Выберите отдел");
                    return;
                }



                using var content =
                    new MultipartFormDataContent();



                content.Add(
                    new StringContent(
                        Name,
                        Encoding.UTF8),
                    "Name");


                content.Add(
                    new StringContent(
                        Description,
                        Encoding.UTF8),
                    "Description");


                content.Add(
                    new StringContent(
                        Materials,
                        Encoding.UTF8),
                    "Materials");


                content.Add(
                    new StringContent(
                        IsPermanent.ToString().ToLower(),
                        Encoding.UTF8),
                    "IsPermanent");


                content.Add(
                    new StringContent(
                        DepartmentId.ToString(),
                        Encoding.UTF8),
                    "DepartmentId");



                if (!string.IsNullOrEmpty(SelectedImagePath))
                {
                    var bytes =
                        await File.ReadAllBytesAsync(
                            SelectedImagePath);


                    var file =
                        new ByteArrayContent(bytes);



                    var extension =
                        Path.GetExtension(
                            SelectedImagePath)
                            .ToLower();



                    var type = extension switch
                    {
                        ".png" => "image/png",
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        _ => "application/octet-stream"
                    };


                    file.Headers.ContentType =
                        new System.Net.Http.Headers
                        .MediaTypeHeaderValue(type);



                    content.Add(
                        file,
                        "Image",
                        Path.GetFileName(
                            SelectedImagePath));
                }



                await _apiService
                    .PostMultipartAsync<ExhibitResponse>(
                        "Exhibit",
                        content);



                MessageBox.Show(
                    "Экспонат успешно создан");



                _hub.ShowExhibits();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка создания:\n{ex.Message}");
            }
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