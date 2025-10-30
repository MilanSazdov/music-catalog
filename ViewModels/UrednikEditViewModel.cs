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
using System.Text.RegularExpressions;
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
        private string _confirmPassword = string.Empty;
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
            set { if (_ime == value) return; _ime = value; OnPropertyChanged(nameof(Ime)); OnPropertyChanged(nameof(CanConfirm)); }
        }

        public string Prezime
        {
            get => _prezime;
            set { if (_prezime == value) return; _prezime = value; OnPropertyChanged(nameof(Prezime)); OnPropertyChanged(nameof(CanConfirm)); }
        }

        public string Email
        {
            get => _email;
            set { if (_email == value) return; _email = value; OnPropertyChanged(nameof(Email)); OnPropertyChanged(nameof(CanConfirm)); }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password == value) return;
                _password = value;
                OnPropertyChanged(nameof(Password));
                OnPropertyChanged(nameof(ConfirmPassword));
                OnPropertyChanged(nameof(CanConfirm));
            }
        }

   
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword == value) return;
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
                OnPropertyChanged(nameof(CanConfirm));
            }
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

        public ICommand AddZanrCommand { get; }
        public ICommand RemoveZanrCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public UrednikEditViewModel(IZanrRepository zanrRepo, IKorisnikRepository? korisnikRepo = null, MuzickiUrednik? urednik = null)
        {
            _zanrRepo = zanrRepo ?? throw new ArgumentNullException(nameof(zanrRepo));
            _korisnikRepo = korisnikRepo;
            _originalUrednik = urednik;

            AddZanrCommand = new RelayCommand(_ => AddZanr());
            RemoveZanrCommand = new RelayCommand(p => RemoveZanr(p as Zanr));
            ConfirmCommand = new RelayCommand(_ => Confirm(), _ => CanConfirm());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadAllZanrovi();

            if (urednik != null)
            {
                IsEditing = true;
                Ime = urednik.Ime ?? string.Empty;
                Prezime = urednik.Prezime ?? string.Empty;
                Email = urednik.Email ?? string.Empty;
               
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

        private void AddZanrCommandCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void RemoveZanr(Zanr? z)
        {
            if (z == null) return;
            Specijalizacija.Remove(z);
            OnPropertyChanged(nameof(Specijalizacija));
        }

        private bool CanConfirm()
        {
            return IsValid;
        }

        private void Confirm()
        {
            ErrorMessage = string.Empty;

            if (!CanConfirm())
            {
                
                return;
            }

            try
            {
                if (IsEditing && _originalUrednik != null)
                {
                 
                    if (_originalUrednik.Email != Email && _korisnikRepo != null && _korisnikRepo.GetByEmail(Email) != null)
                    {
                        ErrorMessage = "Korisnik sa ovim emailom već postoji.";
                        return;
                    }
                

                    _originalUrednik.Ime = Ime;
                    _originalUrednik.Prezime = Prezime;
                    _originalUrednik.Email = Email;
                    if (!string.IsNullOrEmpty(Password))
                        _originalUrednik.Lozinka = Password;

                    _originalUrednik.Specijalizacija = Specijalizacija.ToList();

                    if (_korisnikRepo != null)
                    {
                        _korisnikRepo.Update(_originalUrednik);
                        _korisnikRepo.SaveChanges();
                    }
                }
                else
                {
                    if (_korisnikRepo != null && _korisnikRepo.GetByEmail(Email) != null)
                    {
                        ErrorMessage = "Korisnik sa ovim emailom već postoji.";
                        return;
                    }


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
                        if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Email nije u ispravnom formatu.";
                        if (_korisnikRepo != null)
                        {
                            var postojeci = _korisnikRepo.GetByEmail(Email);
                            if (postojeci != null)
                            {
                                
                                if (!IsEditing && postojeci != null)
                                    return "Korisnik sa ovim emailom već postoji.";
                                
                                if (IsEditing && postojeci != null && _originalUrednik != null && postojeci.Email != _originalUrednik.Email)
                                    return "Korisnik sa ovim emailom već postoji.";
                            }
                        }
                        break;
                    case nameof(Password):
                        if (!IsEditing)
                        {
                            if (string.IsNullOrWhiteSpace(Password)) return "Lozinka je obavezna za novog urednika.";
                            if (Password.Length < 6) return "Lozinka mora imati najmanje 6 karaktera.";
                        }
                        else
                        {
                            
                            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
                                return "Lozinka mora imati bar 6 karaktera.";
                            
                            if (!string.IsNullOrWhiteSpace(Password) && string.IsNullOrWhiteSpace(ConfirmPassword))
                                OnPropertyChanged(nameof(ConfirmPassword));
                        }
                        break;
                    case nameof(ConfirmPassword):
                        
                        if (!string.IsNullOrWhiteSpace(Password))
                        {
                            if (Password != ConfirmPassword)
                                return "Lozinke se ne poklapaju.";
                        }
                        
                        else if (string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword))
                        {
                            return "Unesite prvo lozinku.";
                        }
                        break;
                }
                return string.Empty;
            }
        }

        private bool IsValid
        {
            get
            {
                string[] properties = { nameof(Ime), nameof(Prezime), nameof(Email), nameof(Password), nameof(ConfirmPassword) };
                foreach (var prop in properties)
                {
                    if (!string.IsNullOrEmpty(this[prop]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}