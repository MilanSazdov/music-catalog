// Datoteka: Repositories/MuzickiUmetnikRepository.cs
using MusicCatalog.Models.Umetnici;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization; // Potrebno za ReferenceHandler

namespace MusicCatalog.Repositories
{
    public class MuzickiUmetnikRepository : IMuzickiUmetnikRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private List<MuzickiUmetnik> _umetnici;

        public MuzickiUmetnikRepository(string filePath = "Data/umetnici.json")
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            
                PropertyNameCaseInsensitive = true
            };
            Load();
        }

        private void Load()
        {

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                _umetnici = new List<MuzickiUmetnik>();
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _umetnici = new List<MuzickiUmetnik>();
                    return;
                }
                
                _umetnici = JsonSerializer.Deserialize<List<MuzickiUmetnik>>(json, _options) ?? new List<MuzickiUmetnik>();
            }
            catch (JsonException ex)
            {
               
                System.Diagnostics.Debug.WriteLine($"Greška prilikom čitanja {_filePath}: {ex.Message}");
                _umetnici = new List<MuzickiUmetnik>();
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine($"Neočekivana greška prilikom čitanja {_filePath}: {ex.Message}");
                _umetnici = new List<MuzickiUmetnik>();
            }
        }

        public void Add(MuzickiUmetnik umetnik)
        {

            if (umetnik.Id == 0)
            {
                umetnik.Id = GetNextId();
            }
            _umetnici.Add(umetnik);
        }

        public void Delete(int id)
        {
            var umetnik = GetById(id);
            if (umetnik != null)
            {
                _umetnici.Remove(umetnik);
            }
        }

        public List<MuzickiUmetnik> GetAll()
        {

            return new List<MuzickiUmetnik>(_umetnici);
        }

        public MuzickiUmetnik? GetById(int id)
        {
            return _umetnici.FirstOrDefault(u => u.Id == id);
        }

        public int GetNextId()
        {
            
            if (!_umetnici.Any())
                return 1;
            return _umetnici.Max(u => u.Id) + 1;
        }

        public void SaveChanges()
        {
      
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            try
            {
                string json = JsonSerializer.Serialize(_umetnici, _options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Greška prilikom snimanja {_filePath}: {ex.Message}");
        
            }
        }

        public void Update(MuzickiUmetnik umetnik)
        {
            var postojeci = GetById(umetnik.Id);
            if (postojeci != null)
            {
                _umetnici.Remove(postojeci);
                _umetnici.Add(umetnik);

            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Umetnik sa ID={umetnik.Id} nije pronađen za ažuriranje.");
            }
        }
    }
}