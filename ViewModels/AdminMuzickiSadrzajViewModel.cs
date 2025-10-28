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

 public ObservableCollection<MuzickoDeloView> Dela { get; } = new();
 private MuzickoDeloView? _selected;
 public MuzickoDeloView? Selected { get => _selected; set { _selected = value; OnPropertyChanged(nameof(Selected)); CommandManager.InvalidateRequerySuggested(); } }

 public ICommand AddPesmaCommand { get; }
 public ICommand AddAlbumCommand { get; }
 public ICommand EditCommand { get; }
 public ICommand DeleteCommand { get; }

 public AdminMuzickiSadrzajViewModel(IMuzickoDeloRepository delaRepo, IZanrRepository zanrRepo)
 {
 _delaRepo = delaRepo;
 _zanrRepo = zanrRepo;

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
 var vm = new MuzickoDeloEditViewModel(_delaRepo, _zanrRepo, isAlbum, id);
 var v = new MuzickoDeloEditView { DataContext = vm };
 vm.OnSaved = () => { v.Close(); Load(); };
 vm.OnCancelled = () => v.Close();
 v.ShowDialog();
 }

 private void DeleteSelected()
 {
 if (Selected == null) return;
 var md = Selected.Source;
 if (MessageBox.Show($"Obrisati '{md.Naziv}'?", "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
 // remove from all genres lists first
 foreach (var z in _zanrRepo.GetAll())
 {
 if (z.MuzickaDelaIDs.Remove(md.Id))
 {
 _zanrRepo.Update(z);
 }
 }
 _delaRepo.Delete(md.Id);
 Load();
 }
 }

 public class MuzickoDeloView
 {
 public int Id => Source.Id;
 public string Naziv => Source.Naziv;
 public TimeSpan Trajanje => Source.Trajanje;
 public DateTime DatumIzdanja => Source.DatumIzdanja;
 public string Tip { get; }
 public MuzickoDelo Source { get; }
 public string Slika => Source.Slika;
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
