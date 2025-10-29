using MusicCatalog.Models;
using MusicCatalog.Models.MuzickiSadrzaj;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace MusicCatalog.Repositories
{
    public class MuzickoDeloRepository : IMuzickoDeloRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;
        private readonly IZanrRepository _zanrRepo;

        public List<MuzickoDelo> _muzickaDela = new List<MuzickoDelo>();

        public MuzickoDeloRepository(IZanrRepository zanrRepo, string filePath = "Data/muzicka_dela.json")
        {
            _zanrRepo = zanrRepo;
            _filePath = filePath;
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
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
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
                _muzickaDela = JsonSerializer.Deserialize<List<MuzickoDelo>>(json, _options) ?? new List<MuzickoDelo>();
            }
            catch (JsonException)
            {
                _muzickaDela = new List<MuzickoDelo>();
            }
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(_muzickaDela, _options);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, json);
        }
        #endregion

        public void Add(MuzickoDelo muzickoDelo)
        {
            try
            {
                ValidatateMuzickoDelo(muzickoDelo);
                if (muzickoDelo.Id == 0)
                {
                    muzickoDelo.Id = GetNextId();
                }

                _muzickaDela.Add(muzickoDelo);
                Save();

                // Link to genres ONLY for songs (Pesma).
                if (muzickoDelo is not Album)
                {
                    foreach (var zanrId in muzickoDelo.ZanrIDs.Distinct())
                    {
                        var zanr = _zanrRepo.GetById(zanrId);
                        if (zanr == null) continue;

                        if (!zanr.MuzickaDelaIDs.Contains(muzickoDelo.Id))
                        {
                            zanr.MuzickaDelaIDs.Add(muzickoDelo.Id);
                            _zanrRepo.Update(zanr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greska prilikom dodavanja muzickog dela: {ex.Message}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Delete(int id)
        {
            var muzickoDelo = GetById(id);
            if (muzickoDelo != null)
            {
                _muzickaDela.Remove(muzickoDelo);
                Save();

                // Unlink from all genres
                foreach (var zanr in _zanrRepo.GetAll())
                {
                    if (zanr.MuzickaDelaIDs.Remove(id))
                    {
                        _zanrRepo.Update(zanr);
                    }
                }
            }
        }

        public List<MuzickoDelo> GetAll() => _muzickaDela;

        public MuzickoDelo? GetById(int id) => _muzickaDela.FirstOrDefault(md => md.Id == id);

        public int GetNextId() => _muzickaDela.Count == 0 ? 1 : _muzickaDela.Max(m => m.Id) + 1;

        public void Update(MuzickoDelo muzickoDelo)
        {
            try
            {
                ValidatateMuzickoDelo(muzickoDelo);
                var existing = GetById(muzickoDelo.Id);
                if (existing != null)
                {
                    // Sync genre links ONLY for songs (Pesma).
                    if (muzickoDelo is not Album)
                    {
                        var oldSet = existing.ZanrIDs.Distinct().ToHashSet();
                        var newSet = muzickoDelo.ZanrIDs.Distinct().ToHashSet();

                        var toRemove = oldSet.Except(newSet);
                        var toAdd = newSet.Except(oldSet);

                        foreach (var zanrId in toRemove)
                        {
                            var zanr = _zanrRepo.GetById(zanrId);
                            if (zanr != null && zanr.MuzickaDelaIDs.Remove(muzickoDelo.Id))
                            {
                                _zanrRepo.Update(zanr);
                            }
                        }

                        foreach (var zanrId in toAdd)
                        {
                            var zanr = _zanrRepo.GetById(zanrId);
                            if (zanr != null && !zanr.MuzickaDelaIDs.Contains(muzickoDelo.Id))
                            {
                                zanr.MuzickaDelaIDs.Add(muzickoDelo.Id);
                                _zanrRepo.Update(zanr);
                            }
                        }
                    }

                    int index = _muzickaDela.IndexOf(existing);
                    _muzickaDela[index] = muzickoDelo;
                    Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greska prilikom azuriranja muzickog dela: {ex.Message}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ValidatateMuzickoDelo(MuzickoDelo muzickoDelo)
        {
            if (muzickoDelo == null)
                throw new ArgumentNullException(nameof(muzickoDelo), "Muzicko delo ne moze biti null.");
            if (string.IsNullOrWhiteSpace(muzickoDelo.Naziv))
                throw new ArgumentException("Naziv muzickog dela ne moze biti prazan.");
            if (muzickoDelo.Trajanje <= TimeSpan.Zero)
                throw new ArgumentException("Trajanje muzickog dela mora biti pozitivno.");
            if (muzickoDelo.DatumIzdanja > DateTime.Now)
                throw new ArgumentException("Datum izdanja muzickog dela ne moze biti u buducnosti.");
            if (muzickoDelo.ZanrIDs == null || !muzickoDelo.ZanrIDs.Any())
                throw new ArgumentException("Muzicko delo mora imati bar jedan zanr.");
            if (_muzickaDela.Any(md => md.Id != muzickoDelo.Id && md.Naziv.Equals(muzickoDelo.Naziv, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Muzicko delo sa istim nazivom vec postoji.");
        }
    }
}