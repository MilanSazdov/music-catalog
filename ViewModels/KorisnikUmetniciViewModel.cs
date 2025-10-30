using MusicCatalog.Models;
using MusicCatalog.Models.Umetnici;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class UmetnikView : ViewModelBase
    {
        public MuzickiUmetnik Umetnik { get; }
        public int Id => Umetnik.Id;
        public string Opis => Umetnik.Opis;
        public string Slika => Umetnik.Slika;
        public string? Naziv => (Umetnik as Bend)?.Naziv;
        public DateOnly? DatumNastanka => (Umetnik as Bend)?.DatumNastanka;
        public bool? Aktivan => (Umetnik as Bend)?.Aktivan;
        public string? Ime => (Umetnik as Izvodjac)?.Ime;
        public string? Prezime => (Umetnik as Izvodjac)?.Prezime;
        public bool IsBend => Umetnik is Bend;

        private bool _isFavorit;
        public bool IsFavorit
        {
            get => _isFavorit;
            set => SetField(ref _isFavorit, value);
        }

        public UmetnikView(MuzickiUmetnik umetnik)
        {
            Umetnik = umetnik;
        }
    }


    public class KorisnikUmetniciViewModel : ViewModelBase
    {
        private readonly IMuzickiUmetnikRepository _umetniciRepository;
        private readonly IClanstvoRepository _clanstvoRepo;
        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IRecenzijaRepository _recenzijaRepo;
        private readonly IOcenaRepository _ocenaRepo;

        
        private readonly RegistrovaniKorisnik? _korisnik;
        private readonly IKorisnikRepository? _korisnikRepo;
        

        public ObservableCollection<UmetnikView> Umetnici { get; } = new();

        public ICommand ShowInfoCommand { get; }
        public ICommand ToggleFavoritCommand { get; }

        
        public bool CanManageFavorites => _korisnik != null;
        
        public KorisnikUmetniciViewModel(
            IMuzickiUmetnikRepository umetniciRepo,
            IClanstvoRepository clanstvoRepo,
            IMuzickoDeloRepository deloRepo,
            IRecenzijaRepository recenzijaRepo,
            IOcenaRepository ocenaRepo)
        {
            _umetniciRepository = umetniciRepo;
            _clanstvoRepo = clanstvoRepo;
            _deloRepo = deloRepo;
            _recenzijaRepo = recenzijaRepo;
            _ocenaRepo = ocenaRepo;

            _korisnik = null; 
            _korisnikRepo = null; 

            ShowInfoCommand = new RelayCommand(ShowInfo);
            ToggleFavoritCommand = new RelayCommand(ToggleFavorit);

            Load();
        }

        
        public KorisnikUmetniciViewModel(
            IMuzickiUmetnikRepository umetniciRepo,
            IClanstvoRepository clanstvoRepo,
            IMuzickoDeloRepository deloRepo,
            RegistrovaniKorisnik korisnik,
            IKorisnikRepository korisnikRepo,
            IRecenzijaRepository recenzijaRepo,
            IOcenaRepository ocenaRepo)
        {
            _umetniciRepository = umetniciRepo;
            _clanstvoRepo = clanstvoRepo;
            _deloRepo = deloRepo;
            _korisnik = korisnik;
            _korisnikRepo = korisnikRepo;
            _recenzijaRepo = recenzijaRepo;
            _ocenaRepo = ocenaRepo;

            ShowInfoCommand = new RelayCommand(ShowInfo);
            ToggleFavoritCommand = new RelayCommand(ToggleFavorit);

            Load();
        }

        private void Load()
        {
            Umetnici.Clear();

            
            var favoritiSet = _korisnik?.FavoritUmetnikIDs.ToHashSet() ?? new HashSet<int>();
            

            foreach (var umetnik in _umetniciRepository.GetAll().OrderBy(d => d is Bend))
            {
                var umetnikView = new UmetnikView(umetnik)
                {
                    IsFavorit = favoritiSet.Contains(umetnik.Id)
                };
                Umetnici.Add(umetnikView);
            }
        }

        private void ToggleFavorit(object? parameter)
        {
            
            if (_korisnik == null || _korisnikRepo == null || parameter is not UmetnikView umetnikView)
                return;
            

            umetnikView.IsFavorit = !umetnikView.IsFavorit;

            if (umetnikView.IsFavorit)
            {
                if (!_korisnik.FavoritUmetnikIDs.Contains(umetnikView.Id))
                {
                    _korisnik.FavoritUmetnikIDs.Add(umetnikView.Id);
                }
            }
            else
            {
                _korisnik.FavoritUmetnikIDs.Remove(umetnikView.Id);
            }

            _korisnikRepo.Update(_korisnik);
            _korisnikRepo.SaveChanges();
        }

        private void ShowInfo(object? parameter)
        {
            if (parameter is UmetnikView umetnikView)
            {
                
                var vm = new UmetnikInfoViewModel(
                    umetnikView.Umetnik,
                    _umetniciRepository,
                    _clanstvoRepo,
                    _deloRepo,
                    _recenzijaRepo,
                    _ocenaRepo);

                var view = new UmetnikInfoView
                {
                    DataContext = vm,
                    Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                view.ShowDialog();
            }
        }
    }
}