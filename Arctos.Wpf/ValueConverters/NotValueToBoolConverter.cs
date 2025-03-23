using System.Globalization;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
  public class NotValueToBoolConverter : IValueConverter
  {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var val = !value?.Equals(parameter);
      return val;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
