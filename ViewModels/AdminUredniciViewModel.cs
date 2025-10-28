using MusicCatalog.Models;
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
    public class AdminUredniciViewModel : ViewModelBase
    {
        private readonly IKorisnikRepository _korisnikRepository;
        private readonly IZanrRepository _zanrRepository;
        private MuzickiUrednik? _selectedUrednik;

        public ICommand AddUrednikCommand { get; }
        public ICommand EditUrednikCommand { get; }
        public ICommand DeleteUrednikCommand { get; }

        public ObservableCollection<MuzickiUrednik> Urednici { get; } = new ObservableCollection<MuzickiUrednik>();

        public MuzickiUrednik? SelectedUrednik
        {
            get => _selectedUrednik;
            set
            {
                if (_selectedUrednik == value) return;
                _selectedUrednik = value;
                OnPropertyChanged(nameof(SelectedUrednik));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public AdminUredniciViewModel(IKorisnikRepository korisnikRepository, IZanrRepository zanrRepository)
        {
            _korisnikRepository = korisnikRepository;
            _zanrRepository = zanrRepository;

            AddUrednikCommand = new RelayCommand(_ => OpenEditWindowFor(null));
            EditUrednikCommand = new RelayCommand(p => OpenEditWindowFor(p as MuzickiUrednik));
            DeleteUrednikCommand = new RelayCommand(p => DeleteUrednik(p as MuzickiUrednik));

            LoadUrednici();
        }

        public void LoadUrednici()
        {
            Urednici.Clear();
            var all = _korisnikRepository.GetAll().Where(k => k is MuzickiUrednik).Select(k => k as MuzickiUrednik);
            foreach (var k in all) Urednici.Add(k);
        }


        private void OpenEditWindowFor(MuzickiUrednik urednik)
        {
            var editVm = new UrednikEditViewModel(_zanrRepository, _korisnikRepository, urednik);

            var editView = new UrednikEditView
            {
                DataContext = editVm
            };

            editVm.OnSaved = () =>
            {
                LoadUrednici();
                editView?.Close();
            };

            editVm.OnCancelled = () =>
            {
                editView?.Close();
            };

            editView.ShowDialog();

            LoadUrednici();
        }

        private void DeleteUrednik(MuzickiUrednik? urednik)
        {
            _korisnikRepository.Delete(urednik.Email);
            LoadUrednici();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
