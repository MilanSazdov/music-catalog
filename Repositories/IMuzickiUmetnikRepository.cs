
using MusicCatalog.Models.Umetnici;
using System.Collections.Generic;

namespace MusicCatalog.Repositories
{
    public interface IMuzickiUmetnikRepository
    {
        List<MuzickiUmetnik> GetAll();
        MuzickiUmetnik? GetById(int id);
        int GetNextId();
        void Add(MuzickiUmetnik umetnik);
        void Update(MuzickiUmetnik umetnik);
        void Delete(int id);
        void SaveChanges();
    }
}