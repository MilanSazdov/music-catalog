using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.Repositories
{
    public class RecenzijaRepository : IRecenzijaRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private List<Recenzija> _items = new();

        public RecenzijaRepository(string filePath = "Data/recenzije.json")
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions { WriteIndented = true };
            Load();
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _items = new();
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                return;
            }
            var json = File.ReadAllText(_filePath);
            _items = string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<Recenzija>>(json, _options) ?? new();
        }

        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_items, _options));
        }

        public List<Recenzija> GetAll() => _items;
        public Recenzija? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

        public Recenzija? GetByKorisnikAndDelo(string korisnikEmail, int deloId)
            => _items.FirstOrDefault(x => x.KorisnikEmail.Equals(korisnikEmail, StringComparison.OrdinalIgnoreCase) && x.MuzickoDeloId == deloId);

        public List<Recenzija> GetByDeloId(int deloId) => _items.Where(x => x.MuzickoDeloId == deloId).ToList();
        public List<Recenzija> GetByKorisnik(string korisnikEmail) => _items.Where(x => x.KorisnikEmail.Equals(korisnikEmail, StringComparison.OrdinalIgnoreCase)).ToList();

        public void Add(Recenzija rec)
        {
            if (rec.Id == 0) rec.Id = GetNextId();
            _items.Add(rec);
            Save();
        }

        public void Update(Recenzija rec)
        {
            var ex = GetById(rec.Id);
            if (ex == null) return;
            var idx = _items.IndexOf(ex);
            _items[idx] = rec;
            Save();
        }

        public void Delete(int id)
        {
            var ex = GetById(id);
            if (ex == null) return;
            _items.Remove(ex);
            Save();
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            var set = ids.ToHashSet();
            _items.RemoveAll(r => set.Contains(r.Id));
            Save();
        }

        public int GetNextId() => _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
    }
}