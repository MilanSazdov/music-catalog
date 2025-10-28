using MusicCatalog.Models.Umetnici;
using MusicCatalog.Repositories;
using MusicCatalog.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MusicCatalog.ViewModels
{
    public class UrednikUmetniciViewModel : ViewModelBase
    {
        private readonly IMuzickiUmetnikRepository _umetnikRepository;

        public ObservableCollection<MuzickiUmetnik> Umetnici { get; private set; }

        public ICommand AddBendCommand { get; }
        public ICommand AddIzvodjacCommand { get; }
        public ICommand EditUmetnikCommand { get; }
        public ICommand DeleteUmetnikCommand { get; }

        public UrednikUmetniciViewModel(IMuzickiUmetnikRepository umetnikRepository)
        {
            _umetnikRepository = umetnikRepository;
            Umetnici = new ObservableCollection<MuzickiUmetnik>();

            AddBendCommand = new RelayCommand(_ => AddUmetnik(typeof(Bend)));
            AddIzvodjacCommand = new RelayCommand(_ => AddUmetnik(typeof(Izvodjac)));
            EditUmetnikCommand = new RelayCommand(EditUmetnik);
            DeleteUmetnikCommand = new RelayCommand(DeleteUmetnik);

            LoadUmetnici();
        }

        private void LoadUmetnici()
        {
            Umetnici.Clear();
            var umetniciLista = _umetnikRepository.GetAll();
            foreach (var umetnik in umetniciLista)
            {
                Umetnici.Add(umetnik);
            }
        }

        private void AddUmetnik(System.Type tip)
        {
            // TODO: Otvoriti novi prozor (npr. UmetnikEditView) za dodavanje
            // Za sada, samo placeholder poruka
            MessageBox.Show($"Logika za dodavanje novog: {tip.Name}. Potrebno je kreirati UmetnikEditView/ViewModel.", "Info");

            // Nakon što se prozor zatvori i umetnik doda:
            // _umetnikRepository.SaveChanges();
            // LoadUmetnici();
        }

        private void EditUmetnik(object? parameter)
        {
            if (parameter is MuzickiUmetnik umetnik)
            {
                // TODO: Otvoriti UmetnikEditView za izmenu postojećeg umetnika
                MessageBox.Show($"Logika za izmenu umetnika: {umetnik.Id}", "Info");

                // Nakon što se prozor zatvori:
                // _umetnikRepository.SaveChanges();
                // LoadUmetnici();
            }
        }

        private void DeleteUmetnik(object? parameter)
        {
            if (parameter is MuzickiUmetnik umetnik)
            {
                if (MessageBox.Show($"Da li ste sigurni da želite da obrišete umetnika (ID: {umetnik.Id})?", "Potvrda brisanja", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _umetnikRepository.Delete(umetnik.Id);
                    _umetnikRepository.SaveChanges();
                    LoadUmetnici(); // Ponovo učitaj listu
                }
            }
        }
    }
}