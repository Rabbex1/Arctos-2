using System.Windows.Controls;
using System.Windows.Data;

namespace Arctos.Wpf.Inputs
{
    public class StringToIntRangeExtraValidationRule : ValidationBindingRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value is not BindingExpression binding || binding.DataItem is not InputIntRangeExtraVM viewModel)
                return new ValidationResult(false, "Unable to validate the integer range input.");

            var text = GetBoundValue(value)?.ToString();
            return viewModel.IsValidText(text, out var error)
                ? ValidationResult.ValidResult
                : new ValidationResult(false, error);
        }
    }
}
