namespace MusicCatalog.Models
{
    // Administrator JE Korisnik
    internal class Administrator : Korisnik
    {
        // Prosledjujemo sve parametre baznom konstruktoru
        // i specificiramo Ulogu.
        public Administrator(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.Administrator)
        {
            // Nema dodatnih atributa na dijagramu za Administratora
        }

        // --- Metode specificne za Administratora (iz UML dijagrama) ---

        public void blokiranjeKorisnika()
        {
            // TODO: Implementirati logiku
        }

        public void kreiranjeAnkete()
        {
            // TODO: Implementirati logiku
        }

        public void upravljanjeReklamama()
        {
            // TODO: Implementirati logiku
        }

        public void pregledUrednika()
        {
            // TODO: Implementirati logiku
        }

        public void uredjivanjePocetneStrane()
        {
            // TODO: Implementirati logiku
        }

        public void brisanjeRecenzija()
        {
            // TODO: Implementirati logiku
        }

        public void dodavanjePesme()
        {
            // TODO: Implementirati logiku
        }

        public void dodavanjeAlbuma()
        {
            // TODO: Implementirati logiku
        }

        public void dodavanjeGrupe()
        {
            // TODO: Implementirati logiku
        }

        public void dodavanjeIzvodjaca()
        {
            // TODO: Implementirati logiku
        }

        public void registracijaUrednika()
        {
            // TODO: Implementirati logiku
        }

        public void zadavanjeZadatka()
        {
            // TODO: Implementirati logiku
        }

        public void prihvatanjeZahtevaIzmene()
        {
            // TODO: Implementirati logiku
        }

        public void napraviTopListu()
        {
            // TODO: Implementirati logiku
        }

        public void dodajClanstvo()
        {
            // TODO: Implementirati logiku
        }
    }
}