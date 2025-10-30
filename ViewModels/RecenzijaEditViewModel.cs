using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MusicCatalog.Models;
using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Models.Recenzije;
using MusicCatalog.Repositories;
using MusicCatalog.Services;
using MusicCatalog.Utils;

namespace MusicCatalog.ViewModels
{
    public class RecenzijaEditViewModel : ViewModelBase
    {
        private readonly AuthService _auth;
        private readonly IRecenzijaRepository _recRepo;
        private readonly IOcenaRepository _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository _reqRepo;
        private readonly IKorisnikRepository _korisnikRepo;
        private readonly MuzickoDelo _delo;
        private readonly Recenzija? _existingRec;

        public string DeloNaziv => _delo.Naziv;
        public bool IsCreate => _existingRec == null;

        private int? _vrednost;
        public int? Vrednost { get => _vrednost; set { _vrednost = value; OnPropertyChanged(nameof(Vrednost)); } }
        public string? Opis { get; set; }

        public int? NovaOcenaVrednost { get; set; }

        public ICommand SaveCreateCommand { get; }
        public ICommand PosaljiZahtevCommand { get; }
        public ICommand IzbrisiRecenzijuCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? Close { get; set; }

        public RecenzijaEditViewModel(AuthService auth, IKorisnikRepository korisnikRepo, IRecenzijaRepository recRepo, IOcenaRepository ocenaRepo, IZahtevZaIzmenuRepository reqRepo, MuzickoDelo delo, Recenzija? existingRec)
        {
            _auth = auth;
            _korisnikRepo = korisnikRepo;
            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _reqRepo = reqRepo;
            _delo = delo;
            _existingRec = existingRec;

            SaveCreateCommand = new RelayCommand(_ => Create(), _ => CanCreate());
            PosaljiZahtevCommand = new RelayCommand(_ => PosaljiZahtev(), _ => CanPosaljiZahtev());
            IzbrisiRecenzijuCommand = new RelayCommand(_ => IzbrisiRecenziju());

           
            CloseCommand = new RelayCommand(_ => Close?.Invoke());
            

            if (!IsCreate)
            {
                
                var ocena = _ocenaRepo.GetByRecenzijaId(_existingRec!.Id);
                NovaOcenaVrednost = ocena?.Vrednost ?? null;
                Opis = _existingRec!.Opis;
            }
        }

        private bool CanCreate() => IsCreate && Vrednost is >= 1 and <= 5;
        private void Create()
        {
            var korisnik = _auth.TrenutniKorisnik!;
            var rec = new Recenzija
            {
                MuzickoDeloId = _delo.Id,
                KorisnikEmail = korisnik.Email,
                KorisnikUloga = korisnik.Uloga,
                Opis = string.IsNullOrWhiteSpace(Opis) ? null : Opis,
                Datum = DateTime.Now
            };
            _recRepo.Add(rec);

            var ocena = new Ocena
            {
                RecenzijaId = rec.Id,
                Vrednost = Vrednost!.Value
            };
            _ocenaRepo.Add(ocena);

            Close?.Invoke();
        }

        private bool CanPosaljiZahtev()
        {
            if (IsCreate) return false;
            return NovaOcenaVrednost is >= 1 and <= 5;
        }

        private void PosaljiZahtev()
        {
            var korisnik = _auth.TrenutniKorisnik!;
            var postojece = _reqRepo.GetByRecenzijaId(_existingRec!.Id)
                .FirstOrDefault(r => r.Status != StatusZahteva.PRIHVACEN);
            if (postojece != null)
            {
                MessageBox.Show("Već postoji zahtev za ovu recenziju koji nije prihvaćen (NA_CEKANJU ili ODBIJEN).", "Obaveštenje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var zahtev = new ZahtevZaIzmenu
            {
                
                RecenzijaId = _existingRec!.Id,
                TipZahteva = TipZahteva.IZMENA,
                Status = StatusZahteva.NA_CEKANJU,
                NovaOcenaVrednost = NovaOcenaVrednost,
                RecenzijaSnapshot = new Recenzija
                {
                    Id = _existingRec.Id,
                    MuzickoDeloId = _existingRec.MuzickoDeloId,
                    KorisnikEmail = _existingRec.KorisnikEmail,
                    KorisnikUloga = _existingRec.KorisnikUloga,
                    Opis = _existingRec.Opis,
                    Datum = _existingRec.Datum
                },
                AutorEmail = korisnik.Email,
                DatumKreiranja = DateTime.Now
            };
            _reqRepo.Add(zahtev);
            Close?.Invoke();
        }

        private void IzbrisiRecenziju()
        {
            var korisnik = _auth.TrenutniKorisnik!;
            var postojece = _reqRepo.GetByRecenzijaId(_existingRec!.Id)
                .FirstOrDefault(r => r.Status != StatusZahteva.PRIHVACEN);
            if (postojece != null)
            {
                MessageBox.Show("Već postoji zahtev za ovu recenziju koji nije prihvaćen (NA_CEKANJU ili ODBIJEN).", "Obaveštenje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var zahtev = new ZahtevZaIzmenu
            {
                RecenzijaId = _existingRec!.Id,
                TipZahteva = TipZahteva.BRISANJE,
                Status = StatusZahteva.NA_CEKANJU,
                NovaOcenaVrednost = null,
                RecenzijaSnapshot = new Recenzija
                {
                    Id = _existingRec.Id,
                    MuzickoDeloId = _existingRec.MuzickoDeloId,
                    KorisnikEmail = _existingRec.KorisnikEmail,
                    KorisnikUloga = _existingRec.KorisnikUloga,
                    Opis = _existingRec.Opis,
                    Datum = _existingRec.Datum
                },
                AutorEmail = korisnik.Email,
                DatumKreiranja = DateTime.Now
            };
            _reqRepo.Add(zahtev);
            Close?.Invoke();
        }
    }
}