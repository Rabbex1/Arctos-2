using System.Windows;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class NotBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool val && targetType == typeof(Visibility))
            {
                if (!val)
                    return Visibility.Visible;
                return parameter is Visibility ? parameter : Visibility.Collapsed;
            }
            if (value != null)
                return Visibility.Visible;
            return parameter is Visibility ? parameter : Visibility.Collapsed;

        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility val && targetType == typeof(bool))
            {
                return val != Visibility.Visible;
            }
            throw new ArgumentException("Invalid argument/return type. Expected argument: Visibility and return type: bool");
        }

    }
}
