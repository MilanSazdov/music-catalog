using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public AdminViewModel(AnketaRepository anketaRepository, IKorisnikRepository korisnikRepository, IZanrRepository zanrRepository, IMuzickiUmetnikRepository umetnikRepository, IClanstvoRepository clanstvoRepository)
        {
            _clanstvoRepository = clanstvoRepository;
            _umetnikRepository = umetnikRepository;

            _korisnikRepository = korisnikRepository;
            _anketaRepository = anketaRepository;
            _zanrRepository = zanrRepository;
            _deloRepo = new MuzickoDeloRepository(_zanrRepository);
            ShowKorisniciCommand = new RelayCommand(_ => ShowKorisnici());
            ShowUredniciCommand = new RelayCommand(_ => ShowUrednici());
            ShowAnketeCommand = new RelayCommand(_ => ShowAnkete());
            ShowMuzickiSadrzajCommand = new RelayCommand(_ => ShowMuzickiSadrzaj());
            ShowZanroviCommand = new RelayCommand(_ => ShowZanrove());
            ShowUmetniciCommand = new RelayCommand(_ => ShowUmetnici());

            LogoutCommand = new RelayCommand(_ => Logout());

            ShowKorisnici();
        }

        private void ShowUmetnici()
        {
            // --- ISPRAVKA JE OVDE ---
            // Dodat je '_deloRepo' kao treći argument da bi se rešila greška
            CurrentView = new AdminUmetniciView { DataContext = new AdminUmetniciViewModel(_umetnikRepository, _clanstvoRepository, _deloRepo) };
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
            CurrentView = new AdminMuzickiSadrzajView { DataContext = new AdminMuzickiSadrzajViewModel(_deloRepo, _zanrRepository) };
        }

        private void ShowZanrove()
        {
            CurrentView = new AdminZanroviView { DataContext = new AdminZanroviViewModel(_zanrRepository) };
        }

        private void ShowAnkete()
        {
            CurrentView = new AdminAnketeView { DataContext = new AdminAnketeViewModel(_anketaRepository) };
        }

        private void Logout()
        {
            LoggedOut?.Invoke();
        }

    }
}