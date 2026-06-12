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

        public RelayCommand EditDepartmentCommand { get; }


        public DepartmentsViewModel()
        {
            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            _auth = AuthService.Instance();

            LoadCommand = new RelayCommand(async _ => await LoadDepartmentsAsync());
            OpenDepartmentCommand = new RelayCommand(async param => await OpenDepartment(param));

            EditDepartmentCommand = new RelayCommand(async param => await EditDepartment(param));

            _auth.AuthChanged += OnAuthChanged;

            CanEdit = _auth.IsAdmin; // начальное состояние
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
                MessageBox.Show($"Ошибка загрузки отделов: {ex.Message}");
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

            await Task.Run(() =>
            {
                MessageBox.Show(
                    $"Отдел:\n{dept.Name}\nID: {dept.DepartmentId}"
                );
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task EditDepartment(object? parameter)
{
    if (parameter is not DepartmentDto dept)
        return;

    await Task.Run(() =>
    {
        MessageBox.Show(
            $"Редактирование отдела:\n{dept.Name}\nID: {dept.DepartmentId}"
        );
    });
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