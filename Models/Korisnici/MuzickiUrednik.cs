using MusicCatalog.Models;
using MusicCatalog.Models.Enums;
using System;

namespace MusicCatalog.Models.Korisnici
{
    // ISPRAVKA: Promenjeno iz 'internal class' u 'public class'
    public class MuzickiUrednik : Korisnik
    {
        public MuzickiUrednik(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.MuzickiUrednik)
        {
        }

        // --- Metode iz UML dijagrama ---
        public void dodavanjeAlbuma() { }
        public void dodavanjeGrupe() { }
        public void dodavanjeIzvodjaca() { }
        public void dodavanjeNumere() { }
        public void davanjeOcene() { }
        public void pisanjeRecenzije() { }
        public void dodajClanstvo() { }
    }
}