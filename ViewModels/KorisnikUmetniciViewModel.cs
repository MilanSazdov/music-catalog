using MusicCatalog.Models.Umetnici;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class KorisnikUmetniciViewModel : ViewModelBase
    {
        private readonly IMuzickiUmetnikRepository _umetniciRepository;
        private readonly IClanstvoRepository _clanstvoRepo;
        private readonly IMuzickoDeloRepository _deloRepo;

        public ObservableCollection<MuzickiUmetnik> Umetnici { get; } = new();

        public ICommand ShowInfoCommand { get; }

        public KorisnikUmetniciViewModel(IMuzickiUmetnikRepository umetniciRepo, IClanstvoRepository clanstvoRepo, IMuzickoDeloRepository deloRepo)
        {
            _umetniciRepository = umetniciRepo;
            _clanstvoRepo = clanstvoRepo;
            _deloRepo = deloRepo;

            ShowInfoCommand = new RelayCommand(ShowInfo);

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

        private void ShowInfo(object? parameter)
        {
            if (parameter is MuzickiUmetnik umetnik)
            {
                
                var vm = new UmetnikInfoViewModel(umetnik, _umetniciRepository, _clanstvoRepo, _deloRepo);

                
                var view = new UmetnikInfoView
                {
                    DataContext = vm,
                    
                    Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                };

                
                view.ShowDialog();
            }
        }
    }
}