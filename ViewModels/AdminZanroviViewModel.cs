using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
 public class AdminZanroviViewModel : ViewModelBase
 {
 private readonly IZanrRepository _repo;
 public ObservableCollection<ZanrView> Zanrovi { get; } = new();
 private ZanrView? _selected;
 public ZanrView? Selected { get => _selected; set { _selected = value; OnPropertyChanged(nameof(Selected)); RefreshDetails(); CommandManager.InvalidateRequerySuggested(); } }

 public ObservableCollection<ZanrItem> Podzanrovi { get; } = new();
 public ObservableCollection<ZanrItem> DostupniPodzanrovi { get; } = new();

 private bool _prikaziDetalje;
 public bool PrikaziDetalje { get => _prikaziDetalje; set { _prikaziDetalje = value; OnPropertyChanged(nameof(PrikaziDetalje)); } }
 private bool _dodavanjePodzanra;
 public bool DodavanjePodzanra { get => _dodavanjePodzanra; set { _dodavanjePodzanra = value; OnPropertyChanged(nameof(DodavanjePodzanra)); } }

 public ICommand AddCommand { get; }
 public ICommand EditCommand { get; }
 public ICommand DeleteCommand { get; }
 public ICommand ShowCommand { get; }
 public ICommand AddPodzanrCommand { get; }
 public ICommand RemovePodzanrCommand { get; }
 public ICommand AddPodzanrFromListCommand { get; }
 public ICommand ZatvoriDodavanjeCommand { get; }

 public AdminZanroviViewModel(IZanrRepository repo)
 {
 _repo = repo;
 AddCommand = new RelayCommand(_ => Add());
 EditCommand = new RelayCommand(_ => Edit(), _ => Selected != null);
 DeleteCommand = new RelayCommand(_ => Delete(), _ => Selected != null);
 ShowCommand = new RelayCommand(p => { PrikaziDetalje = true; RefreshDetails(); }, _ => Selected != null);
 AddPodzanrCommand = new RelayCommand(_ => { DodavanjePodzanra = true; RefreshAvailable(); }, _ => Selected != null);
 RemovePodzanrCommand = new RelayCommand(p => RemovePodzanr(p as ZanrItem));
 AddPodzanrFromListCommand = new RelayCommand(p => AddPodzanrFromList(p as ZanrItem));
 ZatvoriDodavanjeCommand = new RelayCommand(_ => DodavanjePodzanra = false);
 Load();
 }

 private void Load()
 {
 Zanrovi.Clear();
 foreach (var z in _repo.GetAll().OrderBy(z => z.Id))
 {
 Zanrovi.Add(new ZanrView(z));
 }
 RefreshDetails();
 }

 private void RefreshDetails()
 {
 Podzanrovi.Clear();
 if (Selected == null) return;
 var z = _repo.GetById(Selected.Id);
 if (z == null) return;
 foreach (var id in z.ZanrIDs)
 {
 var p = _repo.GetById(id);
 if (p != null) Podzanrovi.Add(new ZanrItem(p));
 }
 }

 private void RefreshAvailable()
 {
 DostupniPodzanrovi.Clear();
 if (Selected == null) return;
 var z = _repo.GetById(Selected.Id);
 if (z == null) return;
 var ids = z.ZanrIDs.ToHashSet();
 ids.Add(z.Id);
 foreach (var other in _repo.GetAll().Where(o => !ids.Contains(o.Id)).OrderBy(o => o.Naziv))
 {
 DostupniPodzanrovi.Add(new ZanrItem(other));
 }
 }

 private void Add()
 {
 var name = Prompt("Naziv novog zanra:");
 if (string.IsNullOrWhiteSpace(name)) return;
 var z = new Zanr(name) { Id = _repo.GetNextId() };
 _repo.Add(z);
 Load();
 }

 private void Edit()
 {
 if (Selected == null) return;
 var name = Prompt("Novi naziv zanra:", Selected.Naziv);
 if (string.IsNullOrWhiteSpace(name)) return;
 var z = _repo.GetById(Selected.Id);
 if (z == null) return;
 z.Naziv = name;
 _repo.Update(z);
 Load();
 }

 private void Delete()
 {
 if (Selected == null) return;
 if (MessageBox.Show($"Obrisati zanr '{Selected.Naziv}'?", "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
 _repo.Delete(Selected.Id);
 Load();
 }

 private void RemovePodzanr(ZanrItem? item)
 {
 if (Selected == null || item == null) return;
 var z = _repo.GetById(Selected.Id);
 if (z == null) return;
 if (z.ZanrIDs.Remove(item.Id))
 {
 _repo.Update(z);
 RefreshDetails();
 RefreshAvailable();
 }
 }

 private void AddPodzanrFromList(ZanrItem? item)
 {
 if (Selected == null || item == null) return;
 var z = _repo.GetById(Selected.Id);
 if (z == null) return;
 if (!z.ZanrIDs.Contains(item.Id))
 {
 z.ZanrIDs.Add(item.Id);
 _repo.Update(z);
 RefreshDetails();
 RefreshAvailable();
 }
 }

 private static string? Prompt(string text, string? value = null)
 {
 return Microsoft.VisualBasic.Interaction.InputBox(text, "Unos", value ?? string.Empty);
 }
 }

 public class ZanrView
 {
 public int Id { get; }
 public string Naziv { get; }
 public int BrojDela { get; }
 public int BrojIzvodjaca { get; }
 public ZanrView(Zanr z)
 {
 Id = z.Id; Naziv = z.Naziv; BrojDela = z.MuzickaDelaIDs.Count; BrojIzvodjaca = z.IzvodjaciIDs.Count;
 }
 }

 public class ZanrItem
 {
 public int Id { get; }
 public string Naziv { get; }
 public ZanrItem(Zanr z) { Id = z.Id; Naziv = z.Naziv; }
 }
}
