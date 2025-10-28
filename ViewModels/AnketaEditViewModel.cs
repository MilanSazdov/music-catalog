using MusicCatalog.Models;
using MusicCatalog.Models.Enums;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class AnketaEditViewModel : ViewModelBase
    {
        private readonly AnketaRepository _repository;
        private string _naziv = string.Empty;
        private Period _selectedPeriod = Period.Mesec;
        private DateTime? _datumPocetka;
        private DateTime? _datumKraja;
        private string _errorMessage = string.Empty;
        private int _id;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Action? OnSaved { get; set; }
        public Action? OnCancelled { get; set; }

        public AnketaEditViewModel(AnketaRepository repository, int? anketaId = null)
        {
            _repository = repository;

            Periods = Enum.GetValues(typeof(Period)).Cast<Period>().ToList();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            if (anketaId.HasValue)
            {
                var a = _repository.GetById(anketaId.Value);
                if (a != null)
                {
                    // režim editovanja
                    Id = a.Id;
                    Naziv = a.Naziv;
                    SelectedPeriod = a.Period;
                    DatumPocetka = a.DatumPocetka;
                    DatumKraja = a.DatumKraja;
                    Title = "Izmena ankete";
                    SaveButtonText = "Sačuvaj";
                    IsEditing = true;
                }
                else
                {
                    // prosleđeni id ne postoji -> ponaša se kao kreiranje nove
                    InitializeForCreate();
                }
            }
            else
            {
                InitializeForCreate();
            }
        }

        private void InitializeForCreate()
        {
            Title = "Nova anketa";
            SaveButtonText = "Kreiraj";
            IsEditing = false;

            // podrazumevane vrednosti
            DatumPocetka = DateTime.Today;
            DatumKraja = DateTime.Today.AddDays(7);
        }

        public int Id
        {
            get => _id;
            private set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Title { get; private set; } = "Anketa";

        public string SaveButtonText { get; private set; } = "Sačuvaj";

        public bool IsEditing { get; private set; }

        public List<Period> Periods { get; }

        public string Naziv
        {
            get => _naziv;
            set
            {
                if (_naziv == value) return;
                _naziv = value;
                OnPropertyChanged(nameof(Naziv));
                OnValidationChanged();
            }
        }

        public Period SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (_selectedPeriod == value) return;
                _selectedPeriod = value;
                OnPropertyChanged(nameof(SelectedPeriod));
            }
        }

        public DateTime? DatumPocetka
        {
            get => _datumPocetka;
            set
            {
                if (_datumPocetka == value) return;
                _datumPocetka = value;
                OnPropertyChanged(nameof(DatumPocetka));
                OnValidationChanged();
            }
        }

        public DateTime? DatumKraja
        {
            get => _datumKraja;
            set
            {
                if (_datumKraja == value) return;
                _datumKraja = value;
                OnPropertyChanged(nameof(DatumKraja));
                OnValidationChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }


        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }



        private void Save()
        {
            ErrorMessage = string.Empty;

            if (!CanSave())
            {
                ErrorMessage = "Proverite polja: naziv je obavezan i datum početka mora biti pre ili jednak datumu kraja.";
                RaiseCommandsCanExecuteChanged();
                return;
            }

            if (IsEditing)
            {
                // update postojeće ankete
                var existing = _repository.GetById(Id);
                if (existing != null)
                {
                    existing.Naziv = Naziv;
                    existing.Period = SelectedPeriod;
                    existing.DatumPocetka = DatumPocetka!.Value;
                    existing.DatumKraja = DatumKraja!.Value;
                    _repository.Update(existing);
                }
                else
                {
                    var newA = new Anketa
                    {
                        Id = Id,
                        Naziv = Naziv,
                        Period = SelectedPeriod,
                        DatumPocetka = DatumPocetka!.Value,
                        DatumKraja = DatumKraja!.Value
                    };
                    _repository.Add(newA);
                }
            }
            else
            {
                var all = _repository.GetAll();
                int nextId = all.Any() ? all.Max(x => x.Id) + 1 : 1;

                var newAnketa = new Anketa
                {
                    Id = nextId,
                    Naziv = Naziv,
                    Period = SelectedPeriod,
                    DatumPocetka = DatumPocetka!.Value,
                    DatumKraja = DatumKraja!.Value
                };

                _repository.Add(newAnketa);
                Id = nextId;
                IsEditing = true;
            }

            _repository.SaveChanges();

            OnSaved?.Invoke();
            RaiseCommandsCanExecuteChanged();
        }

        private void Cancel()
        {
            OnCancelled?.Invoke();
        }


        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Naziv):
                        if (string.IsNullOrWhiteSpace(Naziv))
                            return "Naziv ankete je obavezan.";
                        break;
                    case nameof(DatumPocetka):
                    case nameof(DatumKraja):
                        if (!DatumPocetka.HasValue)
                            return "Datum početka je obavezan.";
                        if (!DatumKraja.HasValue)
                            return "Datum kraja je obavezan.";
                        if (DatumPocetka > DatumKraja)
                            return "Datum početka ne može biti posle datuma kraja.";
                        break;
                }
                return string.Empty;
            }
        }

        private bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(Naziv)) return false;
            if (!DatumPocetka.HasValue || !DatumKraja.HasValue) return false;
            if (DatumPocetka > DatumKraja) return false;
            return true;
        }

        private void OnValidationChanged()
        {
            ErrorMessage = string.Empty;
            RaiseCommandsCanExecuteChanged();
        }


        private void RaiseCommandsCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
