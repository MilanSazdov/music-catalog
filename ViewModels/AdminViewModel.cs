using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Windows.Input;
using MusicCatalog.Views;

namespace MusicCatalog.ViewModels
{
    public class AdminViewModel : ViewModelBase
    {
        AnketaRepository _anketaRepository;
        IKorisnikRepository _korisnikRepository;
        IZanrRepository _zanrRepository;
        IMuzickiUmetnikRepository _umetnikRepository;
        IClanstvoRepository _clanstvoRepository;
        private readonly IMuzickoDeloRepository _deloRepo;

        // NEW
        private readonly IRecenzijaRepository _recRepo;
        private readonly IOcenaRepository _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository _zahtevRepo;

        private object _currentView;
        public object CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); } }
        public event Action? LoggedOut;
        public ICommand ShowKorisniciCommand { get; }
        public ICommand ShowUredniciCommand { get; }
        public ICommand ShowAnketeCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowZanroviCommand { get; }
        public ICommand ShowMuzickiSadrzajCommand { get; }
        public ICommand ShowUmetniciCommand { get; }
        public ICommand ShowZahteviZaIzmenuCommand { get; }

        public AdminViewModel(
            AnketaRepository anketaRepository,
            IKorisnikRepository korisnikRepository,
            IZanrRepository zanrRepository,
            IMuzickiUmetnikRepository umetnikRepository,
            IClanstvoRepository clanstvoRepository,
            IMuzickoDeloRepository deloRepo,
            IRecenzijaRepository recRepo,
            IOcenaRepository ocenaRepo,
            IZahtevZaIzmenuRepository zahtevRepo)
        {
            _clanstvoRepository = clanstvoRepository;
            _umetnikRepository = umetnikRepository;
            _korisnikRepository = korisnikRepository;
            _anketaRepository = anketaRepository;
            _zanrRepository = zanrRepository;
            _deloRepo = deloRepo;

            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _zahtevRepo = zahtevRepo;

            ShowKorisniciCommand = new RelayCommand(_ => ShowKorisnici());
            ShowUredniciCommand = new RelayCommand(_ => ShowUrednici());
            ShowAnketeCommand = new RelayCommand(_ => ShowAnkete());
            ShowMuzickiSadrzajCommand = new RelayCommand(_ => ShowMuzickiSadrzaj());
            ShowZanroviCommand = new RelayCommand(_ => ShowZanrove());
            ShowUmetniciCommand = new RelayCommand(_ => ShowUmetnici());
            ShowZahteviZaIzmenuCommand = new RelayCommand(_ => ShowZahteviZaIzmenu());

            LogoutCommand = new RelayCommand(_ => Logout());

            ShowKorisnici();
        }

        private void ShowUmetnici()
        {
            CurrentView = new AdminUmetniciView { DataContext = new AdminUmetniciViewModel(_umetnikRepository, _clanstvoRepository) };
        }

        private void ShowKorisnici()
        {
            CurrentView = new AdminKorisniciView { DataContext = new AdminKorisniciViewModel(_korisnikRepository) };
        }

        private void ShowUrednici()
        {
            CurrentView = new AdminUredniciView { DataContext = new AdminUredniciViewModel(_korisnikRepository, _zanrRepository) };
        }

        private void ShowMuzickiSadrzaj()
        {
            CurrentView = new AdminMuzickiSadrzajView { DataContext = new AdminMuzickiSadrzajViewModel(_deloRepo, _zanrRepository, _recRepo, _ocenaRepo, _zahtevRepo, _umetnikRepository) };
        }

        private void ShowZanrove()
        {
            CurrentView = new AdminZanroviView { DataContext = new AdminZanroviViewModel(_zanrRepository) };
        }

        private void ShowAnkete()
        {
            CurrentView = new AdminAnketeView { DataContext = new AdminAnketeViewModel(_anketaRepository) };
        }

        private void ShowZahteviZaIzmenu()
        {
            CurrentView = new AdminZahteviView { DataContext = new AdminZahteviViewModel(_zahtevRepo, _recRepo, _ocenaRepo) };
        }

        private void Logout()
        {
            LoggedOut?.Invoke();
        }
    }
}

