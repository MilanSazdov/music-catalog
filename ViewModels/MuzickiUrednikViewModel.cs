using MusicCatalog.Models;
using MusicCatalog.Services;
using MusicCatalog.Utils;
using MusicCatalog.Views; // Dodaj ovaj using
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicCatalog.ViewModels
{
    // OVA KLASA JE ZNAČAJNO IZMENJENA DA LIČI NA RegistrovaniKorisnikViewModel
    public class MuzickiUrednikViewModel : ViewModelBase
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
        public ICommand IzmeniPodatkeCommand { get; }
        public ICommand ObrisiNalogCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DodajUmetnikaCommand { get; }

        public MuzickiUrednikViewModel(AuthService authService)
        {
            _authService = authService;
            TrenutniKorisnik = _authService.TrenutniKorisnik!;
            ShowLoginView = () => { };

            
            PrikaziSadrzajCommand = new RelayCommand(PrikaziSadrzaj);
            IzmeniPodatkeCommand = new RelayCommand(IzmeniPodatke);
            ObrisiNalogCommand = new RelayCommand(ObrisiNalog);
            LogoutCommand = new RelayCommand(Logout);
            DodajUmetnikaCommand = new RelayCommand(DodajUmetnika);

            
            PrikaziSadrzaj(null);
        }

        private void PrikaziSadrzaj(object? parameter)
        {
            
            var placeholder = new System.Windows.Controls.TextBlock
            {
                Text = "Ovde će biti prikazan sadržaj za Urednika...",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["HintBrush"]
            };
            CurrentContentView = placeholder;
        }

        private void IzmeniPodatke(object? parameter)
        {
            
            var vm = new IzmeniPodatkeViewModel(_authService, TrenutniKorisnik);

            
            vm.ZatvoriView = () =>
            {
                // Osveži podatke o korisniku na UI (npr. Ime i Prezime u panelu)
                OnPropertyChanged(nameof(TrenutniKorisnik));
                PrikaziSadrzaj(null);
            };

            // Postavi CurrentContentView na ovaj novi ViewModel
            CurrentContentView = vm;
        }

        // NOVO: Metoda za dodavanje umetnika (za sada prazna)
        private void DodajUmetnika(object? parameter)
        {
            // Trenutno ne radi ništa, kao što je traženo
            // Ovde bi kasnije išla logika za otvaranje view-a za dodavanje umetnika
            var placeholder = new System.Windows.Controls.TextBlock
            {
                Text = "Ovde će biti forma za dodavanje umetnika...",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["HintBrush"]
            };
            CurrentContentView = placeholder;
        }

        private void ObrisiNalog(object? parameter)
        {
            MessageBoxResult result = MessageBox.Show(
                "Da li ste sigurni da želite da obrišete nalog? Vaš nalog će biti blokiran.",
                "Potvrda brisanja naloga",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // MuzickiUrednik se takođe može blokirati
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