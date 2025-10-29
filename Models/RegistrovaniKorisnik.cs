using MusicCatalog.Models.Enums;
using System.Collections.Generic;

namespace MusicCatalog.Models
{
    
    public class RegistrovaniKorisnik : Korisnik
    {
        public bool Pretplacen { get; set; }
        public bool Blokiran { get; set; }

        public List<int> FavoritDeloIDs { get; set; } = new List<int>();

        public List<int> FavoritUmetnikIDs { get; set; } = new List<int>();

        public RegistrovaniKorisnik(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.RegistrovaniKorisnik)
        {
            Blokiran = false;
            Pretplacen = false;
        }

        
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