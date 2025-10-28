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
 private double _trajanjeMin =3;
 public double TrajanjeMin { get => _trajanjeMin; set { _trajanjeMin = value; OnPropertyChanged(nameof(TrajanjeMin)); UpdateCanSave(); } }
 private DateTime _datumIzdanja = DateTime.Today;
 public DateTime DatumIzdanja { get => _datumIzdanja; set { _datumIzdanja = value; OnPropertyChanged(nameof(DatumIzdanja)); UpdateCanSave(); } }
 public ObservableCollection<ZanrOption> Zanrovi { get; } = new();

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

 private void UpdateCanSave() => CommandManager.InvalidateRequerySuggested();
 private bool CanSave()
 {
 return !string.IsNullOrWhiteSpace(Naziv) && TrajanjeMin >0 && DatumIzdanja <= DateTime.Today && Zanrovi.Any(z => z.IsSelected);
 }

 private void Save()
 {
 ErrorMessage = string.Empty;
 var ids = Zanrovi.Where(z => z.IsSelected).Select(z => z.Id).ToList();
 var traj = TimeSpan.FromMinutes(TrajanjeMin);
 try
 {
 MuzickoDelo entity;
 if (_id.HasValue)
 {
 entity = _deloRepo.GetById(_id.Value)!;
 entity.Naziv = Naziv;
 entity.Trajanje = traj;
 entity.DatumIzdanja = DatumIzdanja;
 entity.ZanrIDs = ids;
 _deloRepo.Update(entity);
 }
 else
 {
 entity = _isAlbum ? new Album(0, Naziv, traj, DatumIzdanja, ids) : new Pesma(0, Naziv, traj, DatumIzdanja, ids);
 _deloRepo.Add(entity);
 }
 OnSaved?.Invoke();
 }
 catch (Exception ex)
 {
 ErrorMessage = ex.Message;
 }
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
}
