// Nema 'using' izjava, ali pazi na namespace
namespace MusicCatalog.ViewModels
{
    // Greška je verovatno bila jer ti je ovde namespace bio pogrešan.
    // Obe klase, MainViewModel i ViewModelBase, moraju biti u 'MusicCatalog.ViewModels'

    public class MainViewModel : ViewModelBase // <-- Ova linija sada radi
    {
        private ViewModelBase _trenutniView; // <-- I ova linija sada radi

        public ViewModelBase TrenutniView
        {
            get => _trenutniView;
            set => SetField(ref _trenutniView, value);
        }

        public MainViewModel(ViewModelBase pocetniView)
        {
            _trenutniView = pocetniView;
        }
    }
}