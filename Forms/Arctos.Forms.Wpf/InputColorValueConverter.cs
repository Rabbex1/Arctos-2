using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Arctos.Forms;

namespace Arctos.Forms.Wpf;

/// <summary>
/// Converts portable form colors for the WPF color picker.
/// </summary>
public sealed class InputColorValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is InputColor color
            ? Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue)
            : Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Color color
            ? new InputColor(color.R, color.G, color.B, color.A)
            : DependencyProperty.UnsetValue;
    }
}
