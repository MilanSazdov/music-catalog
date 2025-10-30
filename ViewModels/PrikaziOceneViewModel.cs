using System.Collections.ObjectModel;
using System.Linq;
using MusicCatalog.Models;
using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Models.Recenzije;
using MusicCatalog.Repositories;
using System.Windows.Input;
using MusicCatalog.Utils;
using System;

namespace MusicCatalog.ViewModels
{
    public class PrikaziOceneViewModel : ViewModelBase
    {
        public class RecenzijaPrikaz
        {
            public string KorisnikIme { get; set; } = string.Empty;
            public string KorisnikTip { get; set; } = string.Empty;
            public int Vrednost { get; set; }
            public string? Opis { get; set; }
        }

        private readonly IKorisnikRepository _korisnikRepo;
        private readonly IRecenzijaRepository _recRepo;
        private readonly IOcenaRepository _ocenaRepo;
        private readonly MuzickoDelo _delo;

        public string DeloNaziv => _delo.Naziv;
        public ObservableCollection<RecenzijaPrikaz> Stavke { get; } = new();

        public ICommand CloseCommand { get; }
        public Action? CloseWindow { get; set; }
        

        public PrikaziOceneViewModel(IKorisnikRepository korisnikRepo, IRecenzijaRepository recRepo, IOcenaRepository ocenaRepo, MuzickoDelo delo)
        {
            _korisnikRepo = korisnikRepo;
            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;
            _delo = delo;

            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());

            Load();
        }

        private void Load()
        {
            Stavke.Clear();
            var recs = _recRepo.GetAll().Where(r => r.MuzickoDeloId == _delo.Id).ToList();
            foreach (var r in recs)
            {
                var k = _korisnikRepo.GetByEmail(r.KorisnikEmail);
                var ocena = _ocenaRepo.GetByRecenzijaId(r.Id);
                Stavke.Add(new RecenzijaPrikaz
                {
                    KorisnikIme = k != null ? $"{k.Ime} {k.Prezime}" : r.KorisnikEmail,
                    KorisnikTip = k != null ? k.Uloga.ToString() : r.KorisnikUloga.ToString(),
                    Vrednost = ocena?.Vrednost ?? 0,
                    Opis = r.Opis
                });
            }
        }
    }
}