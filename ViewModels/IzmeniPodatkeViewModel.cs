// Datoteka: ViewModels/IzmeniPodatkeViewModel.cs
using MusicCatalog.Models;
using MusicCatalog.Services;
using MusicCatalog.Utils;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class IzmeniPodatkeViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly AuthService _authService;
        private readonly Korisnik _korisnik;

        public Action? ZatvoriView { get; set; }

        #region Properties
        private string _ime;
        public string Ime
        {
            get => _ime;
            set => SetField(ref _ime, value);
        }

        private string _prezime;
        public string Prezime
        {
            get => _prezime;
            set => SetField(ref _prezime, value);
        }

        // Email se ne menja, samo se prikazuje
        public string Email => _korisnik.Email;

        private string _novaLozinka = string.Empty;
        public string NovaLozinka
        {
            get => _novaLozinka;
            set
            {
                if (SetField(ref _novaLozinka, value))
                {
                    // Ako se menja lozinka, osveži i potvrdu
                    OnPropertyChanged(nameof(PotvrdaNoveLozinke));
                }
            }
        }

        private string _potvrdaNoveLozinke = string.Empty;
        public string PotvrdaNoveLozinke
        {
            get => _potvrdaNoveLozinke;
            set => SetField(ref _potvrdaNoveLozinke, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }
        #endregion

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public IzmeniPodatkeViewModel(AuthService authService, Korisnik trenutniKorisnik)
        {
            _authService = authService;
            _korisnik = trenutniKorisnik;

            // Učitaj trenutne podatke
            _ime = _korisnik.Ime;
            _prezime = _korisnik.Prezime;

            SaveCommand = new RelayCommand(OnSave, CanSave);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private bool CanSave(object? parameter)
        {
            return IsValid;
        }

        private void OnSave(object? parameter)
        {
            if (!IsValid) return;

            // Prosledi novu lozinku samo ako je uneta
            string? lozinkaZaSlanje = string.IsNullOrWhiteSpace(NovaLozinka) ? null : NovaLozinka;

            bool uspeh = _authService.UpdatePodatke(Email, Ime, Prezime, lozinkaZaSlanje);

            if (uspeh)
            {
                ZatvoriView?.Invoke();
            }
            else
            {
                ErrorMessage = "Došlo je do greške prilikom ažuriranja podataka.";
            }
        }

        private void OnCancel(object? parameter)
        {
            ZatvoriView?.Invoke();
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

                    // Validacija za lozinku je Opciona ali Rigorozna
                    case nameof(NovaLozinka):
                        // Greška je samo ako je JEDNO polje popunjeno, a drugo nije
                        if (!string.IsNullOrWhiteSpace(NovaLozinka) && NovaLozinka.Length < 6)
                            error = "Lozinka mora imati bar 6 karaktera.";
                        else if (!string.IsNullOrWhiteSpace(NovaLozinka) && string.IsNullOrWhiteSpace(PotvrdaNoveLozinke))
                            error = "Potvrda lozinke je obavezna.";
                        else if (string.IsNullOrWhiteSpace(NovaLozinka) && !string.IsNullOrWhiteSpace(PotvrdaNoveLozinke))
                            error = "Unesite novu lozinku.";
                        break;

                    case nameof(PotvrdaNoveLozinke):
                        if (!string.IsNullOrWhiteSpace(NovaLozinka) && NovaLozinka != PotvrdaNoveLozinke)
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
                // Proveri Ime i Prezime
                if (!string.IsNullOrEmpty(this[nameof(Ime)])) return false;
                if (!string.IsNullOrEmpty(this[nameof(Prezime)])) return false;

                // Proveri lozinke (ako su unete)
                if (!string.IsNullOrEmpty(this[nameof(NovaLozinka)])) return false;
                if (!string.IsNullOrEmpty(this[nameof(PotvrdaNoveLozinke)])) return false;

                return true;
            }
        }
        #endregion
    }
}