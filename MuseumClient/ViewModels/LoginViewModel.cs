using MuseumClient.Commands;
using MuseumClient.Helpers;
using MuseumClient.Models;
using MuseumClient.Services;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace MuseumClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainVM;
        private readonly AuthService _authService;

        public RelayCommand RegisterCommand { get; }

        private string _userPassword;
        public string UserPassword
        {
            get => _userPassword;
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserPassword)));
                }
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPasswordVisible)));
                }
            }
        }

        public RelayCommand TogglePasswordVisibilityCommand { get; }

        public List<UserTypeItem> UserTypes { get; } = new()
        {
            new UserTypeItem { Display = "Гость", Value = "guest" },
            new UserTypeItem { Display = "Администратор", Value = "admin" }
        };

        public class UserTypeItem
        {
            public string Display { get; set; }
            public string Value { get; set; }
        }

        public string UserType { get; set; }

        private UserTypeItem _selectedUserType;
        public UserTypeItem SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                _selectedUserType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedUserType)));
            }
        }

        public LoginViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;


            SelectedUserType = UserTypes.First();

            // Инициализация Singleton AuthService с конфигом
            var config = new Services.ConfigService().Server;
            AuthService.Instance(config);

            TogglePasswordVisibilityCommand = new RelayCommand(async _ =>
            {
                IsPasswordVisible = !IsPasswordVisible;
                await Task.CompletedTask;
            });

            RegisterCommand = new RelayCommand(async _ =>
            {
                var result = await AuthService.Instance().RegisterAsync(SelectedUserType.Value, UserPassword);

                switch (result)
                {
                    case AuthResult.Success:

                        ErrorMessage = "";
                        _mainVM.ShowContentHubView();
                        break;

                    case AuthResult.InvalidCredentials:

                        ErrorMessage = "Неверный пароль.";
                        break;

                    case AuthResult.ServerUnavailable:

                        ErrorMessage = "Нет доступа к серверу.";
                        break;
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}