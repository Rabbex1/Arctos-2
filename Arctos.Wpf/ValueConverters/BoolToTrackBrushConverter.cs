using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
    public class BoolToTrackBrushConverter : IValueConverter
    {
        public Brush OnBrush { get; set; } = Brushes.Green;
        public Brush OffBrush { get; set; } = Brushes.Gray;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is true ? OnBrush : OffBrush;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }
    }
}
