using MusicCatalog.Models;
using MusicCatalog.Services;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MusicCatalog.Repositories;

namespace MusicCatalog.ViewModels
{
    public class RegistrovaniKorisnikViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        public Action ShowLoginView { get; set; }

        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly IMuzickiUmetnikRepository _umetnikRepo; // <-- DODATO POLJE

        public Korisnik TrenutniKorisnik { get; private set; }

        private object? _currentContentView;
        public object? CurrentContentView
        {
            get => _currentContentView;
            set => SetField(ref _currentContentView, value);
        }

        public ICommand PrikaziSadrzajCommand { get; }
        public ICommand PrikaziUmetnikeCommand { get; } // <-- DODATA KOMANDA
        public ICommand IzmeniPodatkeCommand { get; }
        public ICommand ObrisiNalogCommand { get; }
        public ICommand LogoutCommand { get; }

        // --- POČETAK IZMENE (Konstruktor) ---
        public RegistrovaniKorisnikViewModel(
            AuthService authService,
            IMuzickoDeloRepository deloRepo,
            IZanrRepository zanrRepo,
            IMuzickiUmetnikRepository umetnikRepo) // <-- DODAT ARGUMENT
        {
            _authService = authService;
            _deloRepo = deloRepo;
            _zanrRepo = zanrRepo;
            _umetnikRepo = umetnikRepo; // <-- DODELJIVANJE
            TrenutniKorisnik = _authService.TrenutniKorisnik!;
            ShowLoginView = () => { };

            PrikaziSadrzajCommand = new RelayCommand(PrikaziSadrzaj);
            PrikaziUmetnikeCommand = new RelayCommand(PrikaziUmetnike); // <-- INICIJALIZACIJA
            IzmeniPodatkeCommand = new RelayCommand(IzmeniPodatke);
            ObrisiNalogCommand = new RelayCommand(ObrisiNalog);
            LogoutCommand = new RelayCommand(Logout);

            PrikaziSadrzaj(null);
        }
        // --- KRAJ IZMENE ---

        private void PrikaziSadrzaj(object? parameter)
        {
            var vm = new KorisnikMuzickiSadrzajViewModel(_deloRepo, _zanrRepo);
            CurrentContentView = vm;
        }

        // --- POČETAK DODAVANJA (Nova metoda) ---
        private void PrikaziUmetnike(object? parameter)
        {
            // Kreiramo novi ViewModel za prikaz umetnika
            var vm = new KorisnikUmetniciViewModel(_umetnikRepo);
            CurrentContentView = vm;
        }
        // --- KRAJ DODAVANJA ---

        private void IzmeniPodatke(object? parameter)
        {
            var vm = new IzmeniPodatkeViewModel(_authService, TrenutniKorisnik);

            vm.ZatvoriView = () =>
            {
                OnPropertyChanged(nameof(TrenutniKorisnik));
                PrikaziSadrzaj(null);
            };

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