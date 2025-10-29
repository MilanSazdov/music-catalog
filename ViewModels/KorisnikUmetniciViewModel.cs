using MusicCatalog.Models.Umetnici;
using MusicCatalog.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MusicCatalog.ViewModels
{
    public class KorisnikUmetniciViewModel : ViewModelBase
    {
        private readonly IMuzickiUmetnikRepository _umetniciRepository;

        public ObservableCollection<MuzickiUmetnik> Umetnici { get; } = new();

        public KorisnikUmetniciViewModel(IMuzickiUmetnikRepository umetniciRepo)
        {
            _umetniciRepository = umetniciRepo;
            Load();
        }

        private void Load()
        {
            Umetnici.Clear();
            foreach (var d in _umetniciRepository.GetAll().OrderBy(d => d is Bend))
            {
                Umetnici.Add(d);
            }
        }
    }
}