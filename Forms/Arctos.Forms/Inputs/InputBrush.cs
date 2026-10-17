using System.Globalization;
using System.Text;

namespace Arctos.Forms;

[Flags]
public enum InputBrushKinds
{
    None = 0,
    Named = 1,
    Solid = 2,
    Linear = 4,
    Radial = 8,
    Angular = 16,
    All = Named | Solid | Linear | Radial | Angular
}

public enum InputBrushKind
{
    Named,
    Solid,
    Linear,
    Radial,
    Angular
}

public readonly record struct InputGradientStop(double Offset, InputColor Color);

public abstract record InputBrushDescription(InputBrushKind Kind);

public sealed record InputNamedBrushDescription(string Name)
    : InputBrushDescription(InputBrushKind.Named);

public sealed record InputSolidBrushDescription(InputColor Color)
    : InputBrushDescription(InputBrushKind.Solid);

public abstract record InputGradientBrushDescription(
    InputBrushKind Kind,
    IReadOnlyList<InputGradientStop> Stops) : InputBrushDescription(Kind);

public sealed record InputLinearGradientBrushDescription(
    double X1,
    double Y1,
    double X2,
    double Y2,
    IReadOnlyList<InputGradientStop> Stops)
    : InputGradientBrushDescription(InputBrushKind.Linear, Stops);

public sealed record InputRadialGradientBrushDescription(
    double CenterX,
    double CenterY,
    double RadiusX,
    double RadiusY,
    IReadOnlyList<InputGradientStop> Stops)
    : InputGradientBrushDescription(InputBrushKind.Radial, Stops);

public sealed record InputAngularGradientBrushDescription(
    double CenterX,
    double CenterY,
    double StartAngle,
    double EndAngle,
    IReadOnlyList<InputGradientStop> Stops)
    : InputGradientBrushDescription(InputBrushKind.Angular, Stops);

public sealed record InputBrushPaletteEntry(string Name, string Description);

/// <summary>
/// Parses and writes the culture-independent Arctos brush interchange format.
/// Colors are always #RRGGBBAA and angles are clockwise from 12 o'clock.
/// </summary>
public static class InputBrushFormat
{
    private const NumberStyles NumberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static bool TryParse(string? text, out InputBrushDescription? brush, out string error)
    {
        brush = null;
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            error = "A brush value is required.";
            return false;
        }

        var value = text.Trim();
        if (value[0] == '@')
            return TryParseName(value[1..], out brush, out error);

        if (value[0] == '#')
        {
            if (!TryParseColor(value, out var color, out error))
                return false;

            brush = new InputSolidBrushDescription(color);
            return true;
        }

        var open = value.IndexOf('(');
        if (open <= 0 || value[^1] != ')')
        {
            error = "Expected a named brush, #RRGGBBAA, or a supported gradient.";
            return false;
        }

        var keyword = value[..open];
        var sections = value[(open + 1)..^1].Split(';');
        if (sections.Length < 3)
        {
            error = "A gradient requires geometry and at least two color stops.";
            return false;
        }

        if (!TryParseNumbers(sections[0], out var geometry, out error))
            return false;
        if (!TryParseStops(sections.AsSpan(1), out var stops, out error))
            return false;

