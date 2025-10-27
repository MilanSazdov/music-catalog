namespace MusicCatalog.Models
{
    // RegistrovaniKorisnik JE Korisnik
    internal class RegistrovaniKorisnik : Korisnik
    {
        // --- Atributi specificni za Registrovanog Korisnika (iz UML dijagrama) ---

        public bool Pretplacen { get; set; }

        // UML dijagram kaze "= false", sto postavljamo u konstruktoru
        public bool Blokiran { get; set; }

        public RegistrovaniKorisnik(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.RegistrovaniKorisnik)
        {
            // Postavljanje default vrednosti iz UML dijagrama
            Blokiran = false;
            Pretplacen = false; // Pretpostavka, dijagram ne navodi default
        }

        // --- Metode specificne za Registrovanog Korisnika (iz UML dijagrama) ---

        public void stream()
        {
            // TODO: Implementirati logiku
        }

        public void download()
        {
            // TODO: Implementirati logiku
        }

        public void dodavanjeFavorita()
        {
            // TODO: Implementirati logiku
        }

        public void brisanjeNaloga()
        {
            // TODO: Implementirati logiku
        }

        public void davanjeOcene()
        {
            // TODO: Implementirati logiku
        }

        public void pisanjeRecenzije()
        {
            // TODO: Implementirati logiku
        }

        public void zahtevIzmeneRecenzije()
        {
            // TODO: Implementirati logiku
        }

        public void napraviPlejlistu()
        {
            // TODO: Implementirati logiku
        }

        public void izmeniPlejlistu()
        {
            // TODO: Implementirati logiku
        }

        public void izmenaPretplate()
        {
            // TODO: Implementirati logiku
        }
    }
}