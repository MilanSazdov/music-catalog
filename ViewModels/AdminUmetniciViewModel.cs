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
        private readonly IMuzickoDeloRepository _deloRepository;

        // === Koristimo UmetnikView ===
        public ObservableCollection<UmetnikView> Umetnici { get; } = new();
        private UmetnikView? _selected;
        public UmetnikView? Selected
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
        public ICommand ShowInfoCommand { get; }

        public AdminUmetniciViewModel(IMuzickiUmetnikRepository umetniciRepo, IClanstvoRepository clanstvoRepo, IMuzickoDeloRepository deloRepo)
        {
            _umetniciRepository = umetniciRepo;
            _clanstvoRepository = clanstvoRepo;
            _deloRepository = deloRepo;

            AddIzvodjacCommand = new RelayCommand(_ => OpenEdit(false));
            AddBendCommand = new RelayCommand(_ => OpenEdit(true));

            EditCommand = new RelayCommand(p =>
            {
                var item = p as UmetnikView ?? Selected;
                if (item == null) return;
                OpenEdit(item.IsBend, item.Id);
            }, p => (p as UmetnikView) != null || Selected != null);

            DeleteCommand = new RelayCommand(p =>
            {
                var item = p as UmetnikView ?? Selected;
                if (item == null) return;
                DeleteSelected(item);
            }, p => (p as UmetnikView) != null || Selected != null);

            ShowInfoCommand = new RelayCommand(ShowInfo);

            Load();
        }

        private void Load()
        {
            Umetnici.Clear();
            foreach (var d in _umetniciRepository.GetAll().OrderBy(d => d is Bend))
            {
                Umetnici.Add(new UmetnikView(d));
            }
        }

        private void OpenEdit(bool isBend, int? id = null)
        {
            var vm = new UmetnikEditViewModel(_umetniciRepository, _clanstvoRepository, _deloRepository, isBend, id);
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

        private void DeleteSelected(UmetnikView? item)
        {
            if (item == null) return;
            if (MessageBox.Show($"Da li ste sigurni da želite da obrišete umetnika?", "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            _umetniciRepository.Delete(item.Id);
            _umetniciRepository.SaveChanges();
            Load();
        }

        private void ShowInfo(object? parameter)
        {
            if (parameter is UmetnikView umetnikView)
            {
                var vm = new UmetnikInfoViewModel(umetnikView.Umetnik, _umetniciRepository, _clanstvoRepository, _deloRepository);

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