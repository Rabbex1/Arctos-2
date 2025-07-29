using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Arctos.Wpf.Inputs
{
    public class ValidationBindingRule : ValidationRule
    {
        public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }

        protected object? GetBoundValue(object value)
        {
            if (value is not BindingExpression binding) 
                return value;
            var dataItem = binding.DataItem;
            var propertyName = binding.ParentBinding.Path.Path;

            var propertyValue = dataItem?.GetType()?.GetProperty(propertyName)?.GetValue(dataItem, null);

            return propertyValue;

        }
    }
}
