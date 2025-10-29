using MusicCatalog.Models;
using MusicCatalog.Models.Umetnici;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MusicCatalog.Repositories
{
    public class ClanstvoRepository : IClanstvoRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private List<Clanstvo> _clanstva;

        public ClanstvoRepository(string filePath = "Data/clanstvo.json")
        {
            _filePath = filePath;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            Load();
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _clanstva = new List<Clanstvo>();
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _clanstva = new List<Clanstvo>();
                    return;
                }

                _clanstva = JsonSerializer.Deserialize<List<Clanstvo>>(json, _options) ?? new List<Clanstvo>();
            }
            catch (JsonException)
            {
                _clanstva = new List<Clanstvo>();
            }
            catch (IOException)
            {
                _clanstva = new List<Clanstvo>();
            }
        }

        
        public void Add(Clanstvo clanstvo)
        {
            if (clanstvo.Id == 0)
            {
                clanstvo.Id = GetNextId();
            }
            else if (GetById(clanstvo.Id) != null)
            {
                
                return;
            }
            _clanstva.Add(clanstvo);
        }
        

        public void Delete(int id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                _clanstva.Remove(existing);
            }
        }

        public List<Clanstvo> GetAll()
        {
            return _clanstva;
        }

        public Clanstvo? GetById(int id)
        {
            return _clanstva.FirstOrDefault(a => a.Id == id);
        }

        public void Update(Clanstvo clanstvo)
        {
            var existing = GetById(clanstvo.Id);
            if (existing != null)
            {
                int index = _clanstva.IndexOf(existing);
                _clanstva[index] = clanstvo;
            }
            else
            {
                
                Add(clanstvo);
            }
        }

        public void SaveChanges()
        {
            string json = JsonSerializer.Serialize(_clanstva, _options);

            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_filePath, json);
        }

        
        public int GetNextId()
        {
            if (_clanstva.Count == 0)
                return 1;
            return _clanstva.Max(c => c.Id) + 1;
        }
        
    }
}