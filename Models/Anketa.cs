using MusicCatalog.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Models
{
    public class Anketa
    {
        public int Id { get; set; }

        public Period Period { get; set; }

        public DateTime DatumPocetka { get; set; }

        public DateTime DatumKraja { get; set; }

        public string Naziv { get; set; } = string.Empty;
    }
}
