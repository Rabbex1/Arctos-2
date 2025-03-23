using System.Globalization;
using System.Windows.Data;

namespace Ursa.Wpf.ValueConverters
{
  public class PathToShortPathConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string path))
        return value;

      var levels = path.Split('\\');
      if (levels.Length <= 3)
        return path;
      var result = levels[0] + '\\' + levels[1] + '\\';
      for (var i = 2; i < levels.Length - 2; i++)
        result += @"...\";
      return result + levels.Last();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}


