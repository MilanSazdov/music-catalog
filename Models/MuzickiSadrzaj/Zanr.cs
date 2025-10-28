using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicCatalog.Models.Umetnici;
namespace MusicCatalog.Models.MuzickiSadrzaj
{
    public class Zanr
    {
        public int Id { get; set; }
        public string Naziv { get; set; }
        public List<int> MuzickaDelaIDs { get; set; } = new List<int>();
        public List<int> IzvodjaciIDs { get; set; } = new List<int>();
        public List<int> ZanrIDs { get; set; } = new List<int>();

        public Zanr(string naziv)
        {
            Naziv = naziv;

        }

        public void DodajPodzanr(Zanr podzanr)
        {
            ZanrIDs.Add(podzanr.Id);
        }
        public void UkloniPodzanr(Zanr podzanr)
        {
            ZanrIDs.Remove(podzanr.Id);
        }
        public void DodajMuzickoDelo(MuzickoDelo muzickoDelo)
        {
            MuzickaDelaIDs.Add(muzickoDelo.Id);
        }
        public void UkloniMuzickoDelo(MuzickoDelo muzickoDelo)
        {
            MuzickaDelaIDs.Remove(muzickoDelo.Id);
        }
        
        public void DodajIzvodjaca(Izvodjac izvodjac)
        {
            IzvodjaciIDs.Add(izvodjac.Id);
        }
        public void UkloniIzvodjaca(Izvodjac izvodjac)
        {
            IzvodjaciIDs.Remove(izvodjac.Id);
        }
        
    }
}
