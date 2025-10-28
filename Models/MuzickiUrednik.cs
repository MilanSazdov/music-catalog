using MusicCatalog.Models;
using MusicCatalog.Models.Enums;
using MusicCatalog.Models.MuzickiSadrzaj;
using System;

namespace MusicCatalog.Models
{
    
    public class MuzickiUrednik : Korisnik
    {
        public MuzickiUrednik(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.MuzickiUrednik)
        {
            Specijalizacija = new List<Zanr>();
        }

        public List<Zanr> Specijalizacija { get; set; }

        public void dodavanjeAlbuma() { }
        public void dodavanjeGrupe() { }
        public void dodavanjeIzvodjaca() { }
        public void dodavanjeNumere() { }
        public void davanjeOcene() { }
        public void pisanjeRecenzije() { }
        public void dodajClanstvo() { }
    }
}