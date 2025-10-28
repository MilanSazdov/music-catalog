using MusicCatalog.Models;
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
    public interface IAnketaRepository
    {
        void Add(Anketa anketa);
        void Delete(int id);
        List<Anketa> GetAll();
        Anketa? GetById(int id);
        void SaveChanges();
        void Update(Anketa anketa);
    }

    public class AnketaRepository : IAnketaRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private List<Anketa> _ankete;

        public AnketaRepository(string filePath = "Data/ankete.json")
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
                _ankete = new List<Anketa>();
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
                    _ankete = new List<Anketa>();
                    return;
                }

                _ankete = JsonSerializer.Deserialize<List<Anketa>>(json, _options) ?? new List<Anketa>();
            }
            catch (JsonException)
            {
                _ankete = new List<Anketa>();
            }
            catch (IOException)
            {
                _ankete = new List<Anketa>();
            }
        }

        public void Add(Anketa anketa)
        {
            if (GetById(anketa.Id) != null)
            {
                return;
            }
            _ankete.Add(anketa);
        }

        public void Delete(int id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                _ankete.Remove(existing);
            }
        }

        public List<Anketa> GetAll()
        {
            return _ankete;
        }

        public Anketa? GetById(int id)
        {
            return _ankete.FirstOrDefault(a => a.Id == id);
        }

        public void Update(Anketa anketa)
        {
            var existing = GetById(anketa.Id);
            if (existing != null)
            {
                _ankete.Remove(existing);
            }
            _ankete.Add(anketa);
        }

        public void SaveChanges()
        {
            string json = JsonSerializer.Serialize(_ankete, _options);

            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(_filePath, json);
        }
    }
}
