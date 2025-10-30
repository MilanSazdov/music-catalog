using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace MusicCatalog.ViewModels
{
    public class AdminMuzickiSadrzajViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _delaRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly IMuzickiUmetnikRepository? _umetnikRepo;

        // Inject these for cascade delete
        private readonly IRecenzijaRepository? _recRepo;
        private readonly IOcenaRepository? _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository? _zahtevRepo;

        public ObservableCollection<MuzickoDeloView> Dela { get; } = new();
        private MuzickoDeloView? _selected;
        public MuzickoDeloView? Selected { get => _selected; set { _selected = value; OnPropertyChanged(nameof(Selected)); CommandManager.InvalidateRequerySuggested(); } }

        public ICommand AddPesmaCommand { get; }
        public ICommand AddAlbumCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public AdminMuzickiSadrzajViewModel(IMuzickoDeloRepository delaRepo, IZanrRepository zanrRepo, IMuzickiUmetnikRepository? umetnikRepo = null)
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;
            _umetnikRepo = umetnikRepo;

            AddPesmaCommand = new RelayCommand(_ => OpenEdit(false));
            AddAlbumCommand = new RelayCommand(_ => OpenEdit(true));
            EditCommand = new RelayCommand(p =>
            {
                var item = p as MuzickoDeloView ?? Selected;
                OpenEdit(item?.Source is Album, item?.Id);
            }, p => (p as MuzickoDeloView) != null || Selected != null);
            DeleteCommand = new RelayCommand(p =>
            {
                var item = p as MuzickoDeloView ?? Selected;
                if (item == null) return;
                Selected = item;
                DeleteSelected();
            }, p => (p as MuzickoDeloView) != null || Selected != null);

            Load();
        }

        public AdminMuzickiSadrzajViewModel(IMuzickoDeloRepository delaRepo, IZanrRepository zanrRepo, IRecenzijaRepository recRepo, IOcenaRepository ocenaRepo, IZahtevZaIzmenuRepository zahtevRepo, IMuzickiUmetnikRepository? umetnikRepo = null)
        : this(delaRepo, zanrRepo, umetnikRepo)
        {
            _recRepo = recRepo; _ocenaRepo = ocenaRepo; _zahtevRepo = zahtevRepo;
        }

        private void Load()
        {
            Dela.Clear();
            foreach (var d in _delaRepo.GetAll().OrderBy(d => d.Id))
            {
                var tip = d is Album ? "Album" : d is Pesma ? "Pesma" : "";
                Dela.Add(new MuzickoDeloView(d, tip));
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

        private void DeleteSelected()
        {
            if (Selected == null) return;
            var md = Selected.Source;
            if (MessageBox.Show($"Obrisati '{md.Naziv}'?", "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            if (_recRepo != null && _ocenaRepo != null && _zahtevRepo != null)
            {
                _delaRepo.DeleteWithCascade(md.Id, _recRepo, _ocenaRepo, _zahtevRepo);
            }
            else
            {
                _delaRepo.Delete(md.Id);
            }
            Load();
        }
    }

    public class MuzickoDeloView : ViewModelBase
    {
        public int Id => Source.Id;
        public string Naziv => Source.Naziv;
        public TimeSpan Trajanje => Source.Trajanje;
        public DateTime DatumIzdanja => Source.DatumIzdanja;
        public string Tip { get; }
        public MuzickoDelo Source { get; }
        public string Slika => Source.Slika;

        private bool _isFavorit;
        public bool IsFavorit
        {
            get => _isFavorit;
            set => SetField(ref _isFavorit, value); // Koristi SetField da bi se UI ažurirao
        }

        public string ImagePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Slika)) return string.Empty;
                // If already rooted, return as is
                if (Path.IsPathRooted(Slika)) return Slika;
                // If already under Data, keep it, otherwise combine with Data folder
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var candidate = Slika.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                if (candidate.StartsWith("Data" + Path.DirectorySeparatorChar))
                {
                    return Path.GetFullPath(Path.Combine(baseDir, candidate));
                }
                return Path.GetFullPath(Path.Combine(baseDir, "Data", candidate));
            }
        }
        public MuzickoDeloView(MuzickoDelo src, string tip)
        {
            Source = src; Tip = tip;
        }
    }
}
