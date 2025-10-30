using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using MusicCatalog.Models.Umetnici;

namespace MusicCatalog.ViewModels
{
    public class MuzickoDeloEditViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly IMuzickiUmetnikRepository _umetnikRepo;
        private readonly bool _isAlbum;
        private readonly int? _id;

        public string TipText => _isAlbum ? "Album" : "Pesma";

        private string _naziv = string.Empty;
        public string Naziv { get => _naziv; set { _naziv = value; OnPropertyChanged(nameof(Naziv)); UpdateCanSave(); } }

        // === IZMENA: Uklonjeno podrazumevano trajanje, set-er je sada javan ===
        private double _trajanjeMin;
        public double TrajanjeMin { get => _trajanjeMin; set { SetField(ref _trajanjeMin, value); UpdateCanSave(); } }
        // =================================================================

        private DateTime _datumIzdanja = DateTime.Today;
        public DateTime DatumIzdanja { get => _datumIzdanja; set { _datumIzdanja = value; OnPropertyChanged(nameof(DatumIzdanja)); UpdateCanSave(); } }

        public ObservableCollection<ZanrOption> Zanrovi { get; } = new();

        public ObservableCollection<PesmaOption> Pesme { get; } = new();

        public ObservableCollection<UmetnikOption> Umetnici { get; } = new();

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnSaved { get; set; }
        public Action? OnCancelled { get; set; }

