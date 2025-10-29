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
    // =================================================================
    // == OVA KLASA JE IZAZIVALA GREŠKU JER JE VEROVATNO NEDOSTAJALA ==
    // =================================================================
    /// <summary>
    /// Wrapper klasa za MuzickiUmetnik koja dodaje IsFavorit property.
    /// </summary>
    public class UmetnikView : ViewModelBase
    {
        public MuzickiUmetnik Umetnik { get; }
        public int Id => Umetnik.Id;
        public string Opis => Umetnik.Opis;
        public string Slika => Umetnik.Slika;

        // Specifični property-ji za Bend
        public string? Naziv => (Umetnik as Bend)?.Naziv;
        public DateOnly? DatumNastanka => (Umetnik as Bend)?.DatumNastanka;
        public bool? Aktivan => (Umetnik as Bend)?.Aktivan;

        // Specifični property-ji za Izvodjaca
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
    // =================================================================


    public class KorisnikUmetniciViewModel : ViewModelBase
    {
        private readonly IMuzickiUmetnikRepository _umetniciRepository;
        private readonly IClanstvoRepository _clanstvoRepo;
        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly RegistrovaniKorisnik _korisnik;
        private readonly IKorisnikRepository _korisnikRepo;

        // Kolekcija sada koristi 'UmetnikView'
        public ObservableCollection<UmetnikView> Umetnici { get; } = new();

        public ICommand ShowInfoCommand { get; }
        public ICommand ToggleFavoritCommand { get; } // Komanda za srce

        // Konstruktor prima svih 5 argumenata
        public KorisnikUmetniciViewModel(
            IMuzickiUmetnikRepository umetniciRepo,
            IClanstvoRepository clanstvoRepo,
            IMuzickoDeloRepository deloRepo,
            RegistrovaniKorisnik korisnik,
            IKorisnikRepository korisnikRepo)
        {
            _umetniciRepository = umetniciRepo;
            _clanstvoRepo = clanstvoRepo;
            _deloRepo = deloRepo;
            _korisnik = korisnik;
            _korisnikRepo = korisnikRepo;

            ShowInfoCommand = new RelayCommand(ShowInfo);
            ToggleFavoritCommand = new RelayCommand(ToggleFavorit);

            Load();
        }

        private void Load()
        {
            Umetnici.Clear();
            // Proveravamo listu favorita korisnika
            var favoritiSet = _korisnik.FavoritUmetnikIDs.ToHashSet();

            foreach (var umetnik in _umetniciRepository.GetAll().OrderBy(d => d is Bend))
            {
                // Kreiramo 'UmetnikView'
                var umetnikView = new UmetnikView(umetnik)
                {
                    // Postavljamo da li je srce popunjeno
                    IsFavorit = favoritiSet.Contains(umetnik.Id)
                };
                Umetnici.Add(umetnikView);
            }
        }

        // Metod koji se poziva klikom na srce
        private void ToggleFavorit(object? parameter)
        {
            if (parameter is UmetnikView umetnikView)
            {
                // 1. Promeni stanje u UI
                umetnikView.IsFavorit = !umetnikView.IsFavorit;

                // 2. Ažuriraj listu u modelu korisnika
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

                // 3. Sačuvaj promene u JSON fajl
                _korisnikRepo.Update(_korisnik);
                _korisnikRepo.SaveChanges();
            }
        }

        // Metod za "Info" dugme
        private void ShowInfo(object? parameter)
        {
            if (parameter is UmetnikView umetnikView)
            {
                // Prosleđujemo originalni model, ne wrapper
                var vm = new UmetnikInfoViewModel(umetnikView.Umetnik, _umetniciRepository, _clanstvoRepo, _deloRepo);

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