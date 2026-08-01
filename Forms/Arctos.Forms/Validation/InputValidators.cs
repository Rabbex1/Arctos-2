using System.Globalization;

namespace Arctos.Forms;

public static class InputValidators
{
    public static InputValidationResult ValidateInteger(string? value, CultureInfo? culture = null)
    {
        return int.TryParse(Normalize(value), NumberStyles.Integer, culture ?? CultureInfo.CurrentCulture, out _)
            ? InputValidationResult.Success
            : InputValidationResult.Invalid("Please enter a valid integer.");
    }

    public static InputValidationResult ValidateUnsignedInteger(string? value, CultureInfo? culture = null)
    {
        return uint.TryParse(Normalize(value), NumberStyles.Integer, culture ?? CultureInfo.CurrentCulture, out _)
            ? InputValidationResult.Success
            : InputValidationResult.Invalid("Please enter a valid integer.");
    }

    public static InputValidationResult ValidateDouble(string? value, CultureInfo? culture = null)
    {
        return double.TryParse(Normalize(value), NumberStyles.Float | NumberStyles.AllowThousands, culture ?? CultureInfo.CurrentCulture, out _)
            ? InputValidationResult.Success
            : InputValidationResult.Invalid("Please enter a valid double precision number.");
    }

    public static InputValidationResult ValidateFloat(string? value, CultureInfo? culture = null)
    {
        return float.TryParse(Normalize(value), NumberStyles.Float | NumberStyles.AllowThousands, culture ?? CultureInfo.CurrentCulture, out _)
            ? InputValidationResult.Success
            : InputValidationResult.Invalid("Please enter a valid single precision number.");
    }

    public static InputValidationResult ValidateFloatRange(float value, float min, float max)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return InputValidationResult.Invalid("Please enter a valid single precision number.");

        if (float.IsNaN(min) || float.IsInfinity(min) || float.IsNaN(max) || float.IsInfinity(max))
            return InputValidationResult.Invalid("The float range bounds must be finite values.");

        var normalizedMin = Math.Min(min, max);
        var normalizedMax = Math.Max(min, max);

