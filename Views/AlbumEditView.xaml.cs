using System.Windows;
using System.Windows.Input;

namespace MusicCatalog.Views
{
    public partial class AlbumEditView : Window
    {
        public AlbumEditView()
        {
            InitializeComponent();
        }

       
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        
    }
}