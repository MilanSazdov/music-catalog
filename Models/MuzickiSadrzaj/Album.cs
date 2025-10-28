using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Models.MuzickiSadrzaj
{
    public class Album : MuzickoDelo
    {
        public Album(int id, string naziv, TimeSpan trajanje, DateTime datumIzdanja, List<int> zanrovi) : base(id, naziv, trajanje, datumIzdanja, zanrovi)
        {
        }
    }
}
