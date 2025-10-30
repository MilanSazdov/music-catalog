using MusicCatalog.Models;
using MusicCatalog.Models.Enums;
using MusicCatalog.Models.MuzickiSadrzaj;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicCatalog.Models
{
    public class MuzickiUrednik : Korisnik
    {
        // Parameterless ctor for JSON/WPF
        public MuzickiUrednik()
            : base()
        {
            Uloga = Uloga.MuzickiUrednik;
            Specijalizacija = new();
        }

        public MuzickiUrednik(string email, string ime, string prezime, string lozinka)
            : base(email, ime, prezime, lozinka, Uloga.MuzickiUrednik)
        {
            Specijalizacija = new List<Zanr>();
        }

        public List<Zanr> Specijalizacija { get; set; } = new();

        public string SpecijalizacijaString =>
            Specijalizacija is { Count: > 0 }
                ? string.Join(", ", Specijalizacija.Select(z => z.Naziv))
                : string.Empty;

        public void dodavanjeAlbuma() { }
        public void dodavanjeGrupe() { }
        public void dodavanjeIzvodjaca() { }
        public void dodavanjeNumere() { }
        public void davanjeOcene() { }
        public void pisanjeRecenzije() { }
        public void dodajClanstvo() { }
    }
}