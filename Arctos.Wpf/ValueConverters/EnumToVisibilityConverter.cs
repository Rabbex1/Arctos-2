using System.DirectoryServices.ActiveDirectory;
using System.Windows;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
  public class EnumToVisibilityConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
      if (parameter is not string parameterString || value == null)
        return DependencyProperty.UnsetValue;


      if (Enum.IsDefined(value.GetType(), value) == false)
        return DependencyProperty.UnsetValue;

      string[] validEnumList = {""};

      if (parameterString.Contains('|'))
          validEnumList = parameterString.Split('|');
      else
          validEnumList[0] = parameterString;

      foreach (var validEnum in validEnumList)
      {
          if (!Enum.TryParse(value.GetType(), validEnum, true, out var paramEnum)) 
              continue;
          if (paramEnum.Equals(value))
              return Visibility.Visible;
      }
      return Visibility.Collapsed;;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return System.Windows.Data.Binding.DoNothing;
    }
  }
}


