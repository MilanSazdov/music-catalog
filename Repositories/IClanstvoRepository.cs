using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Models.Umetnici;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCatalog.Repositories
{
    public interface IClanstvoRepository
    {
        List<Clanstvo> GetAll();
        Clanstvo? GetById(int id);
        void Add(Clanstvo muzickoDelo);
        void Update(Clanstvo muzickoDelo);
        void Delete(int id);
        void SaveChanges();
    }
}
