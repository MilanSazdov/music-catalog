using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Models.Umetnici
{
    public class Clanstvo
    {
        
        public int Id { get; set; }
        public int UmetnikId { get; set; }
        public int BendId { get; set; }
        public DateOnly datumUclanjenja { get; set; }

        public Clanstvo() { }

    }
}