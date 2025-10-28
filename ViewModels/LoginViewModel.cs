// Datoteka: ViewModels/LoginViewModel.cs
using MusicCatalog.Models;
using MusicCatalog.Models.Enums; // <-- Dodaj ovo za Uloga
using MusicCatalog.Services;
using MusicCatalog.Utils;
using System;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;

        // Preimenovao sam ShowGlavniAppView u ShowRegistrovaniKorisnikView radi jasnoće
        public Action ShowRegisterView { get; set; }
        public Action ShowRegistrovaniKorisnikView { get; set; }
        public Action ShowAdminView { get; set; }

        #region Properties
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetField(ref _email, value))
                {
                    OnPropertyChanged(nameof(CanLogin));
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
                    OnPropertyChanged(nameof(CanLogin));
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

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(OnLogin, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(OnNavigateToRegister);

            // Inicijalizacija da se izbegne pad aplikacije
            ShowRegisterView = () => { };
            ShowRegistrovaniKorisnikView = () => { };
            ShowAdminView = () => { };
        }

        private bool CanLogin(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private void OnLogin(object? parameter)
        {
            ErrorMessage = string.Empty;
            bool success = _authService.Login(Email, Password);

            if (success)
            {
                Korisnik ulogovaniKorisnik = _authService.TrenutniKorisnik!;
                ClearFields();

                // --- ISPRAVLJENA LOGIKA ---
                // Pokrij sve uloge i dodaj bezbedan podrazumevani slučaj.
                switch (ulogovaniKorisnik.Uloga)
                {
                    case Uloga.Administrator:
                        ShowAdminView?.Invoke();
                        break;

                    case Uloga.RegistrovaniKorisnik:
                        ShowRegistrovaniKorisnikView?.Invoke();
                        break;

                    // TODO: Kada bude dodat View za muzičkog urednika, zameni podrazumevano grananje
                    // sa: ShowMuzickiUrednikView?.Invoke();
                    default:
                        // Privremeno: tretiraj sve ostale uloge kao registrovanog korisnika
                        ShowRegistrovaniKorisnikView?.Invoke();
                        break;
                }
            }
            else
            {
                ErrorMessage = "Pogrešan email, lozinka ili je nalog blokiran.";
                Password = string.Empty;
            }
        }

        private void OnNavigateToRegister(object? parameter)
        {
            ClearFields();
            ShowRegisterView?.Invoke();
        }

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}