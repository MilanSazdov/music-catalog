using MusicCatalog.Models;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class AdminKorisniciViewModel : ViewModelBase
    {
        private readonly IKorisnikRepository _repository;
        private RegistrovaniKorisnik? _selectedKorisnik;

        public ObservableCollection<RegistrovaniKorisnik> Korisnici { get; } = new ObservableCollection<RegistrovaniKorisnik>();

        public RegistrovaniKorisnik? SelectedKorisnik
        {
            get => _selectedKorisnik;
            set
            {
                if (_selectedKorisnik == value) return;
                _selectedKorisnik = value;
                OnPropertyChanged(nameof(SelectedKorisnik));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand BlockCommand { get; set; }

        public AdminKorisniciViewModel(IKorisnikRepository repository)
        {
            _repository = repository;

            BlockCommand = new RelayCommand(p => Block());


            LoadKorisnici();
        }

        public void LoadKorisnici()
        {
            Korisnici.Clear();
            var all = _repository.GetAll().Where(k=>k is RegistrovaniKorisnik).Select(k => k as RegistrovaniKorisnik);
            foreach (var k in all) Korisnici.Add(k);
        }

        void Block()
        {
            var k = SelectedKorisnik;
            if (k == null) return;

            k.Blokiran = true;
            _repository.Update(k);
            _repository.SaveChanges();

            LoadKorisnici();
        }
        

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
