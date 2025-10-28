using MusicCatalog.Models;
using MusicCatalog.Repositories;

namespace MusicCatalog.Services
{
    public class AuthService
    {
        private readonly IKorisnikRepository _korisnikRepository;

        public Korisnik? TrenutniKorisnik { get; private set; }

        public AuthService(IKorisnikRepository korisnikRepository)
        {
            _korisnikRepository = korisnikRepository;
        }

        public bool Login(string email, string password)
        {
            var korisnik = _korisnikRepository.GetByEmail(email);
            if (korisnik == null) return false;
            if (korisnik.Lozinka != password) return false;
            if (korisnik is RegistrovaniKorisnik registrovaniKorisnik && registrovaniKorisnik.Blokiran)
            {
                return false;
            }
            TrenutniKorisnik = korisnik;
            return true;
        }

        public RegistrovaniKorisnik? Register(string ime, string prezime, string email, string password)
        {
            var postojeciKorisnik = _korisnikRepository.GetByEmail(email);
            if (postojeciKorisnik != null)
            {
                if (postojeciKorisnik is RegistrovaniKorisnik registrovani && registrovani.Blokiran)
                {
                    registrovani.Ime = ime;
                    registrovani.Prezime = prezime;
                    registrovani.Lozinka = password;
                    registrovani.Blokiran = false;
                    registrovani.Pretplacen = false;
                    _korisnikRepository.Update(registrovani);
                    _korisnikRepository.SaveChanges();
                    return registrovani;
                }
                else
                {
                    return null;
                }
            }
            var noviKorisnik = new RegistrovaniKorisnik(email, ime, prezime, password);
            _korisnikRepository.Add(noviKorisnik);
            _korisnikRepository.SaveChanges();
            return noviKorisnik;
        }

        public void Logout()
        {
            TrenutniKorisnik = null;
        }

        public bool BlokirajNalog(Korisnik korisnik)
        {
            if (korisnik is RegistrovaniKorisnik registrovani)
            {
                registrovani.Blokiran = true;
                _korisnikRepository.Update(registrovani);
                _korisnikRepository.SaveChanges();
                Logout();
                return true;
            }
            return false;
        }

        // <-- DODAJ OVU METODU NA KRAJ KLASE -->
        /// <summary>
        /// Ažurira podatke za korisnika na osnovu email-a.
        /// Lozinka se menja samo ako 'novaLozinka' nije prazna.
        /// </summary>
        public bool UpdatePodatke(string email, string novoIme, string novoPrezime, string? novaLozinka)
        {
            var korisnik = _korisnikRepository.GetByEmail(email);
            if (korisnik == null)
            {
                return false;
            }

            korisnik.Ime = novoIme;
            korisnik.Prezime = novoPrezime;

            // Ažuriraj lozinku samo ako je nova lozinka uneta
            if (!string.IsNullOrWhiteSpace(novaLozinka))
            {
                korisnik.Lozinka = novaLozinka;
            }

            _korisnikRepository.Update(korisnik);
            _korisnikRepository.SaveChanges();

            // Osveži i TrenutnogKorisnika ako je to on
            if (TrenutniKorisnik != null && TrenutniKorisnik.Email == email)
            {
                TrenutniKorisnik = korisnik;
            }

            return true;
        }
    }
}