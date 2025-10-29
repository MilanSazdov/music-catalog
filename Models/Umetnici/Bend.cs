using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Models.Umetnici
{
    public class Bend : MuzickiUmetnik
    {
        public string Naziv { get; set; }
        public DateOnly DatumNastanka { get; set; }

        
        public bool Aktivan { get; set; } = true;
      

        public Bend(string naziv, DateOnly datumNastanka, string opis, string slika, bool aktivan = true) : base(opis, slika)
        {
            Naziv = naziv;
            DatumNastanka = datumNastanka;
            Aktivan = aktivan;
        }

        public Bend() { }


    }
}