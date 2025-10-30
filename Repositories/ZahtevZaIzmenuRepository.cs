using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.Repositories
{
    public class ZahtevZaIzmenuRepository : IZahtevZaIzmenuRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private List<ZahtevZaIzmenu> _items = new();

        public ZahtevZaIzmenuRepository(string filePath = "Data/zahtevi_izmene.json")
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
            _items = string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<ZahtevZaIzmenu>>(json, _options) ?? new();
        }

        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_items, _options));
        }

        public List<ZahtevZaIzmenu> GetAll() => _items;
        public ZahtevZaIzmenu? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
        public void Add(ZahtevZaIzmenu z)
        {
            if (z.Id == 0) z.Id = GetNextId();
            _items.Add(z);
            Save();
        }
        public void Update(ZahtevZaIzmenu z)
        {
            var ex = GetById(z.Id);
            if (ex == null) return;
            var idx = _items.IndexOf(ex);
            _items[idx] = z;
            Save();
        }
        public void Delete(int id)
        {
            var ex = GetById(id);
            if (ex == null) return;
            _items.Remove(ex);
            Save();
        }
        public int GetNextId() => _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
        public List<ZahtevZaIzmenu> GetByRecenzijaId(int recenzijaId) => _items.Where(x => x.RecenzijaId == recenzijaId).ToList();
        public List<ZahtevZaIzmenu> GetByAutor(string email) => _items.Where(x => x.AutorEmail == email).ToList();
    }
}