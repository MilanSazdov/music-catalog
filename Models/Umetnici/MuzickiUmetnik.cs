using MusicCatalog.Models.MuzickiSadrzaj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicCatalog.Models.Umetnici
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Bend), typeDiscriminator: "Bend")]
    [JsonDerivedType(typeof(Izvodjac), typeDiscriminator: "Izvodjac")]
    public class MuzickiUmetnik
    {
        public int Id { get; set; }
        public string Opis { get; set; }
        public string Slika { get; set; }

        public MuzickiUmetnik(string opis, string slika)
        {
            Id = new Random().Next(1, int.MaxValue);
            Opis = opis;
            Slika = slika;
        }

    }
}
