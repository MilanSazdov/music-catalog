using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Windows.Input;
using MusicCatalog.Views;


namespace MusicCatalog.ViewModels
{
    public class AdminViewModel : ViewModelBase
    {
        private readonly AnketaRepository _anketaRepository;
        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IZanrRepository _zanrRepo;

        private object _currentView = new object();
        public object CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); } }
        public event Action? LoggedOut;
        public ICommand ShowKorisniciCommand { get; }
        public ICommand ShowUredniciCommand { get; }
        public ICommand ShowAnketeCommand { get; }
        public ICommand ShowMuzickiSadrzajCommand { get; }
        public ICommand ShowZanroviCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminViewModel(AnketaRepository anketaRepository)
        {
            _anketaRepository = anketaRepository;
            _zanrRepo = new ZanrRepository();
            _deloRepo = new MuzickoDeloRepository(_zanrRepo);
            ShowKorisniciCommand = new RelayCommand(_ => ShowKorisnici());
            ShowUredniciCommand = new RelayCommand(_ => ShowUrednici());

            ShowAnketeCommand = new RelayCommand(_ => ShowAnkete());
            ShowMuzickiSadrzajCommand = new RelayCommand(_ => ShowMuzickiSadrzaj());
            ShowZanroviCommand = new RelayCommand(_ => ShowZanrove());
            LogoutCommand = new RelayCommand(_ => Logout());

            ShowKorisnici();
        }

        private void ShowKorisnici()
        {
            CurrentView = new AdminKorisniciView { DataContext = new AdminKorisniciViewModel() };
        }

        private void ShowUrednici()
        {
            CurrentView = new AdminUredniciView { DataContext = new AdminUredniciViewModel() };
        }

        private void ShowAnkete()
        {
            CurrentView = new AdminAnketeView { DataContext = new AdminAnketeViewModel(_anketaRepository) };
        }

        private void ShowMuzickiSadrzaj()
        {
            CurrentView = new AdminMuzickiSadrzajView { DataContext = new AdminMuzickiSadrzajViewModel(_deloRepo, _zanrRepo) };
        }

        private void ShowZanrove()
        {
            CurrentView = new AdminZanroviView { DataContext = new AdminZanroviViewModel(_zanrRepo) };
        }

        private void Logout()
        {
            LoggedOut?.Invoke();
        }

    }
}

