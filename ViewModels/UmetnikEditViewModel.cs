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
    // Pomoćna klasa je malo izmenjena da koristi 'MuzickoDelo' kao bazu
    public class DeloCheckItem : ViewModelBase
    {
        public MuzickoDelo Delo { get; }
        private bool _isSelected;
        public event Action<DeloCheckItem>? OnSelectionChanged;

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

        // --- DODATO ---
        // Koristićemo ovo u XAML-u da drugačije stilizujemo albume
        public bool IsAlbum => Delo is Album;
        // --- KRAJ DODAVANJA ---

        public DeloCheckItem(MuzickoDelo delo) { Delo = delo; }
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

        private List<Clanstvo> _clanstva = new();

        // Deljeno
        private string _opis;
        public string Opis { get => _opis; set { _opis = value; OnPropertyChanged(nameof(Opis)); UpdateCanSave(); } }
        public string _slikaPutanja = string.Empty;
        public string SlikaPutanja { get => _slikaPutanja; set { _slikaPutanja = value; OnPropertyChanged(nameof(SlikaPutanja)); UpdateCanSave(); } }

        // Bend
        private string _naziv = string.Empty;
        public string Naziv { get => _naziv; set { _naziv = value; OnPropertyChanged(nameof(Naziv)); UpdateCanSave(); } }
        private DateOnly _datumNastanka = new DateOnly();
        public DateOnly DatumNastanka { get => _datumNastanka; set { _datumNastanka = value; OnPropertyChanged(nameof(DatumNastanka)); UpdateCanSave(); } }
        public ObservableCollection<Izvodjac> sviIzvodjaci { get; set; } = new();
        public ObservableCollection<Izvodjac> Clanovi { get; set; } = new();

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


        // Izvodjac
        public string _ime = string.Empty;
        public string Ime { get => _ime; set { _ime = value; OnPropertyChanged(nameof(Ime)); UpdateCanSave(); } }

        public string _prezime = string.Empty;
        public string Prezime { get => _prezime; set { _prezime = value; OnPropertyChanged(nameof(Prezime)); UpdateCanSave(); } }

        // --- POČETAK IZMENE (Jedna lista) ---
        public ObservableCollection<DeloCheckItem> SvaMuzickaDela { get; } = new();
        // --- KRAJ IZMENE ---


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
            RemoveClanCommand = new RelayCommand(param => RemoveClan(param as Izvodjac), param => param is Izvodjac);

            if (id.HasValue)
            {
                var existing = _umetnikRepository.GetById(id.Value);
                Opis = existing.Opis;
                SlikaPutanja = existing.Slika;

                if (existing is Bend bend)
                {
                    Naziv = bend.Naziv;
                    DatumNastanka = bend.DatumNastanka;
                }
                else if (existing is Izvodjac izvodjac)
                {
                    Ime = izvodjac.Ime;
                    Prezime = izvodjac.Prezime;
                }
            }

            if (_isBend)
            {
                LoadIzvojaci();
            }
            else
            {
                // --- POČETAK IZMENE (Logika učitavanja) ---
                LoadDela(); // Učitaj albume i pesme

                if (id.HasValue && _umetnikRepository.GetById(id.Value) is Izvodjac izvodjac)
                {
                    // Selektuj dela koje izvođač ima
                    if (izvodjac.MuzickoDeloIDs != null)
                    {
                        var deloIdSet = izvodjac.MuzickoDeloIDs.ToHashSet();
                        foreach (var item in SvaMuzickaDela.Where(a => deloIdSet.Contains(a.Delo.Id)))
                        {
                            item.OnSelectionChanged -= OnDeloSelectionChanged; // Privremeno ukidamo event
                            item.IsSelected = true;
                            item.OnSelectionChanged += OnDeloSelectionChanged;
                        }
                    }
                }
                // --- KRAJ IZMENE ---
            }
        }

        private void LoadDela()
        {
            SvaMuzickaDela.Clear();

            // Učitavamo SVA dela i sortiramo ih tako da Albumi budu na vrhu
            var dela = _deloRepository.GetAll()
                .OrderBy(d => d is Pesma) // Albumi (false=0) idu pre Pesama (true=1)
                .ThenBy(d => d.Naziv);

            foreach (var delo in dela)
            {
                var item = new DeloCheckItem(delo);
                item.OnSelectionChanged += OnDeloSelectionChanged; // Pretplati se na događaj
                SvaMuzickaDela.Add(item);
            }
        }

        // --- POČETAK IZMENE (Logika kaskadnog odčekiranja) ---
        private void OnDeloSelectionChanged(DeloCheckItem deloItem)
        {
            // Kaskadno odčekiranje
            // Ako je item koji je promenjen Album I ako je odčekiran
            if (deloItem.Delo is Album album && deloItem.IsSelected == false)
            {
                if (album.PesmaIDs == null || !album.PesmaIDs.Any()) return;

                var pesmaIdsToUncheck = album.PesmaIDs.ToHashSet();

                // Prolazimo kroz celu listu i tražimo pesme koje pripadaju tom albumu
                foreach (var pesmaItem in SvaMuzickaDela.Where(p => p.Delo is Pesma && pesmaIdsToUncheck.Contains(p.Delo.Id)))
                {
                    pesmaItem.IsSelected = false; // Ovo će automatski osvežiti UI
                }
            }
        }
        // --- KRAJ IZMENE ---

        private void LoadIzvojaci()
        {
            sviIzvodjaci.Clear();
            sviIzvodjaci = new ObservableCollection<Izvodjac>(_umetnikRepository.GetAll().OfType<Izvodjac>().ToList());

            if (_id.HasValue)
            {
                var Clanstva = _clanstvoRepository.GetAll().Where(c => c.BendId == _id.Value).ToList();
                foreach (Clanstvo c in Clanstva)
                {
                    Clanovi.Add(sviIzvodjaci.FirstOrDefault(i => i.Id == c.UmetnikId)!);
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
            if (_id.HasValue)
            {
                var existing = _umetnikRepository.GetById(_id.Value);
                existing.Opis = Opis;
                existing.Slika = SlikaPutanja;
                if (_isBend && existing is Bend bend)
                {
                    bend.Naziv = Naziv;
                    bend.DatumNastanka = DatumNastanka;
                }
                else if (!_isBend && existing is Izvodjac izvodjac)
                {
                    izvodjac.Ime = Ime;
                    izvodjac.Prezime = Prezime;
                    // --- POČETAK IZMENE (Snimanje ID-jeva) ---
                    izvodjac.MuzickoDeloIDs = SvaMuzickaDela.Where(a => a.IsSelected).Select(a => a.Delo.Id).ToList();
                    // --- KRAJ IZMENE ---
                }
                _umetnikRepository.Update(existing);
                _umetnikRepository.SaveChanges();
                OnSaved?.Invoke();
            }
            else
            {
                if (_isBend)
                {
                    _umetnikRepository.Add(new Bend(Naziv, DatumNastanka, Opis, SlikaPutanja, true));
                    _umetnikRepository.SaveChanges();
                    OnSaved?.Invoke();
                }
                else
                {
                    // --- POČETAK IZMENE (Snimanje ID-jeva kod kreiranja) ---
                    var noviIzvodjac = new Izvodjac(Ime, Prezime, Opis, SlikaPutanja)
                    {
                        MuzickoDeloIDs = SvaMuzickaDela.Where(a => a.IsSelected).Select(a => a.Delo.Id).ToList()
                    };
                    _umetnikRepository.Add(noviIzvodjac);
                    // --- KRAJ IZMENE ---
                    _umetnikRepository.SaveChanges();
                    OnSaved?.Invoke();
                }
            }
        }

        private void UpdateCanAddClan() => CommandManager.InvalidateRequerySuggested();
        private bool CanAddClan()
        {
            return _isBend && _id.HasValue && SelectedIzvodjac != null;
        }

        private void AddClan()
        {
            ErrorMessage = string.Empty;

            if (!_isBend)
            {
                ErrorMessage = "Samo bend može imati članove.";
                return;
            }
            if (!_id.HasValue)
            {
                ErrorMessage = "Bend još nije spremljen. Sačuvajte bend prije dodavanja članova.";
                return;
            }
            if (SelectedIzvodjac == null)
            {
                ErrorMessage = "Izaberite izvođača.";
                return;
            }

            var already = _clanstvoRepository.GetAll()
                .FirstOrDefault(c => c.BendId == _id.Value && c.UmetnikId == SelectedIzvodjac.Id);
            if (already != null)
            {
                ErrorMessage = "Izvođač je već član benda.";
                return;
            }

            var novi = new Clanstvo
            {
                UmetnikId = SelectedIzvodjac.Id,
                BendId = _id.Value,
                datumUclanjenja = DateOnly.FromDateTime(NewDatumUclanjenja.Date)
            };

            _clanstvoRepository.Add(novi);
            _clanstvoRepository.SaveChanges();

            Clanovi.Add(SelectedIzvodjac);
            _clanstva.Add(novi);

            SelectedIzvodjac = null;
            NewDatumUclanjenja = DateTime.Today;
        }

        private void RemoveClan(Izvodjac izvodjac)
        {
            if (izvodjac == null) return;
            if (!_id.HasValue) return;

            var cl = _clanstvoRepository.GetAll().FirstOrDefault(c => c.BendId == _id.Value && c.UmetnikId == izvodjac.Id);
            if (cl != null)
            {
                _clanstvoRepository.Delete(cl.Id);
                _clanstvoRepository.SaveChanges();

                Clanovi.Remove(izvodjac);
                _clanstva.Remove(cl);
            }
            else
            {
                Clanovi.Remove(izvodjac);
            }
        }
    }
}