using System;
using System.Collections.Generic;

namespace MusicCatalog.Models.MuzickiSadrzaj
{
    public class Album : MuzickoDelo
    {
        public Album() { }

        public Album(int id, string naziv, TimeSpan trajanje, DateTime datumIzdanja, List<int> zanrIDs)
            : base(id, naziv, trajanje, datumIzdanja, zanrIDs)
        {
        }
    }
}
