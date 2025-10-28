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
        private AuthService _authService;

        private MainViewModel _mainViewModel;
        private LoginViewModel _loginViewModel;
        private RegisterViewModel _registerViewModel;
        // private GlavniAppViewModel _glavniAppViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Inicijalizacija Repozitorijuma i Servisa
            _korisnikRepository = new KorisnikRepository();
            _authService = new AuthService(_korisnikRepository);

            // Primer dodavanja admina (za testiranje)
            SeedAdmin();

            // 2. Inicijalizacija ViewModel-a
            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);
            // _glavniAppViewModel = new GlavniAppViewModel(_authService);

            // 3. Postavljanje početnog View-a
            _mainViewModel = new MainViewModel(_loginViewModel);

            // 4. Podešavanje Navigacije
            _loginViewModel.ShowRegisterView = () => _mainViewModel.TrenutniView = _registerViewModel;
            _registerViewModel.ShowLoginView = () => _mainViewModel.TrenutniView = _loginViewModel;

            // TODO: Kad napraviš glavni prozor, odkomentariši ovo:
            // _loginViewModel.ShowGlavniAppView = () => _mainViewModel.TrenutniView = _glavniAppViewModel; 

            // 5. Kreiranje i prikaz glavnog prozora
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