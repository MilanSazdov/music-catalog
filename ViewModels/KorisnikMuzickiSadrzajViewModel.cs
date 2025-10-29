using MusicCatalog.Models.MuzickiSadrzaj;
using MusicCatalog.Repositories;
using MusicCatalog.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows.Input;
using MusicCatalog.Utils;
using System.Windows;

namespace MusicCatalog.ViewModels
{
    public class KorisnikMuzickiSadrzajViewModel : ViewModelBase
    {
        private readonly IMuzickoDeloRepository _delaRepo;
        private readonly IZanrRepository _zanrRepo;

        public ObservableCollection<MuzickoDeloView> Dela { get; } = new();
        private MuzickoDeloView? _selected;
        public MuzickoDeloView? Selected { get => _selected; set { _selected = value; OnPropertyChanged(nameof(Selected)); } }

        public ICommand OceniCommand { get; }

        public KorisnikMuzickiSadrzajViewModel(IMuzickoDeloRepository delaRepo, IZanrRepository zanrRepo)
        {
            _delaRepo = delaRepo;
            _zanrRepo = zanrRepo;

            OceniCommand = new RelayCommand(OceniDelo);

            Load();
        }

        private void Load()
        {
            Dela.Clear();
            foreach (var d in _delaRepo.GetAll().OrderBy(d => d.Id))
            {
                var tip = d is Album ? "Album" : d is Pesma ? "Pesma" : "";
                Dela.Add(new MuzickoDeloView(d, tip));
            }
        }

        private void OceniDelo(object? parameter)
        {
            if (parameter is MuzickoDeloView delo)
            {
                MessageBox.Show($"Ocenjujete delo: {delo.Naziv}", "Ocenjivanje", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}