        switch (keyword)
        {
            case "linear" when geometry.Length == 4:
                if (!AllNormalized(geometry) || (geometry[0] == geometry[2] && geometry[1] == geometry[3]))
                {
                    error = "Linear coordinates must be in 0..1 and its endpoints must differ.";
                    return false;
                }
                brush = new InputLinearGradientBrushDescription(geometry[0], geometry[1], geometry[2], geometry[3], stops);
                return true;
            case "radial" when geometry.Length == 4:
                if (geometry[2] <= 0 || geometry[3] <= 0)
                {
                    error = "Radial center coordinates must be finite and radii must be finite and greater than zero.";
                    return false;
                }
                brush = new InputRadialGradientBrushDescription(geometry[0], geometry[1], geometry[2], geometry[3], stops);
                return true;
            case "angular" when geometry.Length == 4:
                if (!IsNormalized(geometry[0]) || !IsNormalized(geometry[1]) || geometry[3] <= geometry[2] || geometry[3] - geometry[2] > 360)
                {
                    error = "Angular center must be in 0..1 and the clockwise sweep must be greater than 0 and at most 360 degrees.";
                    return false;
                }
                brush = new InputAngularGradientBrushDescription(geometry[0], geometry[1], geometry[2], geometry[3], stops);
                return true;
            case "linear":
            case "radial":
            case "angular":
                error = $"The {keyword} gradient requires exactly four geometry numbers.";
                return false;
            default:
                error = $"Unknown brush kind '{keyword}'.";
                return false;
        }
    }

    public static InputBrushDescription Parse(string text)
    {
        if (TryParse(text, out var brush, out var error))
            return brush!;

        throw new FormatException(error);
    }

    public static string Format(InputBrushDescription brush)
    {
        ArgumentNullException.ThrowIfNull(brush);
        return brush switch
        {
            InputNamedBrushDescription named => "@" + EncodeName(named.Name),
            InputSolidBrushDescription solid => FormatColor(solid.Color),
            InputLinearGradientBrushDescription linear => FormatGradient(
                "linear", [linear.X1, linear.Y1, linear.X2, linear.Y2], linear.Stops),
            InputRadialGradientBrushDescription radial => FormatGradient(
                "radial", [radial.CenterX, radial.CenterY, radial.RadiusX, radial.RadiusY], radial.Stops),
            InputAngularGradientBrushDescription angular => FormatGradient(
                "angular", [angular.CenterX, angular.CenterY, angular.StartAngle, angular.EndAngle], angular.Stops),
            _ => throw new ArgumentOutOfRangeException(nameof(brush))
        };
    }

    public static string Canonicalize(string text) => Format(Parse(text));

    public static string FormatColor(InputColor color) =>
        $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}{color.Alpha:X2}";

    public static bool TryParseColor(string? text, out InputColor color, out string error)
    {
        color = default;
        error = string.Empty;
        if (text is null || text.Length != 9 || text[0] != '#')
        {
            error = "Colors must use #RRGGBBAA notation.";
            return false;
        }

        if (!byte.TryParse(text.AsSpan(1, 2), NumberStyles.HexNumber, Invariant, out var red)
            || !byte.TryParse(text.AsSpan(3, 2), NumberStyles.HexNumber, Invariant, out var green)
            || !byte.TryParse(text.AsSpan(5, 2), NumberStyles.HexNumber, Invariant, out var blue)
            || !byte.TryParse(text.AsSpan(7, 2), NumberStyles.HexNumber, Invariant, out var alpha))
        {
            error = "Colors must contain eight hexadecimal digits in RRGGBBAA order.";
            return false;
        }

        color = new InputColor(red, green, blue, alpha);
        return true;
    }

    public static InputBrushKinds ToFlag(InputBrushKind kind) => kind switch
    {
        InputBrushKind.Named => InputBrushKinds.Named,
        InputBrushKind.Solid => InputBrushKinds.Solid,
        InputBrushKind.Linear => InputBrushKinds.Linear,
        InputBrushKind.Radial => InputBrushKinds.Radial,
        InputBrushKind.Angular => InputBrushKinds.Angular,
        _ => InputBrushKinds.None
    };

    private static bool TryParseName(string encoded, out InputBrushDescription? brush, out string error)
    {
        brush = null;
        error = string.Empty;
        if (encoded.Length == 0)
        {
            error = "A named brush reference cannot be empty.";
            return false;
        }

        for (var index = 0; index < encoded.Length; index++)
        {
            if (encoded[index] == '%')
            {
                if (index + 2 >= encoded.Length || !IsHex(encoded[index + 1]) || !IsHex(encoded[index + 2]))
                {
                    error = "The named brush contains invalid percent escaping.";
                    return false;
                }
                index += 2;
                continue;
            }

            if (!IsUnreserved(encoded[index]))
            {
                error = "Named brush characters outside A-Z, a-z, 0-9, '-', '.', '_' and '~' must be percent-encoded.";
                return false;
            }
        }

        try
        {
            var name = Uri.UnescapeDataString(encoded);
            if (string.IsNullOrWhiteSpace(name) || name.Contains('\0'))
            {
                error = "A named brush reference cannot be empty or contain a null character.";
                return false;
            }

            brush = new InputNamedBrushDescription(name);
            return true;
        }
        catch (UriFormatException)
        {
            error = "The named brush contains invalid percent escaping.";
            return false;
        }
    }

    private static string EncodeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\0'))
            throw new FormatException("A named brush reference cannot be empty or contain a null character.");

        return Uri.EscapeDataString(name);
    }

    private static bool TryParseNumbers(string text, out double[] numbers, out string error)
    {
        error = string.Empty;
        var parts = text.Split(',');
        numbers = new double[parts.Length];
        for (var index = 0; index < parts.Length; index++)
        {
            if (!double.TryParse(parts[index], NumberStyle, Invariant, out numbers[index]) || !double.IsFinite(numbers[index]))
            {
                error = $"'{parts[index]}' is not a finite invariant-culture number.";
                return false;
            }
        }
        return true;
    }

    private static bool TryParseStops(ReadOnlySpan<string> sections, out IReadOnlyList<InputGradientStop> stops, out string error)
    {
        var result = new List<InputGradientStop>(sections.Length);
        error = string.Empty;
        var previous = -1d;
        foreach (var section in sections)
        {
            var separator = section.IndexOf(':');
            if (separator <= 0
                || !double.TryParse(section.AsSpan(0, separator), NumberStyle, Invariant, out var offset)
                || !double.IsFinite(offset)
                || !IsNormalized(offset))
            {
                stops = [];
                error = "Each gradient stop must use offset:#RRGGBBAA with an offset in 0..1.";
                return false;
            }
            if (offset < previous)
            {
                stops = [];
                error = "Gradient stops must be ordered by non-decreasing offset.";
                return false;
            }
            if (!TryParseColor(section[(separator + 1)..], out var color, out error))
            {
                stops = [];
                return false;
            }
            result.Add(new InputGradientStop(offset, color));
            previous = offset;
        }
        if (result.Count < 2)
        {
            stops = [];
            error = "A gradient requires at least two color stops.";
            return false;
        }
        stops = result;
        return true;
    }

    private static string FormatGradient(string keyword, IReadOnlyList<double> geometry, IReadOnlyList<InputGradientStop> stops)
    {
        var candidate = keyword + "(" + string.Join(',', geometry.Select(FormatNumber)) + ";"
            + string.Join(';', stops.Select(stop => $"{FormatNumber(stop.Offset)}:{FormatColor(stop.Color)}")) + ")";
        if (!TryParse(candidate, out _, out var error))
            throw new FormatException(error);
        return candidate;
    }

    private static string FormatNumber(double value)
    {
        if (!double.IsFinite(value))
            throw new FormatException("Brush numbers must be finite.");
        var rounded = Math.Round(value, 6, MidpointRounding.AwayFromZero);
        if (rounded == 0)
            rounded = 0;
        return rounded.ToString("0.######", Invariant);
    }

    private static bool AllNormalized(IEnumerable<double> values) => values.All(IsNormalized);
    private static bool IsNormalized(double value) => value is >= 0 and <= 1;
    private static bool IsHex(char value) => value is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';
    private static bool IsUnreserved(char value) => char.IsAsciiLetterOrDigit(value) || value is '-' or '.' or '_' or '~';
}
