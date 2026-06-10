using MuseumClient.Commands;
using MuseumClient.Services;
using MuseumClient.ViewModels.Details;
using System.ComponentModel;
using System.Windows;

namespace MuseumClient.ViewModels
{
    public class ContentHubViewModel : INotifyPropertyChanged
    {
        private object _currentTabView;

        public string CurrentUserType =>
            AuthService.Instance().CurrentUserType?.ToUpper() ?? "UNKNOWN"; // Действующая тип юсера (роль)
        public string CurrentUserTypeDisplay =>
            AuthService.Instance().CurrentUserType == "admin"
                ? "Администратор"
                : "Гость";

        public object CurrentTabView
        {
            get => _currentTabView;
            set
            {
                _currentTabView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTabView)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUserType)));
            }
        }

        // Внутренние ViewModels
        public AboutMuseumViewModel AboutMuseumVM { get; }
        public ArticlesViewModel ArticlesVM { get; }
        public ExhibitsViewModel ExhibitsVM { get; }
        public DepartmentsViewModel DepartmentsVM { get; }
        public MediaImagesViewModel MediaImagesVM { get; }
        public MediaVideosViewModel MediaVideosVM { get; }

        private readonly MainViewModel _mainVM;

        // Команды для кнопок
        public RelayCommand ShowAboutMuseumCommand { get; }
        public RelayCommand ShowArticlesCommand { get; }
        public RelayCommand ShowExhibitsCommand { get; }
        public RelayCommand ShowDepartmentsCommand { get; }
        public RelayCommand ShowIllustrationsCommand { get; }
        public RelayCommand ShowVideosCommand { get; }
        public RelayCommand ExitCommand { get; }


        public ContentHubViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            // Инициализация внутренних ViewModel
            AboutMuseumVM = new AboutMuseumViewModel();
            ArticlesVM = new ArticlesViewModel(this);
            ExhibitsVM = new ExhibitsViewModel();
            DepartmentsVM = new DepartmentsViewModel();
            MediaImagesVM = new MediaImagesViewModel(this);
            MediaVideosVM = new MediaVideosViewModel();

            // Команды
            ShowAboutMuseumCommand = new RelayCommand(async _ =>
            {
                CurrentTabView = AboutMuseumVM;
                await AboutMuseumVM.LoadDepartmentCountAsync(); // обновляем данные
            });

            ShowArticlesCommand = new RelayCommand(async _ =>
            {
                CurrentTabView = ArticlesVM;
                //await Task.CompletedTask;
            });

            ShowExhibitsCommand = new RelayCommand(async _ =>
            {
                CurrentTabView = ExhibitsVM;
                //await Task.CompletedTask;
            });

            ShowDepartmentsCommand = new RelayCommand(async _ =>
            {
                CurrentTabView = DepartmentsVM;
                //await Task.CompletedTask;
            });

            ShowIllustrationsCommand = new RelayCommand(async _ =>
            {
                CurrentTabView = MediaImagesVM;
                //await Task.CompletedTask;
            });

            ShowVideosCommand = new RelayCommand(async _ =>
            {
                CurrentTabView = MediaVideosVM;
                //await Task.CompletedTask;
            });

            ExitCommand = new RelayCommand(async _ =>
            {
                AuthService.Instance().Logout();

                _mainVM.ShowLoginView();
            });

            // Стартовая вкладка
            CurrentTabView = AboutMuseumVM;

            AuthService.Instance().AuthChanged += OnAuthChanged;
        }

        public void ShowIllustration(int id)
        {
            CurrentTabView = new IllustrationViewModel(id);
        }

        public void ShowDocument(int id, string fileType)
        {
            CurrentTabView = new DocumentViewerViewModel(id, fileType);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnAuthChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUserType)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUserTypeDisplay)));
        }
    }
}