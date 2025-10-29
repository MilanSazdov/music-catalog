using MusicCatalog.Models;
using MusicCatalog.Services;
using MusicCatalog.Utils;
using MusicCatalog.Views; // <-- OVAJ USING JE VEĆ POSTOJAO
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MusicCatalog.Repositories; // <-- DODAJ OVAJ USING

namespace MusicCatalog.ViewModels
{
    public class RegistrovaniKorisnikViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        public Action ShowLoginView { get; set; }

        // DODAJ REPOZITORIJUME
        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IZanrRepository _zanrRepo;

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

        // IZMENI KONSTRUKTOR
        public RegistrovaniKorisnikViewModel(AuthService authService, IMuzickoDeloRepository deloRepo, IZanrRepository zanrRepo)
        {
            _authService = authService;
            _deloRepo = deloRepo; // Dodeli
            _zanrRepo = zanrRepo; // Dodeli
            TrenutniKorisnik = _authService.TrenutniKorisnik!;
            ShowLoginView = () => { };

            PrikaziSadrzajCommand = new RelayCommand(PrikaziSadrzaj);
            IzmeniPodatkeCommand = new RelayCommand(IzmeniPodatke);
            ObrisiNalogCommand = new RelayCommand(ObrisiNalog);
            LogoutCommand = new RelayCommand(Logout);

            // Inicijalno prikaži sadržaj
            PrikaziSadrzaj(null);
        }

        // IZMENI OVU METODU
        private void PrikaziSadrzaj(object? parameter)
        {
            // Kreiraj ViewModel za prikaz sadržaja i prosledi mu repozitorijume
            var vm = new KorisnikMuzickiSadrzajViewModel(_deloRepo, _zanrRepo);

            // Postavi CurrentContentView na ovaj novi ViewModel
            // Pogled (KorisnikMuzickiSadrzajView) će biti automatski
            // kreiran na osnovu DataTemplate-a koji ćemo dodati.
            CurrentContentView = vm;
        }


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