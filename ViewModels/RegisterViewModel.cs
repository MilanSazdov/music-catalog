// Datoteka: ViewModels/RegisterViewModel.cs
using MusicCatalog.Services;
using MusicCatalog.Utils;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class RegisterViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly AuthService _authService;
        public Action ShowLoginView { get; set; }

        #region Properties
        private string _ime = string.Empty;
        public string Ime
        {
            get => _ime;
            set
            {
                if (SetField(ref _ime, value))
                {
                    OnPropertyChanged(nameof(CanRegister));
                }
            }
        }

        private string _prezime = string.Empty;
        public string Prezime
        {
            get => _prezime;
            set
            {
                if (SetField(ref _prezime, value))
                {
                    OnPropertyChanged(nameof(CanRegister));
                }
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetField(ref _email, value))
                {
                    if (!string.IsNullOrEmpty(ErrorMessage) && ErrorMessage.Contains("već postoji"))
                    {
                        ErrorMessage = string.Empty;
                    }
                    OnPropertyChanged(nameof(CanRegister));
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (SetField(ref _password, value))
                {
                    OnPropertyChanged(nameof(CanRegister));
                    OnPropertyChanged(nameof(ConfirmPassword)); // Ažuriraj i potvrdu
                }
            }
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetField(ref _confirmPassword, value))
                {
                    OnPropertyChanged(nameof(CanRegister));
                }
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }
        #endregion

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
            // Dozvoli registraciju samo ako su SVI podaci validni
            return IsValid;
        }

        private void OnRegister(object? parameter)
        {
            ErrorMessage = string.Empty;
            var noviKorisnik = _authService.Register(Ime, Prezime, Email, Password);

            if (noviKorisnik != null)
            {
                ClearFields();
                ShowLoginView?.Invoke(); // Vrati se na login
            }
            else
            {
                ErrorMessage = "Korisnik sa ovom email adresom već postoji.";
            }
        }

        private void ClearFields()
        {
            Ime = string.Empty;
            Prezime = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            ErrorMessage = string.Empty;
        }

        private void OnNavigateToLogin(object? parameter)
        {
            ClearFields();
            ShowLoginView?.Invoke();
        }

        #region IDataErrorInfo Implementacija (Validacija)
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string? error = null;
                switch (columnName)
                {
                    case nameof(Ime):
                        if (string.IsNullOrWhiteSpace(Ime))
                            error = "Ime je obavezno.";
                        break;

                    case nameof(Prezime):
                        if (string.IsNullOrWhiteSpace(Prezime))
                            error = "Prezime je obavezno.";
                        break;

                    case nameof(Email):
                        if (string.IsNullOrWhiteSpace(Email))
                            error = "Email je obavezan.";
                        else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                            error = "Email nije u ispravnom formatu.";
                        break;

                    case nameof(Password):
                        if (string.IsNullOrWhiteSpace(Password))
                            error = "Lozinka je obavezna.";
                        else if (Password.Length < 6)
                            error = "Lozinka mora imati bar 6 karaktera.";
                        break;

                    case nameof(ConfirmPassword):
                        if (string.IsNullOrWhiteSpace(ConfirmPassword))
                            error = "Potvrda lozinke je obavezna.";
                        else if (Password != ConfirmPassword)
                            error = "Lozinke se ne poklapaju.";
                        break;
                }
                return error;
            }
        }

        private bool IsValid
        {
            get
            {
                string[] properties = { nameof(Ime), nameof(Prezime), nameof(Email), nameof(Password), nameof(ConfirmPassword) };
                foreach (var prop in properties)
                {
                    if (!string.IsNullOrEmpty(this[prop]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion
    }
}