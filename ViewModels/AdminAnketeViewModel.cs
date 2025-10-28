using MusicCatalog.Models;
using MusicCatalog.Models.Enums;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using MusicCatalog.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class AdminAnketeViewModel : ViewModelBase
    {
        private readonly AnketaRepository _repository;
        private Anketa? _selectedAnketa;

        public ObservableCollection<Anketa> Ankete { get; } = new ObservableCollection<Anketa>();

        public Anketa? SelectedAnketa
        {
            get => _selectedAnketa;
            set
            {
                if (_selectedAnketa == value) return;
                _selectedAnketa = value;
                OnPropertyChanged(nameof(SelectedAnketa));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddAnketaCommand { get; }
        public ICommand EditAnketaCommand { get; }
        public ICommand DeleteAnketaCommand { get; }

        public AdminAnketeViewModel(AnketaRepository repository)
        {
            _repository = repository;

            AddAnketaCommand = new RelayCommand(_ => OpenEditWindowFor(null));
            EditAnketaCommand = new RelayCommand(p => OpenEditWindowFor((p as Anketa)?.Id ?? SelectedAnketa?.Id),
                                                p => (p is Anketa) || SelectedAnketa != null);
            DeleteAnketaCommand = new RelayCommand(p => DeleteAnketa(p as Anketa ?? SelectedAnketa),
                                                   p => (p is Anketa) || SelectedAnketa != null);

            LoadAnkete();
        }

        public void LoadAnkete()
        {
            Ankete.Clear();
            var all = _repository.GetAll().OrderBy(a => a.Id);
            foreach (var a in all) Ankete.Add(a);
        }

        private void OpenEditWindowFor(int? anketaId)
        {
            var editVm = new AnketaEditViewModel(_repository, anketaId);

            var editView = new AnketaEditView
            {
                DataContext = editVm
            };

            editVm.OnSaved = () =>
            {
                LoadAnkete();
                editView?.Close();
            };

            editVm.OnCancelled = () =>
            {
                editView?.Close();
            };

            editView.ShowDialog();

            LoadAnkete();
        }

        private void DeleteAnketa(Anketa? a)
        {
            if (a == null) return;

            var result = MessageBox.Show($"Da li ste sigurni da želite da obrišete anketu \"{a.Naziv}\"?",
                                         "Potvrda brisanja",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                _repository.Delete(a.Id);
                _repository.SaveChanges();
                LoadAnkete();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri brisanju ankete: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
