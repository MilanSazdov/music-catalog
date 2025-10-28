using MusicCatalog.Services; // <-- DODAJ OVAJ RED
using MusicCatalog.Utils;
using System;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        // Ova linija sada radi
        private readonly AuthService _authService;

        public Action ShowRegisterView { get; set; }
        public Action ShowGlavniAppView { get; set; }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }
        // (Ostatak tvoje klase ostaje isti)

        private string _password;
        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(OnLogin, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(OnNavigateToRegister);
            ShowRegisterView = () => { };
            ShowGlavniAppView = () => { };
        }

        private bool CanLogin(object? parameter)
        {
            return !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password);
        }

        private void OnLogin(object? parameter)
        {
            bool success = _authService.Login(Email, Password);
            if (success)
            {
                ErrorMessage = string.Empty;
                ShowGlavniAppView?.Invoke();
            }
            else
            {
                ErrorMessage = "Pogrešan email, lozinka ili je nalog blokiran.";
                Password = string.Empty;
            }
        }

        private void OnNavigateToRegister(object? parameter)
        {
            ShowRegisterView?.Invoke();
        }
    }
}