using MusicCatalog.Models.Korisnici;
using MusicCatalog.Repositories; // <-- DODAJ OVAJ RED

namespace MusicCatalog.Services
{
    public class AuthService
    {
        // Ova linija sada radi
        private readonly IKorisnikRepository _korisnikRepository;

        // (Ostatak tvoje klase ostaje isti)
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
            if (_korisnikRepository.GetByEmail(email) != null)
            {
                return null;
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
    }
}