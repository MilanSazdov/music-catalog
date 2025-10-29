using System;
using System.Collections.Generic;

namespace MusicCatalog.Models.MuzickiSadrzaj
{
    public class Album : MuzickoDelo
    {
        // IDs of songs that belong to this album
        public List<int> PesmaIDs { get; set; } = new();

        public Album() { }

        public Album(int id, string naziv, TimeSpan trajanje, DateTime datumIzdanja, List<int> zanrIDs, List<int>? pesmaIDs = null)
            : base(id, naziv, trajanje, datumIzdanja, zanrIDs)
        {
            if (pesmaIDs != null)
            {
                PesmaIDs = pesmaIDs;
            }
        }
    }
}
