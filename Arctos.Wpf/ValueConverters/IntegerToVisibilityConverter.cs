using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
    public class IntegerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int val && targetType == typeof(Visibility) && parameter is string param)
            {
                var splitParam = param.Split(':');
                if(!int.TryParse(splitParam[1], out int paramVal))
                    return Visibility.Visible;
                switch (splitParam[0])
                {
                    case "<":
                        return val < paramVal ? Visibility.Visible : Visibility.Collapsed;
                    case "<=":
                        return val <= paramVal ? Visibility.Visible : Visibility.Collapsed;
                    case "==":
                        return val == paramVal ? Visibility.Visible : Visibility.Collapsed;
                    case ">=":
                        return val >= paramVal ? Visibility.Visible : Visibility.Collapsed;
                    case ">":
                        return val > paramVal ? Visibility.Visible : Visibility.Collapsed;
                    default: 
                        return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
