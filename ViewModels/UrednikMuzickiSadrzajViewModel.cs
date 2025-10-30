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
using System.Windows.Media;

namespace MusicCatalog.ViewModels
{
    public class UrednikMuzickiSadrzajViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _delaRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly IRecenzijaRepository _recRepo;
        private readonly IOcenaRepository _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository _zahtevRepo;
        private readonly IKorisnikRepository _korisnikRepo;
        private readonly AuthService _auth;
        private readonly IMuzickiUmetnikRepository? _umetnikRepo;

        public ObservableCollection<UrednikMuzickoDeloEntry> Dela { get; } = new();

        public ICommand AddPesmaCommand { get; }
        public ICommand AddAlbumCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public ICommand OceniCommand { get; }
        public ICommand IzmeniOcenuCommand { get; }
        public ICommand PrikaziOceneCommand { get; }

        public UrednikMuzickiSadrzajViewModel(
        IMuzickoDeloRepository delaRepo,
        IZanrRepository zanrRepo,
        IRecenzijaRepository recRepo,
        IOcenaRepository ocenaRepo,
        IZahtevZaIzmenuRepository zahtevRepo,
        IKorisnikRepository korisnikRepo,
        AuthService auth,
        IMuzickiUmetnikRepository? umetnikRepo = null)
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;
            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _zahtevRepo = zahtevRepo;
            _korisnikRepo = korisnikRepo;
            _auth = auth;
            _umetnikRepo = umetnikRepo;

            AddPesmaCommand = new RelayCommand(_ => OpenEdit(false));
            AddAlbumCommand = new RelayCommand(_ => OpenEdit(true));
            EditCommand = new RelayCommand(p =>
            {
                var mv = p as MuzickoDeloView;
                if (mv == null && p is UrednikMuzickoDeloEntry entry) mv = entry.View;
                if (mv == null) return;
                OpenEdit(mv.Source is Album, mv.Id);
            }, p => p is MuzickoDeloView || p is UrednikMuzickoDeloEntry);

            DeleteCommand = new RelayCommand(p =>
            {
                var mv = p as MuzickoDeloView;
                if (mv == null && p is UrednikMuzickoDeloEntry entry) mv = entry.View;
                if (mv == null) return;
                Delete(mv);
            }, p => p is MuzickoDeloView || p is UrednikMuzickoDeloEntry);

            OceniCommand = new RelayCommand(p => DodajOcenu(p as UrednikMuzickoDeloEntry));
            IzmeniOcenuCommand = new RelayCommand(p => IzmeniOcenu(p as UrednikMuzickoDeloEntry));
            PrikaziOceneCommand = new RelayCommand(p => PrikaziOcene(p as UrednikMuzickoDeloEntry));

            Load();
        }

        private void Load()
        {
            Dela.Clear();
            var email = _auth.TrenutniKorisnik?.Email ?? string.Empty;

            foreach (var d in _delaRepo.GetAll().OrderBy(d => d.Id))
            {
                var tip = d is Album ? "Album" : d is Pesma ? "Pesma" : string.Empty;
                var view = new MuzickoDeloView(d, tip);

                Recenzija? userRec = null;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    try { userRec = _recRepo.GetByKorisnikAndDelo(email, d.Id); }
                    catch { userRec = null; }
                }

                Dela.Add(new UrednikMuzickoDeloEntry(view, userRec != null, userRec));
            }
        }

        private void OpenEdit(bool isAlbum, int? id = null)
        {
            var vm = new MuzickoDeloEditViewModel(_delaRepo, _zanrRepo, _umetnikRepo!, isAlbum, id);
            Window v = isAlbum
            ? new AlbumEditView { DataContext = vm }
            : new MuzickoDeloEditView { DataContext = vm };
            vm.OnSaved = () => { v.Close(); Load(); };
            vm.OnCancelled = () => v.Close();
            v.ShowDialog();
        }

        private void Delete(MuzickoDeloView mv)
        {
            var md = mv.Source;
            if (MessageBox.Show($"Obrisati '{md.Naziv}'?", "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            _delaRepo.DeleteWithCascade(md.Id, _recRepo, _ocenaRepo, _zahtevRepo);
            Load();
        }

        private void DodajOcenu(UrednikMuzickoDeloEntry? entry)
        {
            if (entry == null) return;
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

        private void IzmeniOcenu(UrednikMuzickoDeloEntry? entry)
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

        private void PrikaziOcene(UrednikMuzickoDeloEntry? entry)
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
                Owner = Application.Current.MainWindow,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStyle = WindowStyle.None
            };

            vm.CloseWindow = () => window.Close();

            
            window.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) window.DragMove(); };

            window.ShowDialog();
        }
        

        public class UrednikMuzickoDeloEntry
        {
            public MuzickoDeloView View { get; }
            public bool HasUserRecenzija { get; }
            public Recenzija? Recenzija { get; }
            public UrednikMuzickoDeloEntry(MuzickoDeloView view, bool has, Recenzija? rec)
            {
                View = view; HasUserRecenzija = has; Recenzija = rec;
            }
        }
    }
}