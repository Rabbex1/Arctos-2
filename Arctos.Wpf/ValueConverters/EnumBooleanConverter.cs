using System.Windows;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
  public class EnumBooleanConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
      if (parameter is not string parameterString || value == null)
        return DependencyProperty.UnsetValue;


      if (Enum.IsDefined(value.GetType(), value) == false)
        return DependencyProperty.UnsetValue;

      var parameterValue = Enum.Parse(value.GetType(), parameterString);

      return parameterValue.Equals(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
      if (parameter is not string parameterString)
        return DependencyProperty.UnsetValue;

      return value != null && (bool)value ? Enum.Parse(targetType, parameterString) : null;
    }
  }
}
