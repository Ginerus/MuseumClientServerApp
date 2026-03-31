using MuseumClient.Services;
using MuseumClient.Commands;
using MuseumClient.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MuseumClient.ViewModels
{
    public class ArticlesViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // Коллекция документов
        public ObservableCollection<DocumentDto> Documents { get; set; } = new();

        // View для группировки
        private ICollectionView _documentsView;
        public ICollectionView DocumentsView
        {
            get => _documentsView;
            set
            {
                _documentsView = value;
                OnPropertyChanged(nameof(DocumentsView));
            }
        }

        public RelayCommand LoadCommand { get; }

        public ArticlesViewModel()
        {
            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            LoadCommand = new RelayCommand(async _ => await LoadArticlesListAsync());
        }

        // агрузка списка документов
        public async Task LoadArticlesListAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<DocumentsResponse>("Document");

                Documents.Clear();

                if (response?.Data != null)
                {
                    foreach (var doc in response.Data)
                    {
                        Documents.Add(doc);
                    }
                }

                // Создаём CollectionView
                DocumentsView = CollectionViewSource.GetDefaultView(Documents);

                // Группировка по отделу
                DocumentsView.GroupDescriptions.Clear();
                DocumentsView.GroupDescriptions.Add(
                    new PropertyGroupDescription(nameof(DocumentDto.DepartmentName))
                );
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статей: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}