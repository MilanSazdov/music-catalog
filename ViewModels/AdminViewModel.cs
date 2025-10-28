using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class AdminViewModel : ViewModelBase
    {
        AnketaRepository _anketaRepository;
        IKorisnikRepository _korisnikRepository;
        IZanrRepository _zanrRepository;

        private object _currentView;
        public object CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); } }
        public event Action? LoggedOut;
        public ICommand ShowKorisniciCommand { get; }
        public ICommand ShowUredniciCommand { get; }
        public ICommand ShowAnketeCommand { get; }
        public ICommand LogoutCommand { get; }
        public AdminViewModel(AnketaRepository anketaRepository, IKorisnikRepository korisnikRepository, IZanrRepository zanrRepository)
        {
            _korisnikRepository = korisnikRepository;
            _anketaRepository = anketaRepository;
            _zanrRepository = zanrRepository;
            ShowKorisniciCommand = new RelayCommand(_ => ShowKorisnici());
            ShowUredniciCommand = new RelayCommand(_ => ShowUrednici());

            ShowAnketeCommand = new RelayCommand(_ => ShowAnkete());
            LogoutCommand = new RelayCommand(_ => Logout());

            ShowKorisnici();
        }

        private void ShowKorisnici()
        {
            CurrentView = new AdminKorisniciView { DataContext = new AdminKorisniciViewModel(_korisnikRepository) };
        }

        private void ShowUrednici()
        {
            CurrentView = new AdminUredniciView { DataContext = new AdminUredniciViewModel(_korisnikRepository, _zanrRepository) };
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

