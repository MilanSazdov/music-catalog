using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicCatalog.Models.MuzickiSadrzaj;
namespace MusicCatalog.Repositories
{
    public interface IMuzickoDeloRepository
    {
        List<MuzickoDelo> GetAll();
        MuzickoDelo? GetById(int id);
        void Add(MuzickoDelo muzickoDelo);
        void Update(MuzickoDelo muzickoDelo);
        void Delete(int id);
        void DeleteWithCascade(int id, IRecenzijaRepository recRepo, IOcenaRepository ocenaRepo, IZahtevZaIzmenuRepository zahtevRepo);
        int GetNextId();

    }
}
