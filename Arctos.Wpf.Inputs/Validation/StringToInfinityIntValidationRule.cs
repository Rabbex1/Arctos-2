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
                var stringValue = GetBoundValue(value).ToString();
                switch (stringValue)
                {
                    case "i":
                    case "+i":
                    case "-i":
                    case "+\u221E":
                    case "-\u221E":
                        isValid = true;
                        break;
                    default:
                        isValid = long.TryParse(stringValue, out var v);
                        break;
                }
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
