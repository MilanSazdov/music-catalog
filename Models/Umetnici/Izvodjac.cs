using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Models.Umetnici
{
    public class Izvodjac : MuzickiUmetnik
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }

        public List<int> MuzickoDeloIDs { get; set; } = new List<int>();

        public Izvodjac(string ime, string prezime, string opis, string slika) : base(opis, slika)
        {
            Ime = ime;
            Prezime = prezime;

        }

        public Izvodjac() { }
    }
}