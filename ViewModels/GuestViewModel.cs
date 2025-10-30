using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Windows.Input;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.ViewModels
{
    public class GuestViewModel : ViewModelBase
    {
        public Action ShowLoginView { get; set; }

        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly IMuzickiUmetnikRepository _umetnikRepo;
        private readonly IClanstvoRepository _clanstvoRepo;
        private readonly IRecenzijaRepository _recenzijaRepo;
        private readonly IOcenaRepository _ocenaRepo;

        private object? _currentContentView;
        public object? CurrentContentView
        {
            get => _currentContentView;
            set => SetField(ref _currentContentView, value);
        }

        public ICommand PrikaziSadrzajCommand { get; }
        public ICommand PrikaziUmetnikeCommand { get; }
        public ICommand VratiSeCommand { get; }

        public GuestViewModel(
            IMuzickoDeloRepository deloRepo,
            IZanrRepository zanrRepo,
            IMuzickiUmetnikRepository umetnikRepo,
            IClanstvoRepository clanstvoRepo,
            IRecenzijaRepository recenzijaRepo,
            IOcenaRepository ocenaRepo)
        {
            _deloRepo = deloRepo;
            _zanrRepo = zanrRepo;
            _umetnikRepo = umetnikRepo;
            _clanstvoRepo = clanstvoRepo;
            _recenzijaRepo = recenzijaRepo;
            _ocenaRepo = ocenaRepo;

            ShowLoginView = () => { };

            PrikaziSadrzajCommand = new RelayCommand(PrikaziSadrzaj);
            PrikaziUmetnikeCommand = new RelayCommand(PrikaziUmetnike);
            VratiSeCommand = new RelayCommand(VratiSe);

            PrikaziSadrzaj(null);
        }

        private void PrikaziSadrzaj(object? parameter)
        {
            var vm = new KorisnikMuzickiSadrzajViewModel(
                _deloRepo,
                _zanrRepo
            );
            CurrentContentView = vm;
        }

        private void PrikaziUmetnike(object? parameter)
        {
            var vm = new KorisnikUmetniciViewModel(
                _umetnikRepo,
                _clanstvoRepo,
                _deloRepo,
                _recenzijaRepo,
                _ocenaRepo
                );
            CurrentContentView = vm;
        }

        private void VratiSe(object? parameter)
        {
            ShowLoginView?.Invoke();
        }
    }
}