        public MuzickoDeloEditViewModel(IMuzickoDeloRepository deloRepo, IZanrRepository zanrRepo, IMuzickiUmetnikRepository umetnikRepo, bool isAlbum, int? id = null)
        {
            _deloRepo = deloRepo;
            _zanrRepo = zanrRepo;
            _umetnikRepo = umetnikRepo;
            _isAlbum = isAlbum;
            _id = id;
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());
            Load();
        }

        private void Load()
        {
            if (_isAlbum)
            {
                Pesme.Clear();
                var svePesme = _deloRepo.GetAll().OfType<Pesma>().OrderBy(p => p.Naziv).ToList();
                foreach (var p in svePesme)
                {
                    // === IZMENA: Prosleđujemo trajanje i callback funkciju ===
                    Pesme.Add(new PesmaOption(p.Id, p.Naziv, p.Trajanje, RecalculateAlbumDuration));
                }

                if (_id.HasValue)
                {
                    var md = _deloRepo.GetById(_id.Value) as Album;
                    if (md != null)
                    {
                        _naziv = md.Naziv;
                        _trajanjeMin = md.Trajanje.TotalMinutes;
                        _datumIzdanja = md.DatumIzdanja;

                        var setPesama = md.PesmaIDs?.ToHashSet() ?? new HashSet<int>();
                        foreach (var p in Pesme)
                        {
                            p.IsSelected = setPesama.Contains(p.Id);
                        }

                        // === DODATO: Preračunaj trajanje odmah nakon učitavanja ===
                        RecalculateAlbumDuration();
                    }
                }
            }
            else // Ista logika kao pre za Pesme
            {
                var allZ = _zanrRepo.GetAll();
                Zanrovi.Clear();
                foreach (var z in allZ)
                    Zanrovi.Add(new ZanrOption(z.Id, z.Naziv));

                Umetnici.Clear();
                foreach (var u in _umetnikRepo.GetAll().OrderBy(u => u is Bend ? (u as Bend)!.Naziv : (u as Izvodjac)!.Ime))
                {
                    var display = u is Bend b ? b.Naziv : u is Izvodjac i ? ($"{i.Ime} {i.Prezime}") : $"Umetnik {u.Id}";
                    Umetnici.Add(new UmetnikOption(u.Id, display));
                }

                if (_id.HasValue)
                {
                    var md = _deloRepo.GetById(_id.Value);
                    if (md != null)
                    {
                        _naziv = md.Naziv;
                        _trajanjeMin = md.Trajanje.TotalMinutes; // Postavi inicijalno trajanje za pesmu
                        _datumIzdanja = md.DatumIzdanja;
                        foreach (var z in Zanrovi) z.IsSelected = md.ZanrIDs.Contains(z.Id);
                        var setU = (md.UmetnikIDs ?? new List<int>()).ToHashSet();
                        foreach (var u in Umetnici) u.IsSelected = setU.Contains(u.Id);
                    }
                }
                else
                {
                    _trajanjeMin = 3; // Podrazumevano za novu pesmu
                }
            }
            OnPropertyChanged(nameof(Naziv));
            OnPropertyChanged(nameof(TrajanjeMin));
            OnPropertyChanged(nameof(DatumIzdanja));
        }

        // === DODATO: Novi metod za kalkulaciju ===
        private void RecalculateAlbumDuration()
        {
            if (!_isAlbum) return; // Radi samo za albume

            var totalDuration = TimeSpan.Zero;
            foreach (var p in Pesme.Where(p => p.IsSelected))
            {
                totalDuration += p.Trajanje;
            }
            TrajanjeMin = totalDuration.TotalMinutes; // Ovo će automatski ažurirati UI
        }
        // =========================================

        private void UpdateCanSave() => CommandManager.InvalidateRequerySuggested();
        private bool CanSave()
        {
            if (_isAlbum)
            {
                return !string.IsNullOrWhiteSpace(Naziv)
                && DatumIzdanja <= DateTime.Today
                && Pesme.Any(p => p.IsSelected); // Trajanje se sada samo računa, ali mora biti bar jedna pesma
            }
            else
            {
                return !string.IsNullOrWhiteSpace(Naziv)
                && TrajanjeMin > 0
                && DatumIzdanja <= DateTime.Today
                && Zanrovi.Any(z => z.IsSelected);
            }
        }

        private void Save()
        {
            ErrorMessage = string.Empty;

            // Trajanje je već izračunato i nalazi se u TrajanjeMin
            var traj = TimeSpan.FromMinutes(TrajanjeMin);

            try
            {
                if (_id.HasValue)
                {
                    var existing = _deloRepo.GetById(_id.Value)!;

                    if (_isAlbum && existing is Album alb)
                    {
                        var selectedPesmaIds = Pesme.Where(p => p.IsSelected).Select(p => p.Id).ToList();
                        var distinctZanrIds = ComputeAlbumGenresFromSongs(selectedPesmaIds);
                        var distinctUmetnikIds = ComputeAlbumArtistsFromSongs(selectedPesmaIds);

                        alb.Naziv = Naziv;
                        alb.Trajanje = traj; // Koristi izračunato trajanje
                        alb.DatumIzdanja = DatumIzdanja;
                        alb.PesmaIDs = selectedPesmaIds;
                        alb.ZanrIDs = distinctZanrIds;
                        alb.UmetnikIDs = distinctUmetnikIds;

                        _deloRepo.Update(alb);
                    }
                    else
                    {
                        var ids = Zanrovi.Where(z => z.IsSelected).Select(z => z.Id).ToList();
                        var umetnikIds = Umetnici.Where(u => u.IsSelected).Select(u => u.Id).ToList();
                        existing.Naziv = Naziv;
                        existing.Trajanje = traj;
                        existing.DatumIzdanja = DatumIzdanja;
                        existing.ZanrIDs = ids;
                        existing.UmetnikIDs = umetnikIds;
                        _deloRepo.Update(existing);
                    }
                }
                else
                {
                    if (_isAlbum)
                    {
                        var selectedPesmaIds = Pesme.Where(p => p.IsSelected).Select(p => p.Id).ToList();
                        var distinctZanrIds = ComputeAlbumGenresFromSongs(selectedPesmaIds);
                        var distinctUmetnikIds = ComputeAlbumArtistsFromSongs(selectedPesmaIds);

                        var entity = new Album(0, Naziv, traj, DatumIzdanja, distinctZanrIds, selectedPesmaIds)
                        {
                            UmetnikIDs = distinctUmetnikIds
                        };
                        _deloRepo.Add(entity);
                    }
                    else
                    {
                        var ids = Zanrovi.Where(z => z.IsSelected).Select(z => z.Id).ToList();
                        var umetnikIds = Umetnici.Where(u => u.IsSelected).Select(u => u.Id).ToList();
                        MuzickoDelo entity = new Pesma(0, Naziv, traj, DatumIzdanja, ids)
                        {
                            UmetnikIDs = umetnikIds
                        };
                        _deloRepo.Add(entity);
                    }
                }

                OnSaved?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private List<int> ComputeAlbumGenresFromSongs(List<int> selectedPesmaIds)
        {
            if (selectedPesmaIds.Count == 0) return new List<int>();
            var pesme = _deloRepo.GetAll().OfType<Pesma>().Where(p => selectedPesmaIds.Contains(p.Id));
            return pesme.SelectMany(p => p.ZanrIDs).Distinct().ToList();
        }

        private List<int> ComputeAlbumArtistsFromSongs(List<int> selectedPesmaIds)
        {
            if (selectedPesmaIds.Count == 0) return new List<int>();
            var pesme = _deloRepo.GetAll().Where(p => selectedPesmaIds.Contains(p.Id));
            return pesme.SelectMany(p => p.UmetnikIDs ?? new List<int>()).Distinct().ToList();
        }
    }

    public class ZanrOption : ViewModelBase
    {
        public int Id { get; }
        public string Naziv { get; }
        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        public ZanrOption(int id, string naziv) { Id = id; Naziv = naziv; }
    }

    // === IZMENA: PesmaOption sada sadrži Trajanje i callback ===
    public class PesmaOption : ViewModelBase
    {
        public int Id { get; }
        public string Naziv { get; }
        public TimeSpan Trajanje { get; } // Čuva trajanje pesme
        private readonly Action? _onSelectionChanged; // Callback
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetField(ref _isSelected, value))
                {
                    _onSelectionChanged?.Invoke(); // Pozovi callback
                }
            }
        }

        public PesmaOption(int id, string naziv, TimeSpan trajanje, Action? onSelectionChanged = null)
        {
            Id = id;
            Naziv = naziv;
            Trajanje = trajanje;
            _onSelectionChanged = onSelectionChanged;
        }
    }
    // ========================================================

    public class UmetnikOption : ViewModelBase
    {
        public int Id { get; }
        public string Naziv { get; }
        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        public UmetnikOption(int id, string naziv) { Id = id; Naziv = naziv; }
    }
}