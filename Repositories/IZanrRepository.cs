using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicCatalog.Models.MuzickiSadrzaj;
namespace MusicCatalog.Repositories
{
    public interface IZanrRepository
    {

        List<Zanr> GetAll();
        Zanr? GetById(int id);
        void Add(Zanr zanr);
        void Update(Zanr zanr);
        void Delete(int id);
        int GetNextId();
    }
}
