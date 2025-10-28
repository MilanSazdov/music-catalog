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

        private object _currentView;
        public object CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); } }
        public event Action? LoggedOut;
        public ICommand ShowKorisniciCommand { get; }
        public ICommand ShowUredniciCommand { get; }
        public ICommand ShowAnketeCommand { get; }
        public ICommand LogoutCommand { get; }
        public AdminViewModel(AnketaRepository anketaRepository)
        {
            _anketaRepository = anketaRepository;
            ShowKorisniciCommand = new RelayCommand(_ => ShowKorisnici());
            ShowUredniciCommand = new RelayCommand(_ => ShowUrednici());

            ShowAnketeCommand = new RelayCommand(_ => ShowAnkete());
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

        private void Logout()
        {
            LoggedOut?.Invoke();
        }

    }
}

