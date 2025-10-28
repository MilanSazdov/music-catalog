using MusicCatalog.Models.MuzickiSadrzaj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace MusicCatalog.Repositories
{
    public class ZanrRepository : IZanrRepository
    {

        private readonly string _filePath;

        private readonly JsonSerializerOptions _options;

        public List<Zanr> _zanrovi = new List<Zanr>();

        public ZanrRepository(string filePath = "Data/zanrovi.json")
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            Load();
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _zanrovi = new List<Zanr>();
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                return;
            }
            try
            {
                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _zanrovi = new List<Zanr>();
                    return;
                }
                _zanrovi = JsonSerializer.Deserialize<List<Zanr>>(json, _options) ?? new List<Zanr>();
            }
            catch (JsonException)
            {
                _zanrovi = new List<Zanr>();
            }
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(_zanrovi, _options);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }


        public List<Zanr> GetAll()
        {
            return _zanrovi;
        }
        public Zanr? GetById(int id)
        {
            return _zanrovi.FirstOrDefault(z => z.Id == id);
        }
        public void Add(Zanr zanr)
        {
            try
            {
                _zanrovi.Add(zanr);
                Save();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Greška prilikom dodavanja žanra.", ex);
            }
        }
        public void Update(Zanr zanr)
        {
            var existingZanr = GetById(zanr.Id);
            if (existingZanr != null)
            {
                int index = _zanrovi.IndexOf(existingZanr);
                _zanrovi[index] = zanr;
                Save();
            }
            else
            {
                throw new KeyNotFoundException("Žanr sa datim ID-jem nije pronađen.");
            }
        }
        public void Delete(int id)
        {
            var zanr = GetById(id);
            if (zanr != null)
            {
                _zanrovi.Remove(zanr);
                Save();
            }
            else
            {
                throw new KeyNotFoundException("Žanr sa datim ID-jem nije pronađen.");
            }
        }
        public int GetNextId()
        {
            if (_zanrovi.Count == 0)
            {
                return 1;
            }
            return _zanrovi.Max(z => z.Id) + 1;
        }
        
    }
}