        return value < normalizedMin || value > normalizedMax
            ? InputValidationResult.Invalid($"Please enter a value between {normalizedMin} and {normalizedMax}.")
            : InputValidationResult.Success;
    }

    public static InputValidationResult ValidateFloatRange(string? value, float min, float max, CultureInfo? culture = null)
    {
        var normalized = Normalize(value);
        if (!float.TryParse(normalized, NumberStyles.Float | NumberStyles.AllowThousands, culture ?? CultureInfo.CurrentCulture, out var parsedValue))
            return InputValidationResult.Invalid("Please enter a valid single precision number.");

        return ValidateFloatRange(parsedValue, min, max);
    }

    public static InputValidationResult ValidateFloatRange(FloatRangeInput? range)
    {
        if (range == null)
            return InputValidationResult.Invalid("Unable to validate the float range.");

        return ValidateFloatRange(range.Value, range.Min, range.Max);
    }

    public static InputValidationResult ValidateInfinityInteger(string? value, CultureInfo? culture = null)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return InputValidationResult.Invalid("Please enter '[+]i', '-i' or a valid long integer.");

        var isValid = normalized switch
        {
            "i" or "+i" or "-i" or "+\u221E" or "-\u221E" => true,
            _ => long.TryParse(normalized, NumberStyles.Integer, culture ?? CultureInfo.CurrentCulture, out _)
        };

        return isValid
            ? InputValidationResult.Success
            : InputValidationResult.Invalid("Please enter '[+]i', '-i' or a valid long integer.");
    }

    public static InputValidationResult ValidateTimeSpan(string? value, CultureInfo? culture = null)
    {
        return TimeSpan.TryParse(Normalize(value), culture ?? CultureInfo.CurrentCulture, out _)
            ? InputValidationResult.Success
            : InputValidationResult.Invalid("Please enter a valid timespan (hh:mm:ss).");
    }

    public static InputValidationResult ValidateIntegerRange(string? value, int min, int max, CultureInfo? culture = null)
    {
        var normalized = Normalize(value);
        if (!int.TryParse(normalized, NumberStyles.Integer, culture ?? CultureInfo.CurrentCulture, out var parsedValue))
            return InputValidationResult.Invalid("Please enter a valid integer.");

        var normalizedMin = Math.Min(min, max);
        var normalizedMax = Math.Max(min, max);

        return parsedValue < normalizedMin || parsedValue > normalizedMax
            ? InputValidationResult.Invalid($"Please enter a value between {normalizedMin} and {normalizedMax}.")
            : InputValidationResult.Success;
    }

    public static InputValidationResult ValidateIntegerRangeExtra(string? value, int min, int max, IEnumerable<(char Key, string Text)> specialValues)
    {
        return TryGetIntegerRangeExtraCommittedValue(value, min, max, specialValues, out _)
            ? InputValidationResult.Success
            : InputValidationResult.Invalid(GetIntegerRangeExtraDescription(min, max, specialValues));
    }

    public static string GetIntegerRangeExtraDescription(int min, int max, IEnumerable<(char Key, string Text)> specialValues)
    {
        var specialValueMap = BuildSpecialValueMap(specialValues);
        var normalizedMin = Math.Min(min, max);
        var normalizedMax = Math.Max(min, max);
        var specials = specialValueMap.Count == 0
            ? string.Empty
            : $" or {string.Join(", ", specialValueMap.Select(pair => $"'{pair.Key}' = {pair.Value}"))}";

        return $"Enter a value between {normalizedMin} and {normalizedMax}{specials}.";
    }

    public static string NormalizeIntegerRangeExtraInput(string? value, IEnumerable<(char Key, string Text)> specialValues)
    {
        var specialValueMap = BuildSpecialValueMap(specialValues);
        return NormalizeIntegerRangeExtraInput(value, specialValueMap);
    }

    public static string NormalizeIntegerRangeExtraInitialValue(string? value, int min, int max, IEnumerable<(char Key, string Text)> specialValues)
    {
        return TryGetIntegerRangeExtraCommittedValue(value, min, max, specialValues, out var committedValue)
            ? committedValue
            : Math.Min(min, max).ToString(CultureInfo.InvariantCulture);
    }

    public static bool TryGetIntegerRangeExtraCommittedValue(string? value, int min, int max, IEnumerable<(char Key, string Text)> specialValues, out string committedValue)
    {
        var specialValueMap = BuildSpecialValueMap(specialValues);
        var normalized = NormalizeIntegerRangeExtraInput(value, specialValueMap);

        if (normalized.Length == 1)
        {
            var specialKey = char.ToLowerInvariant(normalized[0]);
            if (specialValueMap.ContainsKey(specialKey))
            {
                committedValue = specialKey.ToString();
                return true;
            }
        }

        var normalizedMin = Math.Min(min, max);
        var normalizedMax = Math.Max(min, max);

        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
            && parsedValue >= normalizedMin
            && parsedValue <= normalizedMax)
        {
            committedValue = parsedValue.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        committedValue = string.Empty;
        return false;
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string NormalizeIntegerRangeExtraInput(string? value, IReadOnlyDictionary<char, string> specialValueMap)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrEmpty(normalized))
            return string.Empty;

        var lastChar = char.ToLowerInvariant(normalized[^1]);
        if (specialValueMap.ContainsKey(lastChar))
            return lastChar.ToString();

        if (normalized.Length > 1)
        {
            var firstChar = char.ToLowerInvariant(normalized[0]);
            if (specialValueMap.ContainsKey(firstChar))
                normalized = normalized[1..].TrimStart();
        }

        return normalized;
    }

    private static Dictionary<char, string> BuildSpecialValueMap(IEnumerable<(char Key, string Text)> specialValues)
    {
        var result = new Dictionary<char, string>();

        foreach (var (key, text) in specialValues)
        {
            if (char.IsWhiteSpace(key) || text == null)
                continue;

            result[char.ToLowerInvariant(key)] = text;
        }

        return result;
    }
}
