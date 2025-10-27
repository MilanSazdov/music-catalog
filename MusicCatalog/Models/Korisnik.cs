using System.Text.Json.Serialization;

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
        public Uloga Uloga { get; set; }

        public Korisnik(string email, string ime, string prezime, string lozinka, Uloga uloga)
        {
            Email = email;
            Ime = ime;
            Prezime = prezime;
            Lozinka = lozinka;
            Uloga = uloga;
        }

        // --- Metode iz UML dijagrama ---
        public virtual void logovanje()
        {
            // Logika za logovanje
        }

        public virtual void pregledSadrzaja()
        {
            // Logika za pregled sadrzaja
        }

        public virtual void pretragaSadrzaja()
        {
            // Logika za pretragu sadrzaja
        }

        public virtual void ucestvovanjeAnketa()
        {
            // Logika za ucestvovanje u anketi
        }

        public override string ToString()
        {
            return $"[{Uloga}] {Ime} {Prezime} ({Email})";
        }
    }
}