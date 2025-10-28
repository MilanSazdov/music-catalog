using MusicCatalog.Models;
using MusicCatalog.Services;
using MusicCatalog.Utils;
using MusicCatalog.Views; // <-- DODAJ OVAJ USING
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicCatalog.ViewModels
{
    public class RegistrovaniKorisnikViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        public Action ShowLoginView { get; set; }

        public Korisnik TrenutniKorisnik { get; private set; }

        private object? _currentContentView;
        public object? CurrentContentView
        {
            get => _currentContentView;
            set => SetField(ref _currentContentView, value);
        }

        public ICommand PrikaziSadrzajCommand { get; }
        public ICommand IzmeniPodatkeCommand { get; } // <-- DODATO
        public ICommand ObrisiNalogCommand { get; }
        public ICommand LogoutCommand { get; }

        public RegistrovaniKorisnikViewModel(AuthService authService)
        {
            _authService = authService;
            TrenutniKorisnik = _authService.TrenutniKorisnik!;
            ShowLoginView = () => { };

            PrikaziSadrzajCommand = new RelayCommand(PrikaziSadrzaj);
            IzmeniPodatkeCommand = new RelayCommand(IzmeniPodatke); // <-- DODATO
            ObrisiNalogCommand = new RelayCommand(ObrisiNalog);
            LogoutCommand = new RelayCommand(Logout);

            // Inicijalno prikaži placeholder
            PrikaziSadrzaj(null);
        }

        private void PrikaziSadrzaj(object? parameter)
        {
            // Ovo je onaj tekst "Ovde ce biti prikazan sadrzaj"
            var placeholder = new System.Windows.Controls.TextBlock
            {
                Text = "Ovde će biti prikazan sadržaj...",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["HintBrush"]
            };
            CurrentContentView = placeholder;
        }

        // <-- METODA DODATA -->
        private void IzmeniPodatke(object? parameter)
        {
            // Kreiraj novi ViewModel za izmenu podataka
            var vm = new IzmeniPodatkeViewModel(_authService, TrenutniKorisnik);

            // Postavi akciju koja će se desiti kada se izmena završi (ili otkaže)
            // Vraćamo se na "PrikaziSadrzaj"
            vm.ZatvoriView = () =>
            {
                // Osveži podatke o korisniku na UI (npr. Ime i Prezime u panelu)
                OnPropertyChanged(nameof(TrenutniKorisnik));
                PrikaziSadrzaj(null);
            };

            // Postavi CurrentContentView na ovaj novi ViewModel
            CurrentContentView = vm;
        }

        private void ObrisiNalog(object? parameter)
        {
            MessageBoxResult result = MessageBox.Show(
                "Da li ste sigurni da želite da obrišete nalog? Vaš nalog će biti blokiran i moći ćete da ga reaktivirate ponovnom registracijom.",
                "Potvrda brisanja naloga",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool uspeh = _authService.BlokirajNalog(TrenutniKorisnik);
                if (uspeh)
                {
                    ShowLoginView?.Invoke();
                }
                else
                {
                    MessageBox.Show("Došlo je do greške prilikom brisanja naloga.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Logout(object? parameter)
        {
            _authService.Logout();
            ShowLoginView?.Invoke();
        }
    }
}