using MusicCatalog.Models.Enums;

namespace MusicCatalog.Models.Korisnici
{
    // ISPRAVKA: Promenjeno iz 'internal class' u 'public class'
    public class RegistrovaniKorisnik : Korisnik
    {
        public bool Pretplacen { get; set; }
        public bool Blokiran { get; set; }

        public RegistrovaniKorisnik(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.RegistrovaniKorisnik)
        {
            Blokiran = false;
            Pretplacen = false;
        }

        // --- Metode iz UML dijagrama ---
        public void stream() { }
        public void download() { }
        public void dodavanjeFavorita() { }
        public void brisanjeNaloga() { }
        public void davanjeOcene() { }
        public void pisanjeRecenzije() { }
        public void zahtevIzmeneRecenzije() { }
        public void napraviPlejlistu() { }
        public void izmeniPlejlistu() { }
        public void izmenaPretplate() { }
    }
}