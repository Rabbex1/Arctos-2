using System.Globalization;
using System.Windows.Data;
using Application = System.Windows.Application;

namespace Arctos.Wpf.ValueConverters
{
  [ValueConversion(typeof(object), typeof(object))]
  public class StringToResourceConverter : IValueConverter
  {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is not string) 
          return null;
      var resource = Application.Current.Resources[value];
      return resource ?? value;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
