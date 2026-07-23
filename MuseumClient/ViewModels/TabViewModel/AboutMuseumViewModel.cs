using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MuseumClient.ViewModels
{
    public class AboutMuseumViewModel : INotifyPropertyChanged
    {
        private string _departmentCount = "Загрузка...";
        private string _documentCount = "Загрузка...";
        private string _exhibitCount = "Загрузка...";
        private string _mediaFileCount = "Загрузка...";
        private string _museumDescription = "Загрузка...";

        private readonly ApiService _apiService;

        // Команды
        public RelayCommand LoadCommand { get; }
        public RelayCommand EditDescriptionCommand { get; }

        private bool _canEdit;
        public bool CanEdit
        {
            get => _canEdit;
            set
            {
                _canEdit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanEdit)));
            }
        }

        public AboutMuseumViewModel()
        {
            // Получаем настройки сервера из ConfigService
            var config = new ConfigService().Server;

            // Создаём ApiService с токеном из AuthService
            _apiService = new ApiService(config, AuthService.Instance());

            LoadCommand = new RelayCommand(async _ => await LoadDepartmentCountAsync());
            EditDescriptionCommand = new RelayCommand(async _ => await EditDescription());

            AuthService.Instance().AuthChanged += OnAuthChanged;

            CanEdit = AuthService.Instance().IsAdmin;
        }

        private void OnAuthChanged()
        {
            CanEdit = AuthService.Instance().IsAdmin;
        }

        public string DepartmentCount
        {
            get => _departmentCount;
            set
            {
                _departmentCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DepartmentCount)));
            }
        }

        public string DocumentCount
        {
            get => _documentCount;
            set
            {
                _documentCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DocumentCount)));
            }
        }

        public string ExhibitCount
        {
            get => _exhibitCount;
            set
            {
                _exhibitCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExhibitCount)));
            }
        }

        public string MediaFileCount
        {
            get => _mediaFileCount;
            set
            {
                _mediaFileCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MediaFileCount)));
            }
        }

        public string MuseumDescription
        {
            get => _museumDescription;
            set
            {
                _museumDescription = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(MuseumDescription)));
            }
        }

        public async Task LoadDepartmentCountAsync()
        {
            try
            {
                var departmentTask = _apiService.GetAsync<CountResponse>("Department/count");
                var documentTask = _apiService.GetAsync<CountResponse>("Document/count");
                var exhibitTask = _apiService.GetAsync<CountResponse>("Exhibit/count");
                var mediaTask = _apiService.GetAsync<CountResponse>("MediaFile/count");
                var museumTask = _apiService.GetAsync<MuseumInfoResponse>("MuseumInfo");

                await Task.WhenAll(departmentTask, documentTask, exhibitTask, mediaTask, museumTask);

                DepartmentCount = (await departmentTask).Count.ToString();
                DocumentCount = (await documentTask).Count.ToString();
                ExhibitCount = (await exhibitTask).Count.ToString();
                MediaFileCount = (await mediaTask).Count.ToString();
                MuseumDescription = (await museumTask).Data?.Description ?? string.Empty;
            }
            catch (Exception ex)
            {
                DepartmentCount = $"Ошибка загрузки: {ex.Message}";
                DocumentCount = $"Ошибка загрузки: {ex.Message}";
                ExhibitCount = $"Ошибка загрузки: {ex.Message}";
                MediaFileCount = $"Ошибка загрузки: {ex.Message}";
                MuseumDescription = $"Ошибка загрузки: {ex.Message}";
            }
        }

        private async Task EditDescription()
        {
            InfoService.Show("Редактирование пока не реализовано");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}