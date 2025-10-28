using System;
using System.Collections.Generic;

namespace MusicCatalog.Models.MuzickiSadrzaj
{
    public class Pesma : MuzickoDelo
    {
        public Pesma() { }

        public Pesma(int id, string naziv, TimeSpan trajanje, DateTime datumIzdanja, List<int> zanrIDs)
            : base(id, naziv, trajanje, datumIzdanja, zanrIDs)
        {
        }
    }
}
