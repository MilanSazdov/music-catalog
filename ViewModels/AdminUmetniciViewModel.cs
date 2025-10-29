using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Models.Umetnici;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class AdminUmetniciViewModel : ViewModelBase
    {
        private readonly IMuzickiUmetnikRepository _umetniciRepository;
        private readonly IClanstvoRepository _clanstvoRepository;

        public ObservableCollection<MuzickiUmetnik> Umetnici { get; } = new();
        private MuzickiUmetnik? _selected;
        public MuzickiUmetnik? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddIzvodjacCommand { get; }
        public ICommand AddBendCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public AdminUmetniciViewModel(IMuzickiUmetnikRepository umetniciRepo, IClanstvoRepository clanstvoRepo)
        {
            _umetniciRepository = umetniciRepo;
            _clanstvoRepository = clanstvoRepo;

            AddIzvodjacCommand = new RelayCommand(_ => OpenEdit(false));
            AddBendCommand = new RelayCommand(_ => OpenEdit(true));
            EditCommand = new RelayCommand(p =>
            {
                var item = p as MuzickiUmetnik ?? Selected;
                OpenEdit(item is Bend, item?.Id);
            });

            DeleteCommand = new RelayCommand(p =>
            {
                var item = p as MuzickiUmetnik ?? Selected;
                if (item == null) return;
                Selected = item;
                DeleteSelected();
            });

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

        private void OpenEdit(bool isBend, int? id = null)
        {
            var vm = new UmetnikEditViewModel(_umetniciRepository, _clanstvoRepository, isBend, id);

            var v = new Views.UmetnikEditView
            {
                DataContext = vm,
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            vm.OnSaved = () =>
            {
                v.Close();
                Load();
            };
            vm.OnCancelled = () => v.Close();

            v.ShowDialog();
        }

        private void DeleteSelected()
        {
            if (Selected == null) return;
            var md = Selected;
            if (MessageBox.Show("Da li ste sigurni?", "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            _umetniciRepository.Delete(md.Id);
            Load();
        }
    }

}

