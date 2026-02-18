using System.Windows.Controls;

namespace Arctos.Wpf.Inputs
{
    public class StringToFloatValidationRule : ValidationBindingRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            return value != null && double.TryParse(GetBoundValue(value)?.ToString(), out _)
                ? new ValidationResult(true, null)
                : new ValidationResult(false, "Please enter a valid single precision number.");
        }
    }
}
