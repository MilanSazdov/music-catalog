using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Models.Umetnici
{
    public class Bend: MuzickiUmetnik
    {
        public string Naziv;
        public DateOnly DatumNastanka;
        bool Aktivan = true;
        
        Bend(string naziv, DateOnly datumNastanka, string opis, string slika,bool aktivan = true)
        {
            Naziv = naziv;
            DatumNastanka = datumNastanka;
            Aktivan = aktivan;
            Opis = opis;
            Slika = slika;
        }
    



    }
}
