using MusicCatalog.Models;
using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{

    public class UrednikEditViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly IZanrRepository _zanrRepo;
        private readonly IKorisnikRepository? _korisnikRepo;
        private readonly MuzickiUrednik? _originalUrednik;

        private string _ime = string.Empty;
        private string _prezime = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private Zanr? _newZanr;
        private string _errorMessage = string.Empty;
        private bool _isEditing;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Action? OnSaved { get; set; }
        public Action? OnCancelled { get; set; }

        public ObservableCollection<Zanr> SviZanrovi { get; } = new ObservableCollection<Zanr>();
        public ObservableCollection<Zanr> Specijalizacija { get; } = new ObservableCollection<Zanr>();

        public string Ime
        {
            get => _ime;
            set { if (_ime == value) return; _ime = value; OnPropertyChanged(nameof(Ime)); }
        }

        public string Prezime
        {
            get => _prezime;
            set { if (_prezime == value) return; _prezime = value; OnPropertyChanged(nameof(Prezime)); }
        }

        public string Email
        {
            get => _email;
            set { if (_email == value) return; _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string Password
        {
            get => _password;
            set { if (_password == value) return; _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public Zanr? NewZanr
        {
            get => _newZanr;
            set { if (_newZanr == value) return; _newZanr = value; OnPropertyChanged(nameof(NewZanr)); AddZanrCommandCanExecuteChanged(); }
        }

        public bool CanEditLanguages => true;

        public bool IsEditing
        {
            get => _isEditing;
            private set { if (_isEditing == value) return; _isEditing = value; OnPropertyChanged(nameof(IsEditing)); OnPropertyChanged(nameof(Title)); }
        }

        public string Title => IsEditing ? "Izmeni urednika" : "Kreiraj urednika";

        public string ErrorMessage
        {
            get => _errorMessage;
            private set { if (_errorMessage == value) return; _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        // Komande
        public ICommand AddZanrCommand { get; }
        public ICommand RemoveZanrCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public UrednikEditViewModel(IZanrRepository zanrRepo, IKorisnikRepository? korisnikRepo = null, MuzickiUrednik? urednik = null)
        {
            _zanrRepo = zanrRepo ?? throw new ArgumentNullException(nameof(zanrRepo));
            _korisnikRepo = korisnikRepo;
            _originalUrednik = urednik;

            // Komande
            AddZanrCommand = new RelayCommand(_ => AddZanr(), _ => CanAddZanr());
            RemoveZanrCommand = new RelayCommand(p => RemoveZanr(p as Zanr), p => p is Zanr);
            RegisterCommand = new RelayCommand(_ => Register(), _ => CanRegister());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadAllZanrovi();

            if (urednik != null)
            {
                IsEditing = true;
                Ime = urednik.Ime ?? string.Empty;
                Prezime = urednik.Prezime ?? string.Empty;
                Email = urednik.Email ?? string.Empty;
                Password = urednik.Lozinka;

                Specijalizacija.Clear();
                if (urednik.Specijalizacija != null)
                {
                    foreach (var z in urednik.Specijalizacija)
                        Specijalizacija.Add(z);
                }
            }
            else
            {
                IsEditing = false;
            }

            AddZanrCommandCanExecuteChanged();
        }

        private void LoadAllZanrovi()
        {
            SviZanrovi.Clear();
            var all = _zanrRepo.GetAll();
            foreach (var z in all.OrderBy(x => x.Naziv))
                SviZanrovi.Add(z);
        }

        private void AddZanr()
        {
            if (NewZanr == null) return;

            if (Specijalizacija.Any(s => string.Equals(s.Naziv, NewZanr.Naziv, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Žanr je već dodat u specijalizaciju.";
                return;
            }
            Specijalizacija.Add(NewZanr);
            ErrorMessage = string.Empty;
            NewZanr = null;
        }

        private bool CanAddZanr()
        {
            return NewZanr != null && CanEditLanguages;
        }

        private void AddZanrCommandCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void RemoveZanr(Zanr? z)
        {
            if (z == null) return;
            Specijalizacija.Remove(z);
        }

        private bool CanRegister()
        {
            // Osnovna validacija
            if (string.IsNullOrWhiteSpace(Ime)) return false;
            if (string.IsNullOrWhiteSpace(Prezime)) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;
            // Email provera vrlo osnovna
            if (!Email.Contains("@")) return false;
            // Ako kreiramo novog, lozinka je obavezna
            if (!IsEditing && string.IsNullOrWhiteSpace(Password)) return false;
            // Ako je lozinka dana, neka bude barem 6 chars
            if (!string.IsNullOrEmpty(Password) && Password.Length < 6) return false;
            return true;
        }

        private void Register()
        {
            ErrorMessage = string.Empty;

            if (!CanRegister())
            {
                return;
            }

            try
            {
                if (IsEditing && _originalUrednik != null)
                {
                    // Ažuriraj postojeći objekt
                    _originalUrednik.Ime = Ime;
                    _originalUrednik.Prezime = Prezime;
                    _originalUrednik.Email = Email;
                    if (!string.IsNullOrEmpty(Password))
                        _originalUrednik.Lozinka = Password; // u stvarnoj aplikaciji heširaj lozinku

                    // prekopiraj specijalizaciju
                    _originalUrednik.Specijalizacija = Specijalizacija.ToList();

                    // Ako imamo repo za urednike, snimi
                    if (_korisnikRepo != null)
                    {
                        _korisnikRepo.Update(_originalUrednik);
                        _korisnikRepo.SaveChanges();
                    }
                }
                else
                {
                    // Kreiraj novi urednik objekat i dodaj
                    var novi = new MuzickiUrednik(Email, Ime, Prezime, Password)
                    {
                        Specijalizacija = Specijalizacija.ToList()
                    };

                    _korisnikRepo.Add(novi);
                    _korisnikRepo.SaveChanges();
                    
                }

                OnSaved?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Greška pri čuvanju: {ex.Message}";
            }
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
                    case nameof(Ime):
                        if (string.IsNullOrWhiteSpace(Ime)) return "Ime je obavezno.";
                        break;
                    case nameof(Prezime):
                        if (string.IsNullOrWhiteSpace(Prezime)) return "Prezime je obavezno.";
                        break;
                    case nameof(Email):
                        if (string.IsNullOrWhiteSpace(Email)) return "Email je obavezan.";
                        if (!Email.Contains("@")) return "Email nije validan.";
                        break;
                    case nameof(Password):
                        if (!IsEditing && string.IsNullOrWhiteSpace(Password)) return "Lozinka je obavezna za novog urednika.";
                        if (!string.IsNullOrEmpty(Password) && Password.Length < 6) return "Lozinka mora imati najmanje 6 karaktera.";
                        break;
                }
                return string.Empty;
            }
        }


        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
