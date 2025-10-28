using MusicCatalog.Models;
using MusicCatalog.Repositories;
using MusicCatalog.Services;
using MusicCatalog.ViewModels;
using System.Windows;


namespace MusicCatalog
{
    public partial class App : Application
    {
        private IKorisnikRepository _korisnikRepository;
        private AnketaRepository _anketaRepository;
        private AuthService _authService;

        private MainViewModel _mainViewModel;
        private LoginViewModel _loginViewModel;
        private RegisterViewModel _registerViewModel;
       

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            
            _korisnikRepository = new KorisnikRepository();
            _anketaRepository = new AnketaRepository();
            _authService = new AuthService(_korisnikRepository);

           
            SeedAdmin();

            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);

            _mainViewModel = new MainViewModel(_loginViewModel);

            _loginViewModel.ShowRegisterView = () => _mainViewModel.TrenutniView = _registerViewModel;
            _registerViewModel.ShowLoginView = () => _mainViewModel.TrenutniView = _loginViewModel;

            _loginViewModel.ShowAdminView = () =>
            {
                var vm = new AdminViewModel(_anketaRepository);
                vm.LoggedOut += () => _mainViewModel.TrenutniView = _loginViewModel;
                _mainViewModel.TrenutniView = vm;

            };


            MainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            MainWindow.Show();
        }

        private void SeedAdmin()
        {
            if (_korisnikRepository.GetByEmail("admin@mc.com") == null)
            {
                var admin = new Administrator("admin@mc.com", "Admin", "Adminovic", "admin123");
                _korisnikRepository.Add(admin);
                _korisnikRepository.SaveChanges();
            }
        }
    }
}