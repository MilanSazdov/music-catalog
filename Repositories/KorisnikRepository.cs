using MusicCatalog.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MusicCatalog.Repositories
{
    /// <summary>
    /// Implementacija IKorisnikRepository koja koristi JSON fajl za čuvanje podataka.
    /// </summary>
    public class KorisnikRepository : IKorisnikRepository
    {
        // Putanja do fajla gde se čuvaju podaci.
        private readonly string _filePath;

        // Opcije za JSON serijalizaciju (npr. da lepo formatira JSON).
        private readonly JsonSerializerOptions _options;

        // Lokalna lista korisnika (keš u memoriji).
        private List<Korisnik> _korisnici;

        // Optional cross-repo refs for cascade ops
        private readonly IRecenzijaRepository? _recRepo;
        private readonly IOcenaRepository? _ocenaRepo;
        private readonly IZahtevZaIzmenuRepository? _zahtevRepo;

        public KorisnikRepository(string filePath = "Data/korisnici.json")
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true // Čini JSON fajl čitljivim
            };

            // Učitaj podatke iz fajla čim se repozitorijum kreira
            Load();
        }

        public KorisnikRepository(IRecenzijaRepository recRepo, IOcenaRepository ocenaRepo, IZahtevZaIzmenuRepository zahtevRepo, string filePath = "Data/korisnici.json") : this(filePath)
        {
            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _zahtevRepo = zahtevRepo;
        }

        /// <summary>
        /// Učitava korisnike iz JSON fajla u listu _korisnici.
        /// </summary>
        private void Load()
        {
            // Ako fajl ne postoji, kreiraj novi direktorijum i praznu listu.
            if (!File.Exists(_filePath))
            {
                _korisnici = new List<Korisnik>();
                // Path.GetDirectoryName(_filePath) vraća "Data"
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _korisnici = new List<Korisnik>();
                    return;
                }

                // OVO JE NAJVAŽNIJI DEO:
                // Zahvaljujući [JsonDerivedType] atributima na klasi Korisnik,
                // JsonSerializer će znati da kreira tačne instance
                // (Administrator, MuzickiUrednik, RegistrovaniKorisnik).
                _korisnici = JsonSerializer.Deserialize<List<Korisnik>>(json);
            }
            catch (JsonException)
            {
                // Ako je JSON fajl oštećen, počni sa praznom listom
                _korisnici = new List<Korisnik>();
            }
        }

        public void Add(Korisnik korisnik)
        {
            // Proveri da li email već postoji
            if (GetByEmail(korisnik.Email) != null)
            {
                // U realnoj aplikaciji, ovde bi trebalo baciti izuzetak (Exception)
                return;
            }
            _korisnici.Add(korisnik);
        }

        public void Delete(string email)
        {
            var korisnik = GetByEmail(email);
            if (korisnik != null)
            {
                // Cascade: delete recenzije, ocene, zahtevi for this user if repos are provided
                if (_recRepo != null && _ocenaRepo != null && _zahtevRepo != null)
                {
                    var recs = _recRepo.GetByKorisnik(email);
                    var recIds = recs.Select(r => r.Id).ToList();
                    if (recIds.Count >0)
                    {
                        _ocenaRepo.DeleteManyByRecenzije(recIds);
                        var zahtevi = _zahtevRepo.GetAll().Where(z => recIds.Contains(z.RecenzijaId) || z.AutorEmail.Equals(email, System.StringComparison.OrdinalIgnoreCase)).Select(z => z.Id).ToList();
                        foreach (var zId in zahtevi) _zahtevRepo.Delete(zId);
                        _recRepo.DeleteMany(recIds);
                    }
                    else
                    {
                        // remove any requests authored by the user even if no recenzija currently in storage
                        var zahteviOnly = _zahtevRepo.GetByAutor(email).Select(z => z.Id).ToList();
                        foreach (var zId in zahteviOnly) _zahtevRepo.Delete(zId);
                    }
                }
                _korisnici.Remove(korisnik);
            }
        }

        public List<Korisnik> GetAll()
        {
            return _korisnici;
        }

        public Korisnik? GetByEmail(string email)
        {
            // Koristimo LINQ.
            // StringComparison.OrdinalIgnoreCase čini pretragu case-insensitive
            // (npr. "admin@mc.com" je isto kao "Admin@mc.com")
            return _korisnici.FirstOrDefault(k => k.Email.Equals(email, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Čuva trenutno stanje liste _korisnici u JSON fajl.
        /// </summary>
        public void SaveChanges()
        {
            // Serijalizuje celu listu u JSON string
            string json = JsonSerializer.Serialize(_korisnici, _options);

            // Upisuje string u fajl
            File.WriteAllText(_filePath, json);
        }

        public void Update(Korisnik korisnik)
        {
            // Pronađi postojećeg korisnika
            var postojeci = GetByEmail(korisnik.Email);
            if (postojeci != null)
            {
                // Ukloni staru verziju
                _korisnici.Remove(postojeci);
            }
            // Dodaj novu (ažuriranu) verziju
            _korisnici.Add(korisnik);
        }
    }
}