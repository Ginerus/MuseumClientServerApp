using MuseumClient.Services;
using MuseumClient.Commands;
using MuseumClient.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System;

namespace MuseumClient.ViewModels
{
    public class ArticlesViewModel : INotifyPropertyChanged
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

        // Коллекция документов (readonly — чтобы не заменили извне)
        public ObservableCollection<DocumentDto> Documents { get; } = new();

        // View для группировки
        private ICollectionView? _documentsView;
        public ICollectionView? DocumentsView
        {
            get => _documentsView;
            private set
            {
                _documentsView = value;
                OnPropertyChanged(nameof(DocumentsView));
            }
        }

        // Состояние загрузки (для UI)
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
        public RelayCommand OpenDocumentCommand { get; }
        public RelayCommand CreateArticleCommand { get; }

        private readonly ContentHubViewModel _hub;

        public ArticlesViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            var config = new ConfigService().Server;
            _apiService = new ApiService(config, AuthService.Instance());

            LoadCommand = new RelayCommand(async _ => await LoadArticlesListAsync());
            OpenDocumentCommand = new RelayCommand(async p => await OpenDocument(p));
            CreateArticleCommand = new RelayCommand(async _ => _hub.ShowCreateArticle());

            AuthService.Instance().AuthChanged += OnAuthChanged;

            CanEdit = AuthService.Instance().IsAdmin; // важно: начальное значение
        }

        private void OnAuthChanged()
        {
            CanEdit = AuthService.Instance().IsAdmin;
        }

        public void RefreshPermissions()
        {
            OnPropertyChanged(nameof(CanEdit));
        }

        // Загрузка списка документов
        public async Task LoadArticlesListAsync()
        {
            try
            {
                IsLoading = true;

                var response = await _apiService.GetAsync<DocumentsResponse>("Document");

                Documents.Clear();

                if (response?.Data != null)
                {
                    foreach (var doc in response.Data)
                    {
                        Documents.Add(doc);
                    }
                }

                SetupCollectionView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статей: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Вынесли настройку View отдельно (чистота + переиспользование)
        private void SetupCollectionView()
        {
            DocumentsView = CollectionViewSource.GetDefaultView(Documents);

            if (DocumentsView != null)
            {
                using (DocumentsView.DeferRefresh())
                {
                    DocumentsView.GroupDescriptions.Clear();
                    DocumentsView.GroupDescriptions.Add(
                        new PropertyGroupDescription(nameof(DocumentDto.DepartmentName))
                    );
                }
            }
        }

        // Клик по плитке
        private async Task OpenDocument(object? parameter)
        {
            if (parameter is not DocumentDto doc)
                return;

            _hub.ShowDocument(doc.DocumentId, doc.FileType);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}