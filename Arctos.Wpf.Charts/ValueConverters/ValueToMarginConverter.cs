using System;
using System.Windows;
using System.Windows.Data;

namespace Arctos.Wpf.Charts
{
    public class ValueToMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is not PartOrientation orientation)
                return new Thickness(0, 0, 0, 0);

            var fullRange = Math.Abs((double)values[3] - (double)values[2]);
            var position = Math.Abs((double)values[0] - (double)values[2]);
            var normalized = position / fullRange;

            var offset = normalized * (System.Convert.ToDouble(values[1]) - 1.0);
            switch (orientation)
            {
                case PartOrientation.Vertical:
                    return new Thickness(offset, 0, 0, 0);
                case PartOrientation.Horizontal:
                    return new Thickness(0, offset, 0, 0);
                case PartOrientation.None:
                    return new Thickness(0, 0, 0, 0);
                default:
                    return new Thickness(0, 0, 0, 0);
            }
        }

        public object[]? ConvertBack(object value, System.Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}