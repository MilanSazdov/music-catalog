using MusicCatalog.Models.Korisnici;
using MusicCatalog.Models.MuzickiSadrzaj;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MusicCatalog.Repositories
{
    public class MuzickoDeloRepository : IMuzickoDeloRepository
    {
        private readonly string _filePath;

        private readonly JsonSerializerOptions _options;

        public List<MuzickoDelo> _muzickaDela = new List<MuzickoDelo>();

        MuzickoDeloRepository(List<Zanr> zanrovi,string filePath = "Data/muzicka_dela.json")
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Load();
            
        }
        #region Json Load and Save
        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _muzickaDela = new List<MuzickoDelo>();
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _muzickaDela = new List<MuzickoDelo>();
                    return;
                }
                _muzickaDela = JsonSerializer.Deserialize<List<MuzickoDelo>>(json);
            }
            catch (JsonException)
            {
                _muzickaDela = new List<MuzickoDelo>();
            }
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(_muzickaDela, _options);
            File.WriteAllText(_filePath, json);
        }
#endregion


        public void Add(MuzickoDelo muzickoDelo)
        {
            try 
            {
                //treba id samo postaviti   
                ValidatateMuzickoDelo(muzickoDelo);
                _muzickaDela.Add(muzickoDelo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greska prilikom dodavanja muzickog dela: {ex.Message}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Save();

        }
        public void Delete(int id)
        {
            var muzickoDelo = GetById(id);
            if (muzickoDelo != null)
            {
                _muzickaDela.Remove(muzickoDelo);
            }
            Save();
        }
        public List<MuzickoDelo> GetAll()
        {
            return _muzickaDela;
        }
        public MuzickoDelo? GetById(int id)
        {
            return _muzickaDela.FirstOrDefault(md => md.Id == id);
        }
        public int GetNextId()
        {
            return _muzickaDela.Last().Id + 1;
        }

        public void Update(MuzickoDelo muzickoDelo)
        {
            try 
            {
                ValidatateMuzickoDelo(muzickoDelo);
                var existingMuzickoDelo = GetById(muzickoDelo.Id);
                if (existingMuzickoDelo != null)
                {
                    int index = _muzickaDela.IndexOf(existingMuzickoDelo);
                    _muzickaDela[index] = muzickoDelo;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greska prilikom azuriranja muzickog dela: {ex.Message}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Save();
        }





        public void ValidatateMuzickoDelo(MuzickoDelo muzickoDelo)
        {
            if (muzickoDelo == null)
            {
                throw new ArgumentNullException(nameof(muzickoDelo), "Muzicko delo ne moze biti null.");
            }
            if (string.IsNullOrWhiteSpace(muzickoDelo.Naziv))
            {
                throw new ArgumentException("Naziv muzickog dela ne moze biti prazan.");
            }
            if (muzickoDelo.Trajanje <= TimeSpan.Zero)
            {
                throw new ArgumentException("Trajanje muzickog dela mora biti pozitivno.");
            }
            if (muzickoDelo.DatumIzdanja > DateTime.Now)
            {
                throw new ArgumentException("Datum izdanja muzickog dela ne moze biti u buducnosti.");
            }
            if (muzickoDelo.ZanrIDs == null || !muzickoDelo.ZanrIDs.Any())
            {
                throw new ArgumentException("Muzicko delo mora imati bar jedan zanr.");
            }
            if (_muzickaDela.Any(md => md.Id != muzickoDelo.Id && md.Naziv.Equals(muzickoDelo.Naziv, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Muzicko delo sa istim nazivom vec postoji.");
            }
        }
    }
}
