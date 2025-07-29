using System.Globalization;
using System.Windows.Data;

namespace Arctos.Wpf.ValueConverters
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan val && targetType == typeof(string))
            {
                var result = "";
                if(val.Days != 0)
                    result = val.Days + "d ";
                result += val.Hours + "h ";
                result += val.Minutes + "m ";
                result += val.Seconds + "s ";
                if (val.Milliseconds != 0)
                    result += val.Milliseconds + "ms";
                return result;
            }
            return "";

        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string val && targetType == typeof(TimeSpan))
            {
                var days = 0;
                var hours = 0;
                var minutes = 0;
                var seconds = 0;
                var milliseconds = 0;

                var components = val.Split(" ");
                foreach (var component in components)
                {
                    if (component.EndsWith("d"))
                        days = int.Parse(component.TrimEnd('d'));
                    if (component.EndsWith("h"))
                        hours = int.Parse(component.TrimEnd('h'));
                    if (component.EndsWith("m"))
                        minutes = int.Parse(component.TrimEnd('m'));
                    if (component.EndsWith("s"))
                        seconds = int.Parse(component.TrimEnd('s'));
                    if (component.EndsWith("ms"))
                        milliseconds = int.Parse(component.TrimEndSubString("ms"));
                }
                return new TimeSpan(days, hours, minutes, seconds, milliseconds);
            }
            throw new ArgumentException("Invalid argument/return type. Expected argument: Visibility and return type: bool");
        }

    }
}
