namespace Arctos.Forms.MudBlazor;

public static class MudBlazorValidationAdapters
{
    public static IEnumerable<string> ValidateTimeSpan(string? value)
    {
        return InputValidators.ValidateTimeSpan(value).Errors;
    }

    public static IEnumerable<string> ValidateInfinityInteger(string? value)
    {
        return InputValidators.ValidateInfinityInteger(value).Errors;
    }

    public static IEnumerable<string> ValidateIntegerRange(InputIntRangeVM viewModel, string? value)
    {
        return viewModel.ValidateText(value).Errors;
    }

    public static IEnumerable<string> ValidateIntegerRangeExtra(InputIntRangeExtraVM viewModel, string? value)
    {
        return viewModel.ValidateText(value).Errors;
    }

    public static IEnumerable<string> ValidateFloatRange(InputFloatRangeVM viewModel, double value)
    {
        return viewModel.ValidateValue((float)value).Errors;
    }
}
