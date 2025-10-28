using MusicCatalog.Services; // <-- DODAJ OVAJ RED
using MusicCatalog.Utils;
using System;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        // Ova linija sada radi
        private readonly AuthService _authService;

        public Action ShowLoginView { get; set; }

        private string _ime;
        public string Ime
        {
            get => _ime;
            set => SetField(ref _ime, value);
        }

        // (Ostatak tvoje klase ostaje isti)
        private string _prezime;
        public string Prezime
        {
            get => _prezime;
            set => SetField(ref _prezime, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetField(ref _confirmPassword, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
            RegisterCommand = new RelayCommand(OnRegister, CanRegister);
            NavigateToLoginCommand = new RelayCommand(OnNavigateToLogin);
            ShowLoginView = () => { };
        }

        private bool CanRegister(object? parameter)
        {
            return !string.IsNullOrEmpty(Ime) &&
                   !string.IsNullOrEmpty(Prezime) &&
                   !string.IsNullOrEmpty(Email) &&
                   !string.IsNullOrEmpty(Password) &&
                   Password == ConfirmPassword;
        }

        private void OnRegister(object? parameter)
        {
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Lozinke se ne poklapaju.";
                return;
            }

            var noviKorisnik = _authService.Register(Ime, Prezime, Email, Password);

            if (noviKorisnik != null)
            {
                ShowLoginView?.Invoke();
            }
            else
            {
                ErrorMessage = "Korisnik sa ovim email-om već postoji.";
            }
        }

        private void OnNavigateToLogin(object? parameter)
        {
            ShowLoginView?.Invoke();
        }
    }
}