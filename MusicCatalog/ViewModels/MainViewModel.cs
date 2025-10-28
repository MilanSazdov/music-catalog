
namespace MusicCatalog.ViewModels
{


    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _trenutniView;

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