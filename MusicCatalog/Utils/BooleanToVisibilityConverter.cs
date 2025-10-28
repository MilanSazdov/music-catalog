using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicCatalog.Utils // <-- 100% sam siguran da ti ovde piše nešto drugo
{
    // Ovaj konverter pretvara 'bool' (true/false) u 'Visibility' (Visible/Collapsed)
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasError = false;
            if (value is string str)
            {
                hasError = !string.IsNullOrEmpty(str);
            }
            else if (value is bool b)
            {
                hasError = b;
            }

            return hasError ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}