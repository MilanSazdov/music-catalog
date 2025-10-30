using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.Repositories
{
    public class OcenaRepository : IOcenaRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private List<Ocena> _items = new();

        public OcenaRepository(string filePath = "Data/ocene.json")
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
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return;
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                _items = string.IsNullOrWhiteSpace(json)
                    ? new()
                    : JsonSerializer.Deserialize<List<Ocena>>(json, _options) ?? new();
            }
            catch (JsonException)
            {
                _items = new();
            }
            catch (IOException)
            {
                _items = new();
            }
        }

        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_items, _options));
        }

        public List<Ocena> GetAll() => _items;
        public Ocena? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);
        public Ocena? GetByRecenzijaId(int recenzijaId) => _items.FirstOrDefault(x => x.RecenzijaId == recenzijaId);
        public List<Ocena> GetByRecenzije(IEnumerable<int> recenzijaIds)
        {
            var set = recenzijaIds.ToHashSet();
            return _items.Where(o => set.Contains(o.RecenzijaId)).ToList();
        }
        public void Add(Ocena ocena)
        {
            if (ocena.Id == 0) ocena.Id = GetNextId();
            _items.Add(ocena);
            Save();
        }
        public void Update(Ocena ocena)
        {
            var ex = GetById(ocena.Id);
            if (ex == null) return;
            var idx = _items.IndexOf(ex);
            _items[idx] = ocena;
            Save();
        }
        public void Delete(int id)
        {
            var ex = GetById(id);
            if (ex == null) return;
            _items.Remove(ex);
            Save();
        }
        public void DeleteManyByRecenzije(IEnumerable<int> recenzijaIds)
        {
            var set = recenzijaIds.ToHashSet();
            _items.RemoveAll(o => set.Contains(o.RecenzijaId));
            Save();
        }
        public int GetNextId() => _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
    }
}