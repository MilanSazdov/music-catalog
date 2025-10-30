using System.Collections.Generic;
using MusicCatalog.Models.Recenzije;

namespace MusicCatalog.Repositories
{
    public interface IOcenaRepository
    {
        List<Ocena> GetAll();
        Ocena? GetById(int id);
        Ocena? GetByRecenzijaId(int recenzijaId);
        List<Ocena> GetByRecenzije(IEnumerable<int> recenzijaIds);
        void Add(Ocena ocena);
        void Update(Ocena ocena);
        void Delete(int id);
        void DeleteManyByRecenzije(IEnumerable<int> recenzijaIds);
        int GetNextId();
    }
}