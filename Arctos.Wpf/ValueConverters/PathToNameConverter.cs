using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Ursa.Wpf.ValueConverters
{
  public class PathToNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string path))
        return value;

      return Path.GetFileName(path);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}


