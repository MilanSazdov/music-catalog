using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Models.Umetnici;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class DeloCheckItem<T> : ViewModelBase where T : MuzickoDelo
    {
        public T Delo { get; }
        private bool _isSelected;
        public event Action<DeloCheckItem<T>>? OnSelectionChanged;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetField(ref _isSelected, value))
                {
                    OnSelectionChanged?.Invoke(this);
                }
            }
        }
        public string Naziv => Delo.Naziv;

        public DeloCheckItem(T delo) { Delo = delo; }
    }

    public class ClanBendInfo : ViewModelBase
    {
        public Izvodjac Izvodjac { get; set; }
        private DateTime _datumUclanjenja;
        public DateTime DatumUclanjenja
        {
            get => _datumUclanjenja;
            set => SetField(ref _datumUclanjenja, value);
        }

        public string Ime => Izvodjac.Ime;
        public string Prezime => Izvodjac.Prezime;
    }
    


    public class UmetnikEditViewModel : ViewModelBase
    {
        private readonly IMuzickiUmetnikRepository _umetnikRepository;
        private readonly IClanstvoRepository _clanstvoRepository;
        private readonly IMuzickoDeloRepository _deloRepository;
        private readonly bool _isBend;
        private readonly int? _id;

        public bool IsBend => _isBend;
        public string TipText => _isBend ? "Bend" : "Izvodjac";

        public bool IsEditing => _id.HasValue;

        private List<Clanstvo> _clanstva = new();

        private string _opis;
        public string Opis { get => _opis; set { _opis = value; OnPropertyChanged(nameof(Opis)); UpdateCanSave(); } }
        public string _slikaPutanja = string.Empty;
        public string SlikaPutanja { get => _slikaPutanja; set { _slikaPutanja = value; OnPropertyChanged(nameof(SlikaPutanja)); UpdateCanSave(); } }

        
        private string _naziv = string.Empty;
        public string Naziv { get => _naziv; set { _naziv = value; OnPropertyChanged(nameof(Naziv)); UpdateCanSave(); } }
        private DateOnly _datumNastanka = new DateOnly();
        public DateOnly DatumNastanka { get => _datumNastanka; set { _datumNastanka = value; OnPropertyChanged(nameof(DatumNastanka)); UpdateCanSave(); } }
        public ObservableCollection<Izvodjac> sviIzvodjaci { get; set; } = new();

        public ObservableCollection<ClanBendInfo> ClanoviBenda { get; set; } = new();
        

        public DateTime DatumNastankaDate
        {
            get => _datumNastanka == default ? DateTime.Today : _datumNastanka.ToDateTime(new TimeOnly(0));
            set
            {
                DatumNastanka = DateOnly.FromDateTime(value);
            }
        }

        private Izvodjac _selectedIzvodjac;
        public Izvodjac SelectedIzvodjac { get => _selectedIzvodjac; set { _selectedIzvodjac = value; OnPropertyChanged(nameof(SelectedIzvodjac)); UpdateCanAddClan(); } }

        private DateTime _newDatumUclanjenja = DateTime.Today;
        public DateTime NewDatumUclanjenja { get => _newDatumUclanjenja; set { _newDatumUclanjenja = value; OnPropertyChanged(nameof(NewDatumUclanjenja)); UpdateCanAddClan(); } }


        
        public string _ime = string.Empty;
        public string Ime { get => _ime; set { _ime = value; OnPropertyChanged(nameof(Ime)); UpdateCanSave(); } }

        public string _prezime = string.Empty;
        public string Prezime { get => _prezime; set { _prezime = value; OnPropertyChanged(nameof(Prezime)); UpdateCanSave(); } }

        public ObservableCollection<DeloCheckItem<Album>> SviAlbumi { get; } = new();
        public ObservableCollection<DeloCheckItem<Pesma>> SvePesme { get; } = new();

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddClanCommand { get; }
        public ICommand RemoveClanCommand { get; }

        public Action? OnSaved { get; set; }
        public Action? OnCancelled { get; set; }

        public UmetnikEditViewModel(IMuzickiUmetnikRepository umetnikRepo, IClanstvoRepository clanstvoRepo, IMuzickoDeloRepository deloRepo, bool isBend, int? id = null)
        {
            _umetnikRepository = umetnikRepo;
            _clanstvoRepository = clanstvoRepo;
            _deloRepository = deloRepo;
            _isBend = isBend;
            _id = id;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());
            AddClanCommand = new RelayCommand(_ => AddClan(), _ => CanAddClan());
            
            RemoveClanCommand = new RelayCommand(param => RemoveClan(param as ClanBendInfo), param => param is ClanBendInfo);
            

            MuzickiUmetnik? existingUmetnik = null;

            if (id.HasValue)
            {
                existingUmetnik = _umetnikRepository.GetById(id.Value);
                Opis = existingUmetnik.Opis;
                SlikaPutanja = existingUmetnik.Slika;

                if (existingUmetnik is Bend bend)
                {
                    Naziv = bend.Naziv;
                    DatumNastanka = bend.DatumNastanka;
                }
                else if (existingUmetnik is Izvodjac izvodjac)
                {
                    Ime = izvodjac.Ime;
                    Prezime = izvodjac.Prezime;
                }
            }

            LoadDela();

            if (existingUmetnik != null && existingUmetnik.MuzickoDeloIDs != null)
            {
                var deloIdSet = existingUmetnik.MuzickoDeloIDs.ToHashSet();

                foreach (var item in SviAlbumi.Where(a => deloIdSet.Contains(a.Delo.Id)))
                {
                    item.OnSelectionChanged -= OnAlbumSelectionChanged;
                    item.IsSelected = true;
                    item.OnSelectionChanged += OnAlbumSelectionChanged;
                }
                foreach (var item in SvePesme.Where(p => deloIdSet.Contains(p.Delo.Id)))
                {
                    item.IsSelected = true;
                }
            }

            if (_isBend)
            {
                LoadIzvojaci();
            }
        }

        private void LoadDela()
        {
            var dela = _deloRepository.GetAll();
            SviAlbumi.Clear();
            SvePesme.Clear();

            foreach (var album in dela.OfType<Album>().OrderBy(a => a.Naziv))
            {
                var item = new DeloCheckItem<Album>(album);
                item.OnSelectionChanged += OnAlbumSelectionChanged;
                SviAlbumi.Add(item);
            }

            foreach (var pesma in dela.OfType<Pesma>().OrderBy(p => p.Naziv))
            {
                var item = new DeloCheckItem<Pesma>(pesma);
                SvePesme.Add(item);
            }
        }

        private void OnAlbumSelectionChanged(DeloCheckItem<Album> albumItem)
        {
            var album = albumItem.Delo;
            if (album.PesmaIDs == null || !album.PesmaIDs.Any()) return;

            var pesmaIdsToUpdate = album.PesmaIDs.ToHashSet();

            foreach (var pesmaItem in SvePesme.Where(p => pesmaIdsToUpdate.Contains(p.Delo.Id)))
            {
                pesmaItem.IsSelected = albumItem.IsSelected;
            }
        }

        private void LoadIzvojaci()
        {
            
            sviIzvodjaci.Clear();
            foreach (var izvodjac in _umetnikRepository.GetAll().OfType<Izvodjac>().ToList())
            {
                sviIzvodjaci.Add(izvodjac);
            }

            
            ClanoviBenda.Clear();
            if (IsEditing)
            {
                var clanstvaZaOvajBend = _clanstvoRepository.GetAll().Where(c => c.BendId == _id.Value).ToList();
                foreach (Clanstvo c in clanstvaZaOvajBend)
                {
                    var izvodjac = sviIzvodjaci.FirstOrDefault(i => i.Id == c.UmetnikId);
                    if (izvodjac != null)
                    {
                        ClanoviBenda.Add(new ClanBendInfo
                        {
                            Izvodjac = izvodjac,
                            DatumUclanjenja = c.datumUclanjenja.ToDateTime(TimeOnly.MinValue)
                        });
                    }
                }
            }
            
        }

        private void UpdateCanSave() => CommandManager.InvalidateRequerySuggested();
        private bool CanSave()
        {
            if (_isBend)
            {
                return !string.IsNullOrWhiteSpace(Naziv);
            }
            else
            {
                return !string.IsNullOrWhiteSpace(Ime) && !string.IsNullOrWhiteSpace(Prezime);
            }
        }

        private void Save()
        {
            var albumIDs = SviAlbumi.Where(a => a.IsSelected).Select(a => a.Delo.Id);
            var pesmaIDs = SvePesme.Where(p => p.IsSelected).Select(p => p.Delo.Id);
            var spojeniIdjevi = albumIDs.Concat(pesmaIDs).Distinct().ToList();

            
            MuzickiUmetnik umetnikToSave;

            if (IsEditing)
            {
                umetnikToSave = _umetnikRepository.GetById(_id.Value);
            }
            else
            {
                
                if (_isBend)
                {
                    umetnikToSave = new Bend(Naziv, DatumNastanka, Opis, SlikaPutanja, true);
                }
                else
                {
                    umetnikToSave = new Izvodjac(Ime, Prezime, Opis, SlikaPutanja);
                }
            }

            umetnikToSave.Opis = Opis;
            umetnikToSave.Slika = SlikaPutanja;
            umetnikToSave.MuzickoDeloIDs = spojeniIdjevi;

            
            if (_isBend && umetnikToSave is Bend bend)
            {
                bend.Naziv = Naziv;
                bend.DatumNastanka = DatumNastanka;
            }
            else if (!_isBend && umetnikToSave is Izvodjac izvodjac)
            {
                izvodjac.Ime = Ime;
                izvodjac.Prezime = Prezime;
            }

           
            if (IsEditing)
            {
                _umetnikRepository.Update(umetnikToSave);
            }
            else
            {
                _umetnikRepository.Add(umetnikToSave);
            }
            _umetnikRepository.SaveChanges();

            
            if (_isBend)
            {
                int bendId = umetnikToSave.Id;

                
                var oldClanstva = _clanstvoRepository.GetAll().Where(c => c.BendId == bendId).ToList();
                foreach (var c in oldClanstva)
                {
                    _clanstvoRepository.Delete(c.Id);
                }

                
                foreach (var c in ClanoviBenda)
                {
                    _clanstvoRepository.Add(new Clanstvo
                    {
                        UmetnikId = c.Izvodjac.Id,
                        BendId = bendId,
                        datumUclanjenja = DateOnly.FromDateTime(c.DatumUclanjenja)
                    });
                }

                _clanstvoRepository.SaveChanges();
            }
            

            OnSaved?.Invoke();
        }

        private void UpdateCanAddClan() => CommandManager.InvalidateRequerySuggested();
        private bool CanAddClan()
        {

            return SelectedIzvodjac != null;
            
        }

        private void AddClan()
        {
            ErrorMessage = string.Empty;

            if (!_isBend)
            {
                ErrorMessage = "Samo bend može imati članove.";
                return;
            }
            
            if (SelectedIzvodjac == null)
            {
                ErrorMessage = "Izaberite izvođača.";
                return;
            }

            
            var already = ClanoviBenda.FirstOrDefault(c => c.Izvodjac.Id == SelectedIzvodjac.Id);
            if (already != null)
            {
                ErrorMessage = "Izvođač je već član benda.";
                return;
            }

            
            ClanoviBenda.Add(new ClanBendInfo
            {
                Izvodjac = SelectedIzvodjac,
                DatumUclanjenja = NewDatumUclanjenja.Date
            });
            

            SelectedIzvodjac = null;
            NewDatumUclanjenja = DateTime.Today;
        }

        private void RemoveClan(ClanBendInfo? clanInfo)
        {
            if (clanInfo == null) return;
            ClanoviBenda.Remove(clanInfo);
        }
    }
}