using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicCatalog.Models.MuzickiSadrzaj
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Pesma), typeDiscriminator: "Pesma")]
    [JsonDerivedType(typeof(Album), typeDiscriminator: "Album")]
    public abstract class MuzickoDelo
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public TimeSpan Trajanje { get; set; }
        public DateTime DatumIzdanja { get; set; }

        public string Slika { get; set; } = string.Empty;

        public List<int> ZanrIDs { get; set; } = new List<int>();

        protected MuzickoDelo() { }

        protected MuzickoDelo(int id, string naziv, TimeSpan trajanje, DateTime datumIzdanja, List<int> zanrovi)
        {
            Id = id;
            Naziv = naziv;
            Trajanje = trajanje;
            DatumIzdanja = datumIzdanja;
            ZanrIDs = zanrovi;
        }
    }
}
