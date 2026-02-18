using System.Windows.Controls;

namespace Arctos.Wpf.Inputs
{
    public class StringToIntValidationRule : ValidationBindingRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            var val = value != null && int.TryParse(GetBoundValue(value)?.ToString(), out _) 
                ? new ValidationResult(true, null) 
                : new ValidationResult(false, "Please enter a valid integer.");
            return val;
        }
    }
}
