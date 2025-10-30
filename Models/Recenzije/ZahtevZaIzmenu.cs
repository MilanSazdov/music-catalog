using System;
using System.Text.Json.Serialization;
using MusicCatalog.Models.Enums;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.Models.Recenzije
{
    public class ZahtevZaIzmenu
    {
        public int Id { get; set; }
        public int RecenzijaId { get; set; }
        public TipZahteva TipZahteva { get; set; }
        public StatusZahteva Status { get; set; } = StatusZahteva.NA_CEKANJU;

        // Optional, only for IZMENA
        public int? NovaOcenaVrednost { get; set; }

        // Snapshot of data at the time of creating the request
        public Recenzija RecenzijaSnapshot { get; set; } = new Recenzija();

        // Creator of the request
        public string AutorEmail { get; set; } = string.Empty;
        public DateTime DatumKreiranja { get; set; } = DateTime.Now;
    }
}
