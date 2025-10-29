using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class MuzickoDeloEditViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _deloRepo;
        private readonly IZanrRepository _zanrRepo;
        private readonly bool _isAlbum;
        private readonly int? _id;

        public string TipText => _isAlbum ? "Album" : "Pesma";

        private string _naziv = string.Empty;
        public string Naziv { get => _naziv; set { _naziv = value; OnPropertyChanged(nameof(Naziv)); UpdateCanSave(); } }
        private double _trajanjeMin = 3;
        public double TrajanjeMin { get => _trajanjeMin; set { _trajanjeMin = value; OnPropertyChanged(nameof(TrajanjeMin)); UpdateCanSave(); } }
        private DateTime _datumIzdanja = DateTime.Today;
        public DateTime DatumIzdanja { get => _datumIzdanja; set { _datumIzdanja = value; OnPropertyChanged(nameof(DatumIzdanja)); UpdateCanSave(); } }

        public ObservableCollection<ZanrOption> Zanrovi { get; } = new();

        public ObservableCollection<PesmaOption> Pesme { get; } = new();

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action? OnSaved { get; set; }
        public Action? OnCancelled { get; set; }

        public MuzickoDeloEditViewModel(IMuzickoDeloRepository deloRepo, IZanrRepository zanrRepo, bool isAlbum, int? id = null)
        {
            _deloRepo = deloRepo;
            _zanrRepo = zanrRepo;
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
                // Load all songs to choose from
                Pesme.Clear();
                var svePesme = _deloRepo.GetAll().OfType<Pesma>().OrderBy(p => p.Naziv).ToList();
                foreach (var p in svePesme)
                {
                    Pesme.Add(new PesmaOption(p.Id, p.Naziv));
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
                        foreach (var p in Pesme) p.IsSelected = setPesama.Contains(p.Id);
                    }
                }
            }
            else
            {
                var allZ = _zanrRepo.GetAll();
                Zanrovi.Clear();
                foreach (var z in allZ)
                    Zanrovi.Add(new ZanrOption(z.Id, z.Naziv));

                if (_id.HasValue)
                {
                    var md = _deloRepo.GetById(_id.Value);
                    if (md != null)
                    {
                        _naziv = md.Naziv;
                        _trajanjeMin = md.Trajanje.TotalMinutes;
                        _datumIzdanja = md.DatumIzdanja;
                        foreach (var z in Zanrovi) z.IsSelected = md.ZanrIDs.Contains(z.Id);
                    }
                }
            }
        }

        private void UpdateCanSave() => CommandManager.InvalidateRequerySuggested();
        private bool CanSave()
        {
            if (_isAlbum)
            {
                return !string.IsNullOrWhiteSpace(Naziv)
                && TrajanjeMin > 0
                && DatumIzdanja <= DateTime.Today
                && Pesme.Any(p => p.IsSelected);
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

                        alb.Naziv = Naziv;
                        alb.Trajanje = traj;
                        alb.DatumIzdanja = DatumIzdanja;
                        alb.PesmaIDs = selectedPesmaIds;
                        alb.ZanrIDs = distinctZanrIds;

                        _deloRepo.Update(alb);
                    }
                    else
                    {
                        // Pesma edit
                        var ids = Zanrovi.Where(z => z.IsSelected).Select(z => z.Id).ToList();
                        existing.Naziv = Naziv;
                        existing.Trajanje = traj;
                        existing.DatumIzdanja = DatumIzdanja;
                        existing.ZanrIDs = ids;
                        _deloRepo.Update(existing);
                    }
                }
                else
                {
                    if (_isAlbum)
                    {
                        var selectedPesmaIds = Pesme.Where(p => p.IsSelected).Select(p => p.Id).ToList();
                        var distinctZanrIds = ComputeAlbumGenresFromSongs(selectedPesmaIds);

                        var entity = new Album(0, Naziv, traj, DatumIzdanja, distinctZanrIds, selectedPesmaIds);
                        _deloRepo.Add(entity);
                    }
                    else
                    {
                        var ids = Zanrovi.Where(z => z.IsSelected).Select(z => z.Id).ToList();
                        MuzickoDelo entity = new Pesma(0, Naziv, traj, DatumIzdanja, ids);
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
    }

    public class ZanrOption : ViewModelBase
    {
        public int Id { get; }
        public string Naziv { get; }
        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        public ZanrOption(int id, string naziv) { Id = id; Naziv = naziv; }
    }

    public class PesmaOption : ViewModelBase
    {
        public int Id { get; }
        public string Naziv { get; }
        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        public PesmaOption(int id, string naziv) { Id = id; Naziv = naziv; }
    }
}
