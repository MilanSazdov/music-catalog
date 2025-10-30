using System.Collections.Generic;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.Repositories
{
    public interface IRecenzijaRepository
    {
        List<Recenzija> GetAll();
        Recenzija? GetById(int id);
        Recenzija? GetByKorisnikAndDelo(string korisnikEmail, int deloId);
        List<Recenzija> GetByDeloId(int deloId);
        List<Recenzija> GetByKorisnik(string korisnikEmail);
        void Add(Recenzija rec);
        void Update(Recenzija rec);
        void Delete(int id);
        void DeleteMany(IEnumerable<int> ids);
        int GetNextId();
    }
}