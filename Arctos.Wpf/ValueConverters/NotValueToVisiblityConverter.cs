using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
    public class NotValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            var vis =  value != null && !value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
            return vis;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return System.Windows.Data.Binding.DoNothing;
        }

    }
}

