using System.Windows;
using System.Windows.Input;
using MusicCatalog.ViewModels;

namespace MusicCatalog.Views
{
   
    public partial class UmetnikInfoView : Window
    {
        public UmetnikInfoView()
        {
            InitializeComponent();

            
            if (DataContext is UmetnikInfoViewModel vm)
            {
                vm.CloseWindow = this.Close;
            }
            
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is UmetnikInfoViewModel newVm)
                {
                    newVm.CloseWindow = this.Close;
                }
            };
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