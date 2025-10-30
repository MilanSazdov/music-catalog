// Datoteka: App.xaml.cs
using MusicCatalog.Models;
using MusicCatalog.Repositories;
using MusicCatalog.Services;
using MusicCatalog.ViewModels;
using System.Windows;


namespace MusicCatalog
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly IKorisnikRepository _korisnikRepository;
        private readonly AnketaRepository _anketaRepository;
        private readonly IZanrRepository _zanrRepository;
        private readonly IMuzickiUmetnikRepository _umetnikRepository;
        private readonly IClanstvoRepository _clanstvoRepository;
        private readonly IMuzickoDeloRepository _deloRepository;

        // NEW
        private readonly IRecenzijaRepository _recenzijaRepository;
        private readonly IOcenaRepository _ocenaRepository;
        private readonly IZahtevZaIzmenuRepository _zahtevRepository;

        private readonly MainViewModel _mainViewModel;

        public App()
        {
            // 1. Kreiraj servise i repozitorijume
            _korisnikRepository = new KorisnikRepository("Data/korisnici.json");
            _anketaRepository = new AnketaRepository("Data/ankete.json");
            _zanrRepository = new ZanrRepository("Data/zanrovi.json");
            _umetnikRepository = new MuzickiUmetnikRepository("Data/umetnici.json");
            _clanstvoRepository = new ClanstvoRepository("Data/clanstva.json");
            _deloRepository = new MuzickoDeloRepository(_zanrRepository);

            // NEW
            _recenzijaRepository = new RecenzijaRepository("Data/recenzije.json");
            _ocenaRepository = new OcenaRepository("Data/ocene.json");
            _zahtevRepository = new ZahtevZaIzmenuRepository("Data/zahtevi_izmene.json");

            _authService = new AuthService(_korisnikRepository);

            // 2. Kreiraj glavni ViewModel
            _mainViewModel = new MainViewModel(null!);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 3. Kreiraj glavni prozor
            MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            // 4. Pokaži početni ekran (Login)
            ShowLoginView();

            MainWindow.Show();
            base.OnStartup(e);
        }

        // --- METODE ZA NAVIGACIJU ---

        private void ShowLoginView()
        {
            var loginVM = new LoginViewModel(_authService);
            loginVM.ShowRegisterView = ShowRegisterView;
            loginVM.ShowAdminView = ShowAdminView;
            loginVM.ShowRegistrovaniKorisnikView = ShowRegistrovaniKorisnikView;
            loginVM.ShowMuzickiUrednikView = ShowMuzickiUrednikView;
            _mainViewModel.TrenutniView = loginVM;
        }

        private void ShowRegisterView()
        {
            var registerVM = new RegisterViewModel(_authService);
            registerVM.ShowLoginView = ShowLoginView;
            _mainViewModel.TrenutniView = registerVM;
        }

        private void ShowAdminView()
        {
            var adminVM = new AdminViewModel(
                _anketaRepository,
                _korisnikRepository,
                _zanrRepository,
                _umetnikRepository,
                _clanstvoRepository,
                _deloRepository, // <-- Add this missing argument
                _recenzijaRepository,
                _ocenaRepository,
                _zahtevRepository);
            adminVM.LoggedOut += ShowLoginView;
            _mainViewModel.TrenutniView = adminVM;
        }

        private void ShowRegistrovaniKorisnikView()
        {
            var korisnikVM = new RegistrovaniKorisnikViewModel(_authService, _deloRepository, _zanrRepository, _recenzijaRepository, _ocenaRepository, _zahtevRepository, _korisnikRepository);
            korisnikVM.ShowLoginView = ShowLoginView;
            _mainViewModel.TrenutniView = korisnikVM;
        }

        private void ShowMuzickiUrednikView()
        {
            var urednikVM = new MuzickiUrednikViewModel(_authService, _deloRepository, _zanrRepository, _recenzijaRepository, _ocenaRepository, _zahtevRepository, _korisnikRepository, _umetnikRepository);
            urednikVM.ShowLoginView = ShowLoginView;
            _mainViewModel.TrenutniView = urednikVM;
        }
    }
}