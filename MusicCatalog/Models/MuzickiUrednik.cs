namespace MusicCatalog.Models
{
    // MuzickiUrednik JE Korisnik
    internal class MuzickiUrednik : Korisnik
    {
        public MuzickiUrednik(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.MuzickiUrednik)
        {
            // Nema dodatnih atributa na dijagramu za Muzickog Urednika
        }

        // --- Metode specificne za Muzickog Urednika (iz UML dijagrama) ---

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

        public void dodavanjeNumere()
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

        public void dodajClanstvo()
        {
            // TODO: Implementirati logiku
        }
    }
}