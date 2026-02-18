using System.Windows.Controls;

namespace Arctos.Wpf.Inputs
{
    internal class StringToInfinityIntValidationRule : ValidationBindingRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            bool isValid;
            if (value != null)
            {
                var stringValue = GetBoundValue(value)?.ToString();
                isValid = stringValue switch
                {
                    "i" or "+i" or "-i" or "+\u221E" or "-\u221E" => true,
                    _ => long.TryParse(stringValue, out var v)
                };
            }
            else 
                isValid = false;


            var val = isValid
                ? new ValidationResult(true, null)
                : new ValidationResult(false, "Please enter '[+]i', '-i' or a valid long integer.");
            return val;
        }
    }
}
