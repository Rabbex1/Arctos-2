using System.Windows.Controls;

namespace Arctos.Wpf.Inputs
{
    internal class StringToTimeSpanValidationRule : ValidationBindingRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            return value != null && TimeSpan.TryParse(GetBoundValue(value)?.ToString(), out _)
                ? new ValidationResult(true, null)
                : new ValidationResult(false, "Please enter a valid timespan (hh:mm:ss).");
        }
    }
}
