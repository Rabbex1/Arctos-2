using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
    public class DatePickerConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is DateTime dt && parameter != null)
            {
                return dt.ToString(parameter.ToString());
            }
            return "error";

        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string dt && parameter is string p)
            {
                return DateTime.Parse(dt);
            }
            throw new ArgumentException("Invalid argument/return type. Expected argument: Visibility and return type: bool");
        }

    }
}
