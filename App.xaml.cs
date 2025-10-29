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

        private readonly MainViewModel _mainViewModel;

        public App()
        {

            _korisnikRepository = new KorisnikRepository("Data/korisnici.json");
            _anketaRepository = new AnketaRepository("Data/ankete.json");
            _zanrRepository = new ZanrRepository("Data/zanrovi.json");
            _umetnikRepository = new MuzickiUmetnikRepository("Data/umetnici.json");
            _clanstvoRepository = new ClanstvoRepository("Data/clanstva.json");

            _deloRepository = new MuzickoDeloRepository(_zanrRepository);

            _authService = new AuthService(_korisnikRepository);


            _mainViewModel = new MainViewModel(null!);
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };


            ShowLoginView();

            MainWindow.Show();
            base.OnStartup(e);
        }



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
            var adminVM = new AdminViewModel(_anketaRepository, _korisnikRepository, _zanrRepository, _umetnikRepository, _clanstvoRepository);
            adminVM.LoggedOut += ShowLoginView;
            _mainViewModel.TrenutniView = adminVM;
        }


        private void ShowRegistrovaniKorisnikView()
        {
            var korisnikVM = new RegistrovaniKorisnikViewModel(
                _authService,
                _deloRepository,
                _zanrRepository,
                _umetnikRepository,
                _clanstvoRepository,
                _korisnikRepository
            );
            korisnikVM.ShowLoginView = ShowLoginView;
            _mainViewModel.TrenutniView = korisnikVM;
        }

        private void ShowMuzickiUrednikView()
        {
            var urednikVM = new MuzickiUrednikViewModel(
                _authService,
                _deloRepository,
                _zanrRepository,
                _umetnikRepository,    
                _clanstvoRepository    
                );
            urednikVM.ShowLoginView = ShowLoginView;
            _mainViewModel.TrenutniView = urednikVM;
        }
        
    }
}