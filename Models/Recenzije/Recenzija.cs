using System;
using System.Text.Json.Serialization;
using MusicCatalog.Models.Enums;

namespace MusicCatalog.Models.Recenzije
{
    public class Recenzija
    {
        public int Id { get; set; }
        public int MuzickoDeloId { get; set; }

        // Identifies the author
        public string KorisnikEmail { get; set; } = string.Empty;
        public Uloga KorisnikUloga { get; set; }

        // Optional
        public string? Opis { get; set; }

        public DateTime Datum { get; set; } = DateTime.Now;
    }
}