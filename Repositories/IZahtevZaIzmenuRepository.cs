using System.Collections.Generic;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.Repositories
{
    public interface IZahtevZaIzmenuRepository
    {
        List<ZahtevZaIzmenu> GetAll();
        ZahtevZaIzmenu? GetById(int id);
        void Add(ZahtevZaIzmenu z);
        void Update(ZahtevZaIzmenu z);
        int GetNextId();
        List<ZahtevZaIzmenu> GetByRecenzijaId(int recenzijaId);
        List<ZahtevZaIzmenu> GetByAutor(string email);
        void Delete(int id);
    }
}