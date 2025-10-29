using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicCatalog.Utils
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible;

            if (value is bool b) visible = b;
            else if (value is bool nb) visible = false;
            else if (value is string s) visible = !string.IsNullOrEmpty(s);
            else if (value == null) visible = false;
            else
            {
                if (!bool.TryParse(value.ToString(), out visible))
                    visible = false;
            }

            if (parameter != null)
            {
                var p = parameter.ToString();
                if (p.Equals("inverse", StringComparison.OrdinalIgnoreCase)
                    || p.Equals("invert", StringComparison.OrdinalIgnoreCase)
                    || p.Equals("negate", StringComparison.OrdinalIgnoreCase))
                {
                    visible = !visible;
                }
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
