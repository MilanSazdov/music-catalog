using MusicCatalog.Models;
using MusicCatalog.Services;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MusicCatalog.Repositories; // <-- DODAJ OVAJ USING

namespace MusicCatalog.ViewModels
{
    public class MuzickiUrednikViewModel : ViewModelBase
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
        public ICommand DodajUmetnikaCommand { get; }

        //
        // ========= IZMENA JE OVDE (Konstruktor) =========
        //
        public MuzickiUrednikViewModel(AuthService authService, IMuzickoDeloRepository deloRepo, IZanrRepository zanrRepo)
        {
            _authService = authService;
            // Dodeli primljene repozitorijume
            _deloRepo = deloRepo;
            _zanrRepo = zanrRepo;

            TrenutniKorisnik = _authService.TrenutniKorisnik!;
            ShowLoginView = () => { };


            PrikaziSadrzajCommand = new RelayCommand(PrikaziSadrzaj);
            IzmeniPodatkeCommand = new RelayCommand(IzmeniPodatke);
            ObrisiNalogCommand = new RelayCommand(ObrisiNalog);
            LogoutCommand = new RelayCommand(Logout);
            DodajUmetnikaCommand = new RelayCommand(DodajUmetnika);


            PrikaziSadrzaj(null); // Prikazi sadrzaj odmah
        }

        //
        // ========= IZMENA JE OVDE (Metoda) =========
        //
        private void PrikaziSadrzaj(object? parameter)
        {
            // Umesto placeholdera, kreiramo isti ViewModel
            // koji koristi i registrovani korisnik
            var vm = new KorisnikMuzickiSadrzajViewModel(_deloRepo, _zanrRepo);
            CurrentContentView = vm;
        }

        private void IzmeniPodatke(object? parameter)
        {

            var vm = new IzmeniPodatkeViewModel(_authService, TrenutniKorisnik);

            // Kada se zatvori, vraća se na PrikaziSadrzaj (koji sada radi ispravno)
            vm.ZatvoriView = () =>
            {
                OnPropertyChanged(nameof(TrenutniKorisnik));
                PrikaziSadrzaj(null);
            };

            CurrentContentView = vm;
        }

        private void DodajUmetnika(object? parameter)
        {
            // Ovo ostaje placeholder kao što je i bilo
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