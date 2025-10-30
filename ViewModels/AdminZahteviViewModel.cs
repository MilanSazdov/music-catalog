using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MusicCatalog.Models.Recenzije;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;

namespace MusicCatalog.ViewModels
{
    public class AdminZahteviViewModel : ViewModelBase
    {
        private readonly IZahtevZaIzmenuRepository _zahtevRepo;
        private readonly IRecenzijaRepository _recRepo;
        private readonly IOcenaRepository _ocenaRepo;

        public ObservableCollection<ZahtevZaIzmenu> NaCekanju { get; } = new();
        public ObservableCollection<ZahtevZaIzmenu> Obradjeni { get; } = new();

        public ICommand PrihvatiCommand { get; }
        public ICommand OdbijCommand { get; }
        public ICommand RefreshCommand { get; }

        public AdminZahteviViewModel(IZahtevZaIzmenuRepository zahtevRepo, IRecenzijaRepository recRepo, IOcenaRepository ocenaRepo)
        {
            _zahtevRepo = zahtevRepo;
            _recRepo = recRepo;
            _ocenaRepo = ocenaRepo;

            PrihvatiCommand = new RelayCommand(p => Prihvati(p as ZahtevZaIzmenu), p => (p as ZahtevZaIzmenu)?.Status == StatusZahteva.NA_CEKANJU);
            OdbijCommand = new RelayCommand(p => Odbij(p as ZahtevZaIzmenu), p => (p as ZahtevZaIzmenu)?.Status == StatusZahteva.NA_CEKANJU);
            RefreshCommand = new RelayCommand(_ => Load());

            Load();
        }

        private void Load()
        {
            NaCekanju.Clear();
            Obradjeni.Clear();
            foreach (var z in _zahtevRepo.GetAll().OrderByDescending(z => z.Id))
            {
                if (z.Status == StatusZahteva.NA_CEKANJU) NaCekanju.Add(z);
                else Obradjeni.Add(z);
            }
        }

        private void Prihvati(ZahtevZaIzmenu? z)
        {
            if (z == null) return;

            var rec = _recRepo.GetById(z.RecenzijaId);
            if (z.TipZahteva == TipZahteva.IZMENA)
            {
                if (rec != null)
                {
                    rec.Datum = DateTime.Now;
                    // keep opis as is
                    _recRepo.Update(rec);
                }
                var ocena = _ocenaRepo.GetByRecenzijaId(z.RecenzijaId);
                if (ocena != null && z.NovaOcenaVrednost is >= 1 and <= 5)
                {
                    ocena.Vrednost = z.NovaOcenaVrednost!.Value;
                    _ocenaRepo.Update(ocena);
                }
            }
            else if (z.TipZahteva == TipZahteva.BRISANJE)
            {
                if (rec != null) _recRepo.Delete(rec.Id);
                var ocena = _ocenaRepo.GetByRecenzijaId(z.RecenzijaId);
                if (ocena != null) _ocenaRepo.Delete(ocena.Id);
            }

            z.Status = StatusZahteva.PRIHVACEN;
            _zahtevRepo.Update(z);
            Load();
        }

        private void Odbij(ZahtevZaIzmenu? z)
        {
            if (z == null) return;
            z.Status = StatusZahteva.ODBIJEN;
            _zahtevRepo.Update(z);
            Load();
        }
    }
}