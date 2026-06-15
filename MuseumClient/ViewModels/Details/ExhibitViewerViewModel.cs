using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MuseumClient.ViewModels.Details
{
    public class ExhibitViewerViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _id;

        public ExhibitViewerViewModel(int exhibitId)
        {
            _id = exhibitId;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance());

            CanEdit = AuthService.Instance().IsAdmin;

            AuthService.Instance().AuthChanged += OnAuthChanged;

            _ = InitializeAsync();
        }

        private void OnAuthChanged()
        {
            CanEdit = AuthService.Instance().IsAdmin;
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));

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

        private string _departmentName = "";
        public string DepartmentName
        {
            get => _departmentName;
            set
            {
                _departmentName = value;
                OnPropertyChanged(nameof(DepartmentName));
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
                OnPropertyChanged(nameof(StorageType));
            }
        }

        public string StorageType =>
            IsPermanent
                ? "Постоянное хранение"
                : "Временное хранение";

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

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                await LoadMetadataAsync();
                await LoadImageAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMetadataAsync()
        {
            var response =
                await _apiService.GetAsync<ExhibitResponse>(
                    $"Exhibit/{_id}");

            if (response?.Data == null)
                return;

            Name = response.Data.Name;
            Description = response.Data.Description;
            Materials = response.Data.Materials;
            DepartmentName = response.Data.DepartmentName;
            IsPermanent = response.Data.IsPermanent;
        }

        private async Task LoadImageAsync()
        {
            try
            {
                var bytes =
                    await _apiService.GetBytesAsync(
                        $"Exhibit/image/{_id}");

                var image = new BitmapImage();

                using var ms = new MemoryStream(bytes);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();

                Image = image;
            }
            catch
            {
                // позже можно поставить placeholder
            }
        }
    }
}