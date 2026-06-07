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

        public string UserType { get; set; }

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

        public LoginViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            // Инициализация Singleton AuthService с конфигом
            var config = new Services.ConfigService().Server;
            AuthService.Instance(config);

            RegisterCommand = new RelayCommand(async _ =>
            {
                var result = await AuthService.Instance().RegisterAsync(UserType, UserPassword);

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