using System.Text.Json.Serialization;
using MusicCatalog.Models.Enums; // <-- DODAJ OVAJ RED

namespace MusicCatalog.Models
{
    [JsonDerivedType(typeof(Administrator), typeDiscriminator: "admin")]
    [JsonDerivedType(typeof(MuzickiUrednik), typeDiscriminator: "urednik")]
    [JsonDerivedType(typeof(RegistrovaniKorisnik), typeDiscriminator: "registrovani")]
    public abstract class Korisnik
    {
        public string Email { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Lozinka { get; set; }

        // Ova linija sada radi
        public Uloga Uloga { get; set; }

        public Korisnik(string email, string ime, string prezime, string lozinka, Uloga uloga)
        {
            Email = email;
            Ime = ime;
            Prezime = prezime;
            Lozinka = lozinka;
            Uloga = uloga;
        }

        // (Ostatak tvoje klase ostaje isti)
        public virtual void logovanje() { }
        public virtual void pregledSadrzaja() { }
        public virtual void pretragaSadrzaja() { }
        public virtual void ucestvovanjeAnketa() { }

        public override string ToString()
        {
            return $"[{Uloga}] {Ime} {Prezime} ({Email})";
        }
    }
}