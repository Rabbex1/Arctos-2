using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
    public class ChangedDocTitleConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is string title && values[1] is bool isChanged)
                return isChanged ? title + "*" : title;
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return targetType.Select(_ => System.Windows.Data.Binding.DoNothing).ToArray();
        }

    }
}

