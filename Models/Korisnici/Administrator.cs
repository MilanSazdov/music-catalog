using MusicCatalog.Models.Enums;


namespace MusicCatalog.Models.Korisnici
{
    // ISPRAVKA: Promenjeno iz 'internal class' u 'public class'
    public class Administrator : Korisnik
    {
        public Administrator(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.Administrator)
        {
        }

        // --- Metode iz UML dijagrama ---
        public void blokiranjeKorisnika() { }
        public void kreiranjeAnkete() { }
        public void upravljanjeReklamama() { }
        public void pregledUrednika() { }
        public void uredjivanjePocetneStrane() { }
        public void brisanjeRecenzija() { }
        public void dodavanjePesme() { }
        public void dodavanjeAlbuma() { }
        public void dodavanjeGrupe() { }
        public void dodavanjeIzvodjaca() { }
        public void registracijaUrednika() { }
        public void zadavanjeZadatka() { }
        public void prihvatanjeZahtevaIzmene() { }
        public void napraviTopListu() { }
        public void dodajClanstvo() { }
    }
}