using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Arctos.Wpf.MarkupExtensions
{
    public struct ValueDescription
    {
        public Enum Value { get; set; }
        public int IntValue { get; set; }
        public string Description { get; set; }
    }

    public static class EnumHelper
    {
        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");


            return Enum.GetValues(t).Cast<Enum>().Select((e) => new ValueDescription() { Value = e, Description = e.Description(), IntValue = Convert.ToInt32(e) }).ToList();
        }
    }

    [ValueConversion(typeof(Enum), typeof(IEnumerable<ValueDescription>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value != null ? EnumHelper.GetAllValuesAndDescriptions(value.GetType()) : null);
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
