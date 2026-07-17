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
    public class ExhibitsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

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

        public ObservableCollection<ExhibitDto> Exhibits { get; } = new();

        private ICollectionView? _exhibitsView;
        public ICollectionView? ExhibitsView
        {
            get => _exhibitsView;
            private set
            {
                _exhibitsView = value;
                OnPropertyChanged(nameof(ExhibitsView));
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
        public RelayCommand LoadCommand { get; }
        public RelayCommand OpenExhibitCommand { get; }
        public RelayCommand CreateExhibitCommand { get; }
        public RelayCommand DeleteExhibitCommand { get; }

        private readonly ContentHubViewModel _hub;

        public ExhibitsViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            // Команды
            LoadCommand = new RelayCommand(async _ => await LoadExhibitsAsync());
            OpenExhibitCommand = new RelayCommand(async param => await OpenExhibit(param));
            CreateExhibitCommand = new RelayCommand(async param => await OpenCreateExhibit(param));
            DeleteExhibitCommand = new RelayCommand(async param => await DeleteExhibit(param));

            AuthService.Instance().AuthChanged += OnAuthChanged;

            CanEdit = AuthService.Instance().IsAdmin;
        }

        // Загрузка экспонатов
        public async Task LoadExhibitsAsync()
        {
            try
            {
                IsLoading = true;

                var response = await _apiService.GetAsync<ExhibitsResponse>("Exhibit");

                Exhibits.Clear();

                if (response?.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        Exhibits.Add(item);
                    }

                    // Загрузка картинок
                    _ = LoadThumbnailsAsync();
                }

                SetupCollectionView();
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки экспонатов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Группировка по отделам
        private void SetupCollectionView()
        {
            ExhibitsView = CollectionViewSource.GetDefaultView(Exhibits);

            if (ExhibitsView != null)
            {
                using (ExhibitsView.DeferRefresh())
                {
                    ExhibitsView.GroupDescriptions.Clear();
                    ExhibitsView.GroupDescriptions.Add(
                        new PropertyGroupDescription(nameof(ExhibitDto.DepartmentName))
                    );
                }
            }
        }

        // Клик по экспонату
        private async Task OpenExhibit(object? parameter)
        {
            if (parameter is not ExhibitDto exhibit)
                return;

            _hub.ShowExhibit(exhibit.ExhibitId);

            await Task.CompletedTask;
        }

        private async Task OpenCreateExhibit(object? parameter)
        {
            _hub.ShowCreateExhibit();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Загрузка картинок с header
        private async Task LoadThumbnailsAsync()
        {
            foreach (var exhibit in Exhibits)
            {
                try
                {
                    var bytes = await _apiService.GetBytesAsync($"Exhibit/thumbnail/{exhibit.ExhibitId}");

                    var image = new BitmapImage();

                    using (var ms = new MemoryStream(bytes))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        image.Freeze();
                    }

                    exhibit.ThumbnailImage = image;

                }
                catch
                {
                    // можно позже заменить на placeholder
                }
            }
        }

        private async Task DeleteExhibit(object? parameter)
        {
            if (parameter is not ExhibitDto exhibit)
                return;


            if (!ConfirmService.ConfirmDelete($"экспонат \"{exhibit.Name}\""))
                return;


            try
            {
                bool result = await _apiService.DeleteAsync(
                    $"Exhibit/{exhibit.ExhibitId}"
                );


                if (result)
                {
                    Exhibits.Remove(exhibit);
                }
                else
                {
                    InfoService.Show("Ошибка удаления экспоната");
                }
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void OnAuthChanged()
        {
            CanEdit = AuthService.Instance().IsAdmin;
        }

        public void RefreshPermissions()
        {
            OnPropertyChanged(nameof(CanEdit));
        }   
    }
}