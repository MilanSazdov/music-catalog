using MusicCatalog.Models;
using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Repositories;
using MusicCatalog.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Windows.Input;
using MusicCatalog.Utils;
using System.Windows;

namespace MusicCatalog.ViewModels
{
    public class KorisnikMuzickiSadrzajViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _delaRepo;
        private readonly IZanrRepository _zanrRepo;

        // IZMENA: Oba su sada nullable (mogu biti null)
        private readonly RegistrovaniKorisnik? _korisnik;
        private readonly IKorisnikRepository? _korisnikRepo;

        public ObservableCollection<MuzickoDeloView> Dela { get; } = new();
        private MuzickoDeloView? _selected;
        public MuzickoDeloView? Selected { get => _selected; set { _selected = value; OnPropertyChanged(nameof(Selected)); } }

        public ICommand OceniCommand { get; }

        // DODATO: Ovaj property će sakriti "srce" za Urednika
        public bool CanManageFavorites => _korisnik != null;

        // --- NOVI KONSTRUKTOR (za Muzickog Urednika) ---
        public KorisnikMuzickiSadrzajViewModel(IMuzickoDeloRepository delaRepo, IZanrRepository zanrRepo)
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;
            _korisnik = null;     // Urednik nema favorite
            _korisnikRepo = null; // Urednik ne čuva favorite

            OceniCommand = new RelayCommand(OceniDelo);
            Load();
        }

        // --- POSTOJEĆI KONSTRUKTOR (za Registrovanog Korisnika) ---
        public KorisnikMuzickiSadrzajViewModel(IMuzickoDeloRepository delaRepo, IZanrRepository zanrRepo, RegistrovaniKorisnik korisnik, IKorisnikRepository korisnikRepo)
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;
            _korisnik = korisnik;
            _korisnikRepo = korisnikRepo;

            OceniCommand = new RelayCommand(OceniDelo);
            Load();
        }

        private void Load()
        {
            foreach (var d in Dela)
            {
                d.PropertyChanged -= DeloView_PropertyChanged;
            }
            Dela.Clear();

            // IZMENA: Učitaj favorite SAMO ako korisnik postoji
            var favoritiSet = _korisnik?.FavoritDeloIDs.ToHashSet() ?? new HashSet<int>();

            foreach (var d in _delaRepo.GetAll().OrderBy(d => d.Id))
            {
                var tip = d is Album ? "Album" : d is Pesma ? "Pesma" : "";
                var deloView = new MuzickoDeloView(d, tip)
                {
                    IsFavorit = favoritiSet.Contains(d.Id)
                };

                // IZMENA: Prati promene SAMO ako korisnik postoji
                if (_korisnik != null)
                {
                    deloView.PropertyChanged += DeloView_PropertyChanged;
                }

                Dela.Add(deloView);
            }
        }

        private void DeloView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // IZMENA: Ako nemamo korisnika ili repo (npr. Urednik gleda), ne radi ništa
            if (_korisnik == null || _korisnikRepo == null) return;

            if (e.PropertyName == nameof(MuzickoDeloView.IsFavorit) && sender is MuzickoDeloView deloView)
            {
                if (deloView.IsFavorit)
                {
                    if (!_korisnik.FavoritDeloIDs.Contains(deloView.Id))
                    {
                        _korisnik.FavoritDeloIDs.Add(deloView.Id);
                    }
                }
                else
                {
                    _korisnik.FavoritDeloIDs.Remove(deloView.Id);
                }

                _korisnikRepo.Update(_korisnik);
                _korisnikRepo.SaveChanges();
            }
        }

        private void OceniDelo(object? parameter)
        {
            if (parameter is MuzickoDeloView delo)
            {
                MessageBox.Show($"Ocenjujete delo: {delo.Naziv}", "Ocenjivanje", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}