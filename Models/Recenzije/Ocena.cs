using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Models.Recenzije
{
    public class Ocena
    {
        //id is same as recenzija's id
        public int Id { get; set; }
        public int RecenzijaId { get; set; }
        public int Vrednost { get; set; } // Vrednost ocene, npr. od 1 do 5
        
    }
}
