using MusicCatalog.Models;
using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Models.Recenzije;
using MusicCatalog.Repositories;
using MusicCatalog.Services;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MusicCatalog.Models.Enums;
using StatusZahteva = MusicCatalog.Models.Recenzije.StatusZahteva;

namespace MusicCatalog.ViewModels
{
    public class KorisnikMuzickiSadrzajViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _delaRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly IRecenzijaRepository _recRepo;
        private readonly IOcenaRepository _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository _zahtevRepo;
        private readonly IKorisnikRepository _korisnikRepo;
        private readonly AuthService _auth;

        // Role-based switches for review actions
        private readonly bool _allowAddRecenzija;
        private readonly bool _allowEditRecenzija;

        // Optional bindings for XAML (hide buttons if you bind to these)
        public bool ShowDodajRecenziju => _allowAddRecenzija;
        public bool ShowIzmeniRecenziju => _allowEditRecenzija;

        public ObservableCollection<MuzickoDeloEntry> Dela { get; } = new();

        public ICommand OceniCommand { get; }
        public ICommand IzmeniOcenuCommand { get; }
        public ICommand PrikaziOceneCommand { get; }

        public KorisnikMuzickiSadrzajViewModel(
            IMuzickoDeloRepository delaRepo,
            IZanrRepository zanrRepo,
            IRecenzijaRepository recRepo,
            IOcenaRepository ocenaRepo,
            IZahtevZaIzmenuRepository zahtevRepo,
            IKorisnikRepository korisnikRepo,
            AuthService auth)
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;
            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _zahtevRepo = zahtevRepo;
            _korisnikRepo = korisnikRepo;
            _auth = auth;

            // Administrator can only view ratings
            var uloga = _auth.TrenutniKorisnik?.Uloga;
            var isAdmin = uloga == Uloga.Administrator;
            _allowAddRecenzija = !isAdmin;
            _allowEditRecenzija = !isAdmin;

            OceniCommand = new RelayCommand(p => DodajOcenu(p as MuzickoDeloEntry), p => CanDodajOcenu(p as MuzickoDeloEntry));
            IzmeniOcenuCommand = new RelayCommand(p => IzmeniOcenu(p as MuzickoDeloEntry), p => CanIzmeniOcenu(p as MuzickoDeloEntry));
            PrikaziOceneCommand = new RelayCommand(p => PrikaziOcene(p as MuzickoDeloEntry));

            Load();
        }

        private void Load()
        {
            Dela.Clear();
            var email = _auth.TrenutniKorisnik?.Email ?? string.Empty;

            foreach (var d in _delaRepo.GetAll().OrderBy(d => d.Id))
            {
                var tip = d is Album ? "Album" : d is Pesma ? "Pesma" : "";
                var view = new MuzickoDeloView(d, tip);

                Recenzija? userRec = null;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    try
                    {
                        userRec = _recRepo.GetByKorisnikAndDelo(email, d.Id);
                    }
                    catch
                    {
                        userRec = null;
                    }
                }

                Dela.Add(new MuzickoDeloEntry(view, userRec != null, userRec));
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanDodajOcenu(MuzickoDeloEntry? entry)
            => _allowAddRecenzija && entry != null && !entry.HasUserRecenzija;

        private bool CanIzmeniOcenu(MuzickoDeloEntry? entry)
            => _allowEditRecenzija && entry != null && entry.HasUserRecenzija && entry.Recenzija != null;

        private void DodajOcenu(MuzickoDeloEntry? entry)
        {
            if (entry == null) return;
            if (entry.HasUserRecenzija)
            {
                MessageBox.Show("Već ste ostavili recenziju. Možete je izmeniti.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new RecenzijaEditViewModel(_auth, _korisnikRepo, _recRepo, _ocenaRepo, _zahtevRepo, entry.View.Source, null);
            var view = new RecenzijaEditView { DataContext = vm };
            vm.Close = () =>
            {
                Load();
                var wnd = Window.GetWindow(view);
                wnd?.Close();
            };
            var window = new Window
            {
                Title = "Dodaj ocenu",
                Content = view,
                Width = 400,
                Height = 320,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        private void IzmeniOcenu(MuzickoDeloEntry? entry)
        {
            if (entry == null) return;
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
            catch
            {
                hasPendingOrRejected = false;
            }

            if (hasPendingOrRejected)
            {
                MessageBox.Show("Već postoji zahtev (NA_CEKANJU ili ODBIJEN) za izmenu/brisanje ove recenzije.", "Obaveštenje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var vm = new RecenzijaEditViewModel(_auth, _korisnikRepo, _recRepo, _ocenaRepo, _zahtevRepo, entry.View.Source, entry.Recenzija);
            var view = new RecenzijaEditView { DataContext = vm };
            vm.Close = () =>
            {
                Load();
                var wnd = Window.GetWindow(view);
                wnd?.Close();
            };
            var window = new Window
            {
                Title = "Izmeni ocenu",
                Content = view,
                Width = 420,
                Height = 360,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        private void PrikaziOcene(MuzickoDeloEntry? entry)
        {
            if (entry == null) return;
            var vm = new PrikaziOceneViewModel(_korisnikRepo, _recRepo, _ocenaRepo, entry.View.Source);
            var view = new PrikaziOceneView { DataContext = vm };
            var window = new Window
            {
                Title = $"Ocene — {entry.View.Naziv}",
                Content = view,
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        public class MuzickoDeloEntry
        {
            public MuzickoDeloView View { get; }
            public bool HasUserRecenzija { get; }
            public Recenzija? Recenzija { get; }
            public MuzickoDeloEntry(MuzickoDeloView view, bool has, Recenzija? rec)
            {
                View = view; HasUserRecenzija = has; Recenzija = rec;
            }
        }
    }
}