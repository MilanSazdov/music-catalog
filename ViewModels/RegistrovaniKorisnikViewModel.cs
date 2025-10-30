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
        private readonly IMuzickiUmetnikRepository _umetnikRepo;
        private readonly IClanstvoRepository _clanstvoRepo;
        private readonly IKorisnikRepository _korisnikRepo;
        private readonly IRecenzijaRepository _recenzijaRepo;
        private readonly IOcenaRepository _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository _zahtevRepo; 

        public Korisnik TrenutniKorisnik { get; private set; }

        private object? _currentContentView;
        public object? CurrentContentView
        {
            get => _currentContentView;
            set => SetField(ref _currentContentView, value);
        }

        public ICommand PrikaziSadrzajCommand { get; }
        public ICommand PrikaziUmetnikeCommand { get; }
        public ICommand IzmeniPodatkeCommand { get; }
        public ICommand ObrisiNalogCommand { get; }
        public ICommand LogoutCommand { get; }


        public RegistrovaniKorisnikViewModel(
            AuthService authService,
            IMuzickoDeloRepository deloRepo,
            IZanrRepository zanrRepo,
            IMuzickiUmetnikRepository umetnikRepo,
            IClanstvoRepository clanstvoRepo,
            IKorisnikRepository korisnikRepo,
            IRecenzijaRepository recenzijaRepo,
            IOcenaRepository ocenaRepo,
            IZahtevZaIzmenuRepository zahtevRepo
            )
        {
            _authService = authService;
            _deloRepo = deloRepo;
            _zanrRepo = zanrRepo;
            _umetnikRepo = umetnikRepo;
            _clanstvoRepo = clanstvoRepo;
            _korisnikRepo = korisnikRepo;
            _recenzijaRepo = recenzijaRepo;
            _ocenaRepo = ocenaRepo;
            _zahtevRepo = zahtevRepo;
            TrenutniKorisnik = _authService.TrenutniKorisnik!;
            ShowLoginView = () => { };

            PrikaziSadrzajCommand = new RelayCommand(PrikaziSadrzaj);
            PrikaziUmetnikeCommand = new RelayCommand(PrikaziUmetnike);
            IzmeniPodatkeCommand = new RelayCommand(IzmeniPodatke);
            ObrisiNalogCommand = new RelayCommand(ObrisiNalog);
            LogoutCommand = new RelayCommand(Logout);

            PrikaziSadrzaj(null);
        }

        private void PrikaziSadrzaj(object? parameter)
        {
            var vm = new KorisnikMuzickiSadrzajViewModel(
                _deloRepo,
                _zanrRepo,
                (RegistrovaniKorisnik)TrenutniKorisnik,
                _korisnikRepo,
                _authService,       
                _recenzijaRepo,     
                _ocenaRepo,         
                _zahtevRepo         
            );
            CurrentContentView = vm;
        }

        private void PrikaziUmetnike(object? parameter)
        {

            var vm = new KorisnikUmetniciViewModel(
                _umetnikRepo,
                _clanstvoRepo,
                _deloRepo,
                (RegistrovaniKorisnik)TrenutniKorisnik,
                _korisnikRepo,
                _recenzijaRepo,
                _ocenaRepo
                );
            CurrentContentView = vm;
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