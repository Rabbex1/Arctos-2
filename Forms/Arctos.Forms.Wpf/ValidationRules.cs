using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Arctos.Forms.Wpf;

public class ValidationBindingRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return ValidationResult.ValidResult;
    }

    protected object? GetBoundValue(object value)
    {
        if (value is not BindingExpression binding)
            return value;

        var dataItem = binding.DataItem;
        var propertyName = binding.ParentBinding.Path.Path;
        return dataItem?.GetType()?.GetProperty(propertyName)?.GetValue(dataItem, null);
    }

    protected static ValidationResult ToValidationResult(InputValidationResult result)
    {
        return result.IsValid
            ? ValidationResult.ValidResult
            : new ValidationResult(false, result.FirstError ?? "Invalid.");
    }
}

public sealed class StringToIntValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return ToValidationResult(InputValidators.ValidateInteger(value == null ? null : GetBoundValue(value)?.ToString(), cultureInfo));
    }
}

public sealed class StringToUIntValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return ToValidationResult(InputValidators.ValidateUnsignedInteger(value == null ? null : GetBoundValue(value)?.ToString(), cultureInfo));
    }
}

public sealed class StringToDoubleValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return ToValidationResult(InputValidators.ValidateDouble(value == null ? null : GetBoundValue(value)?.ToString(), cultureInfo));
    }
}

public sealed class StringToFloatValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return ToValidationResult(InputValidators.ValidateFloat(value == null ? null : GetBoundValue(value)?.ToString(), cultureInfo));
    }
}

public sealed class StringToInfinityIntValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return ToValidationResult(InputValidators.ValidateInfinityInteger(value == null ? null : GetBoundValue(value)?.ToString(), cultureInfo));
    }
}

public sealed class StringToTimeSpanValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return ToValidationResult(InputValidators.ValidateTimeSpan(value == null ? null : GetBoundValue(value)?.ToString(), cultureInfo));
    }
}

public sealed class StringToIntRangeValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not BindingExpression binding || binding.DataItem is not InputIntRangeVM viewModel || viewModel.Value == null)
            return new ValidationResult(false, "Unable to validate the integer range.");

        return ToValidationResult(viewModel.ValidateText(GetBoundValue(value)?.ToString()));
    }
}

public sealed class StringToFloatRangeValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not BindingExpression binding || binding.DataItem is not InputFloatRangeVM viewModel)
            return new ValidationResult(false, "Unable to validate the float range.");

        return ToValidationResult(viewModel.ValidateText(GetBoundValue(value)?.ToString()));
    }
}

public sealed class StringToIntRangeExtraValidationRule : ValidationBindingRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not BindingExpression binding || binding.DataItem is not InputIntRangeExtraVM viewModel)
            return new ValidationResult(false, "Unable to validate the integer range input.");

        return ToValidationResult(viewModel.ValidateText(GetBoundValue(value)?.ToString()));
    }
}
