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
using MusicCatalog.Models.Recenzije;
using MusicCatalog.Services; 
using System.Windows.Media; 
using System.Collections.Generic;
using MusicCatalog.Views; 

namespace MusicCatalog.ViewModels
{
 
    public class KorisnikMuzickoDeloEntry : ViewModelBase
    {
        public MuzickoDeloView View { get; }
        public bool HasUserRecenzija { get; }
        public Recenzija? Recenzija { get; }
        public KorisnikMuzickoDeloEntry(MuzickoDeloView view, bool has, Recenzija? rec)
        {
            View = view; HasUserRecenzija = has; Recenzija = rec;
        }
    }


    public class KorisnikMuzickiSadrzajViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _delaRepo;
        private readonly IZanrRepository _zanrRepo;

        private readonly RegistrovaniKorisnik? _korisnik;
        private readonly IKorisnikRepository? _korisnikRepo;


        private readonly AuthService? _auth;
        private readonly IRecenzijaRepository? _recRepo;
        private readonly IOcenaRepository? _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository? _zahtevRepo;

        public ObservableCollection<KorisnikMuzickoDeloEntry> Dela { get; } = new();
        private KorisnikMuzickoDeloEntry? _selected;
        public KorisnikMuzickoDeloEntry? Selected { get => _selected; set { _selected = value; OnPropertyChanged(nameof(Selected)); } }
   
        public bool CanManageFavorites => _korisnik != null;
        public bool CanManageRecenzije => _korisnik != null && _recRepo != null; 

        public ICommand OceniCommand { get; }
        public ICommand IzmeniOcenuCommand { get; }
        public ICommand PrikaziOceneCommand { get; }

        public KorisnikMuzickiSadrzajViewModel(IMuzickoDeloRepository delaRepo, IZanrRepository zanrRepo)
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;
            _korisnik = null;
            _korisnikRepo = null;

            _auth = null;
            _recRepo = null;
            _ocenaRepo = null;
            _zahtevRepo = null;

            OceniCommand = new RelayCommand(_ => { });
            IzmeniOcenuCommand = new RelayCommand(_ => { });
            PrikaziOceneCommand = new RelayCommand(_ => { });

            Load();
        }

        public KorisnikMuzickiSadrzajViewModel(
            IMuzickoDeloRepository delaRepo,
            IZanrRepository zanrRepo,
            RegistrovaniKorisnik korisnik,
            IKorisnikRepository korisnikRepo,
            AuthService auth, 
            IRecenzijaRepository recRepo, 
            IOcenaRepository ocenaRepo,
            IZahtevZaIzmenuRepository zahtevRepo
            )
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;
            _korisnik = korisnik;
            _korisnikRepo = korisnikRepo;

            _auth = auth;
            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _zahtevRepo = zahtevRepo;

            OceniCommand = new RelayCommand(p => DodajOcenu(p as KorisnikMuzickoDeloEntry));
            IzmeniOcenuCommand = new RelayCommand(p => IzmeniOcenu(p as KorisnikMuzickoDeloEntry));
            PrikaziOceneCommand = new RelayCommand(p => PrikaziOcene(p as KorisnikMuzickoDeloEntry));

            Load();
        }

        private void Load()
        {
            foreach (var d in Dela)
            {
                d.View.PropertyChanged -= DeloView_PropertyChanged;
            }
            Dela.Clear();

            var favoritiSet = _korisnik?.FavoritDeloIDs.ToHashSet() ?? new HashSet<int>();
            var email = _korisnik?.Email ?? string.Empty;

            foreach (var d in _delaRepo.GetAll().OrderBy(d => d.Id))
            {
                var tip = d is Album ? "Album" : d is Pesma ? "Pesma" : "";
                var deloView = new MuzickoDeloView(d, tip);

                deloView.IsFavorit = favoritiSet.Contains(d.Id);

                if (_korisnik != null)
                {
                    deloView.PropertyChanged += DeloView_PropertyChanged;
                }

                Recenzija? userRec = null;
                if (!string.IsNullOrWhiteSpace(email) && _recRepo != null)
                {
                    try { userRec = _recRepo.GetByKorisnikAndDelo(email, d.Id); }
                    catch { userRec = null; }
                }

                Dela.Add(new KorisnikMuzickoDeloEntry(deloView, userRec != null, userRec));
            }
        }

        private void DeloView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
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


        private void DodajOcenu(KorisnikMuzickoDeloEntry? entry)
        {
            if (entry == null || _auth == null || _korisnikRepo == null || _recRepo == null || _ocenaRepo == null || _zahtevRepo == null) return;

            if (entry.HasUserRecenzija)
            {
                MessageBox.Show("Već ste ostavili recenziju. Možete je izmeniti.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new RecenzijaEditViewModel(_auth, _korisnikRepo, _recRepo, _ocenaRepo, _zahtevRepo, entry.View.Source, null);
            var view = new RecenzijaEditView { DataContext = vm };

            var window = new Window
            {
                Title = "Dodaj ocenu",
                Content = view,
                Width = 420,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStyle = WindowStyle.None
            };

            vm.Close = () => window.Close();
            window.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) window.DragMove(); };

            window.ShowDialog();
            Load();
        }

        private void IzmeniOcenu(KorisnikMuzickoDeloEntry? entry)
        {
            if (entry == null || _auth == null || _korisnikRepo == null || _recRepo == null || _ocenaRepo == null || _zahtevRepo == null) return;

            if (!entry.HasUserRecenzija || entry.Recenzija == null)
            {
                MessageBox.Show("Nemate postojeću recenziju za ovo delo.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool hasPendingOrRejected = false;
            try
            {
                var existingReq = _zahtevRepo.GetByRecenzijaId(entry.Recenzija.Id)
                .FirstOrDefault(z => z.Status != StatusZahteva.PRIHVACEN);
                hasPendingOrRejected = existingReq != null;
            }
            catch { hasPendingOrRejected = false; }

            if (hasPendingOrRejected)
            {
                MessageBox.Show("Već postoji zahtev (NA_CEKANJU ili ODBIJEN) za izmenu/brisanje ove recenzije.", "Obaveštenje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new RecenzijaEditViewModel(_auth, _korisnikRepo, _recRepo, _ocenaRepo, _zahtevRepo, entry.View.Source, entry.Recenzija);
            var view = new RecenzijaEditView { DataContext = vm };

            var window = new Window
            {
                Title = "Izmeni ocenu",
                Content = view,
                Width = 420,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStyle = WindowStyle.None
            };

            vm.Close = () => window.Close();
            window.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) window.DragMove(); };

            window.ShowDialog();
            Load();
        }

        private void PrikaziOcene(KorisnikMuzickoDeloEntry? entry)
        {
            if (entry == null || _korisnikRepo == null || _recRepo == null || _ocenaRepo == null) return;

            var vm = new PrikaziOceneViewModel(_korisnikRepo, _recRepo, _ocenaRepo, entry.View.Source);
            var view = new PrikaziOceneView { DataContext = vm };
            var window = new Window
            {
                Title = $"Ocene — {entry.View.Naziv}",
                Content = view,
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStyle = WindowStyle.None
            };

            vm.CloseWindow = () => window.Close();
            window.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) window.DragMove(); };

            window.ShowDialog();
        }
        
    }
}