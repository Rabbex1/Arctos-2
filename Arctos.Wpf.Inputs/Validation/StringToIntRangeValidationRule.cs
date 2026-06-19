using System.Windows.Controls;
using System.Windows.Data;

namespace Arctos.Wpf.Inputs
{
    public class StringToIntRangeValidationRule : ValidationBindingRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value is not BindingExpression binding || binding.DataItem is not InputIntRangeVM viewModel)
                return new ValidationResult(false, "Unable to validate the integer range.");

            if (viewModel.Value == null)
                return new ValidationResult(false, "Unable to validate the integer range.");

            var text = GetBoundValue(value)?.ToString();
            if (!int.TryParse(text, out var parsed))
                return new ValidationResult(false, "Please enter a valid integer.");

            return parsed < viewModel.Value.Min || parsed > viewModel.Value.Max
                ? new ValidationResult(false, $"Please enter a value between {viewModel.Value.Min} and {viewModel.Value.Max}.")
                : ValidationResult.ValidResult;
        }
    }
}
