using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
  public class PathToNameConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not string path ? value : Path.GetFileName(path);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      return System.Windows.Data.Binding.DoNothing;
    }
  }
}




