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
    public class MuzickiUrednikViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        public Action ShowLoginView { get; set; }


        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly IMuzickiUmetnikRepository _umetnikRepo; 
        private readonly IClanstvoRepository _clanstvoRepo; 

        private readonly IRecenzijaRepository _recRepo;
        private readonly IOcenaRepository _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository _zahtevRepo;
        private readonly IKorisnikRepository _korisnikRepo;


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

        public MuzickiUrednikViewModel(AuthService authService, IMuzickoDeloRepository deloRepo, IZanrRepository zanrRepo,
            IRecenzijaRepository recRepo, IOcenaRepository ocenaRepo, IZahtevZaIzmenuRepository zahtevRepo, IKorisnikRepository korisnikRepo,
            IMuzickiUmetnikRepository umetnikRepo,
            IClanstvoRepository clanstvoRepo)
        

        {
            _authService = authService;
            _deloRepo = deloRepo;
            _zanrRepo = zanrRepo;
            _umetnikRepo = umetnikRepo;       
            _clanstvoRepo = clanstvoRepo;     

            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _zahtevRepo = zahtevRepo;
            _korisnikRepo = korisnikRepo;
            _umetnikRepo = umetnikRepo;

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
            // Fix: Use the correct constructor for UrednikMuzickiSadrzajViewModel (8 arguments)
            CurrentContentView = new UrednikMuzickiSadrzajView
            {
                DataContext = new UrednikMuzickiSadrzajViewModel(_deloRepo, _zanrRepo, _recRepo, _ocenaRepo, _zahtevRepo, _korisnikRepo, _authService, _umetnikRepo)
            };
        }

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

        private void DodajUmetnika(object? parameter)
        {
            var placeholder = new System.Windows.Controls.TextBlock
            {
                Text = "Ovde će biti forma za dodavanje umetnika...",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["HintBrush"]
            };
            CurrentContentView = placeholder;

            
            var vm = new AdminUmetniciViewModel(_umetnikRepo, _clanstvoRepo, _deloRepo);
            CurrentContentView = vm;
        }

        private void ObrisiNalog(object? parameter)
        {
            var result = MessageBox.Show(
                "Da li ste sigurni da želite da obrišete nalog? Vaš nalog će biti blokiran.",
                "Potvrda brisanja naloga",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool uspeh = _authService.BlokirajNalog(TrenutniKorisnik);
                if (uspeh) ShowLoginView?.Invoke();
                else MessageBox.Show("Došlo je do greške prilikom brisanja naloga.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout(object? parameter)
        {
            _authService.Logout();
            ShowLoginView?.Invoke();
        }
    }
}