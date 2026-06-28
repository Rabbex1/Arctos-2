using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Rendering;

internal static class SvgIconRenderer
{
    public static bool Render(SKCanvas canvas, string? svg, SKRect bounds, bool useNodeColors, SKColor nodeColor, float opacity = 1.0f)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        if (!SvgIconDocument.TryParse(svg, out var document) || document == null || document.Shapes.Count == 0 || bounds.Width <= 0.0f || bounds.Height <= 0.0f)
            return false;

        var contentBounds = document.ContentBounds;
        var scale = MathF.Min(bounds.Width / contentBounds.Width, bounds.Height / contentBounds.Height);
        if (scale <= 0.0f || float.IsNaN(scale) || float.IsInfinity(scale))
            return false;

        var translateX = bounds.Left + ((bounds.Width - (contentBounds.Width * scale)) * 0.5f) - (contentBounds.Left * scale);
        var translateY = bounds.Top + ((bounds.Height - (contentBounds.Height * scale)) * 0.5f) - (contentBounds.Top * scale);

        canvas.Save();
        canvas.Translate(translateX, translateY);
        canvas.Scale(scale, scale);

        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var strokePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
        };

        foreach (var shape in document.Shapes)
        {
            if (useNodeColors && shape.IsViewBoxBackground(document.ViewBox))
                continue;

            if (shape.FillEnabled)
            {
                fillPaint.Color = ApplyOpacity(useNodeColors ? nodeColor : shape.FillColor, opacity);
                canvas.DrawPath(shape.Path, fillPaint);
            }

            if (shape.StrokeEnabled)
            {
                strokePaint.Color = ApplyOpacity(useNodeColors ? nodeColor : shape.StrokeColor, opacity);
                strokePaint.StrokeWidth = MathF.Max(0.0f, shape.StrokeWidth);
                canvas.DrawPath(shape.Path, strokePaint);
            }
        }

        canvas.Restore();
        return true;
    }

    private static SKColor ApplyOpacity(SKColor color, float opacity)
    {
        return color.WithAlpha((byte)Math.Clamp(MathF.Round(color.Alpha * Math.Clamp(opacity, 0.0f, 1.0f)), 0.0f, 255.0f));
    }

    private sealed class SvgIconDocument(SKRect viewBox, IReadOnlyList<SvgIconShape> shapes, SKRect contentBounds)
    {
        public SKRect ViewBox { get; } = viewBox;
        public IReadOnlyList<SvgIconShape> Shapes { get; } = shapes;
        public SKRect ContentBounds { get; } = contentBounds;

        public static bool TryParse(string? svg, out SvgIconDocument? document)
        {
            document = null;
            if (string.IsNullOrWhiteSpace(svg))
                return false;

            try
            {
                var xml = XDocument.Parse(svg, LoadOptions.None);
                var root = xml.Root;
                if (root == null || !string.Equals(root.Name.LocalName, "svg", StringComparison.OrdinalIgnoreCase))
                    return false;

                var viewBoxValue = root.Attribute("viewBox")?.Value;
                if (!TryParseViewBox(viewBoxValue, out var viewBox))
                    return false;

                var shapes = new List<SvgIconShape>();
                foreach (var element in root.Descendants())
                {
                    if (TryParseShape(element, out var shape))
                        shapes.Add(shape);
                }

                var contentBounds = CalculateContentBounds(shapes, viewBox);
                document = new SvgIconDocument(viewBox, shapes, contentBounds);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseShape(XElement element, out SvgIconShape shape)
        {
            shape = default!;
            var kind = element.Name.LocalName;
            if (string.Equals(GetAttributeValue(element, "display"), "none", StringComparison.OrdinalIgnoreCase)
                || string.Equals(GetAttributeValue(element, "visibility"), "hidden", StringComparison.OrdinalIgnoreCase))
                return false;

            SKPath? path = kind switch
            {
                "path" => CreatePath(element),
                "rect" => CreateRectangle(element),
                "circle" => CreateCircle(element),
                "ellipse" => CreateEllipse(element),
                "line" => CreateLine(element),
                "polyline" => CreatePolyline(element, close: false),
                "polygon" => CreatePolyline(element, close: true),
                _ => null,
            };

            if (path == null)
                return false;

            var style = ParseStyle(element.Attribute("style")?.Value);
            var defaultFillEnabled = kind is not "line" and not "polyline";
            var fillSpecified = TryParsePaint(GetAttributeValue(element, "fill", style), SKColors.Black, out var fillEnabled, out var fillColor);
            var strokeSpecified = TryParsePaint(GetAttributeValue(element, "stroke", style), SKColors.Transparent, out var strokeEnabled, out var strokeColor);
            if (!fillSpecified)
            {
                fillEnabled = defaultFillEnabled;
                fillColor = SKColors.Black;
            }

            if (!strokeSpecified)
            {
                strokeEnabled = false;
                strokeColor = SKColors.Transparent;
            }

            var opacity = ParseFloat(GetAttributeValue(element, "opacity", style), 1.0f);
            var fillOpacity = ParseFloat(GetAttributeValue(element, "fill-opacity", style), 1.0f) * opacity;
            var strokeOpacity = ParseFloat(GetAttributeValue(element, "stroke-opacity", style), 1.0f) * opacity;
            fillColor = ApplyOpacity(fillColor, fillOpacity);
            strokeColor = ApplyOpacity(strokeColor, strokeOpacity);

            shape = new SvgIconShape(
                path,
                kind,
                path.Bounds,
                fillEnabled,
                fillColor,
                strokeEnabled,
                strokeColor,
                ParseFloat(GetAttributeValue(element, "stroke-width", style), 1.0f));
            return true;
        }

        private static SKPath? CreatePath(XElement element)
        {
            var pathData = GetAttributeValue(element, "d");
            if (string.IsNullOrWhiteSpace(pathData))
                return null;

            try
            {
                return SKPath.ParseSvgPathData(NormalizeScientificNotation(pathData));
            }
            catch
            {
                return null;
            }
        }

        private static SKPath? CreateRectangle(XElement element)
        {
            var width = ParseFloat(GetAttributeValue(element, "width"), 0.0f);
            var height = ParseFloat(GetAttributeValue(element, "height"), 0.0f);
            if (width <= 0.0f || height <= 0.0f)
                return null;

            var x = ParseFloat(GetAttributeValue(element, "x"), 0.0f);
            var y = ParseFloat(GetAttributeValue(element, "y"), 0.0f);
            var rx = MathF.Max(0.0f, ParseFloat(GetAttributeValue(element, "rx"), 0.0f));
            var ry = MathF.Max(0.0f, ParseFloat(GetAttributeValue(element, "ry"), rx));
            var rect = new SKRect(x, y, x + width, y + height);
            var path = new SKPath();

            if (rx > 0.0f || ry > 0.0f)
            {
                var roundRect = new SKRoundRect();
                roundRect.SetRectRadii(rect,
                [
                    new SKPoint(rx, ry),
                    new SKPoint(rx, ry),
                    new SKPoint(rx, ry),
                    new SKPoint(rx, ry),
                ]);
                path.AddRoundRect(roundRect);
            }
            else
            {
                path.AddRect(rect);
            }

            return path;
        }

        private static SKPath? CreateCircle(XElement element)
        {
            var radius = ParseFloat(GetAttributeValue(element, "r"), 0.0f);
            if (radius <= 0.0f)
                return null;

            var cx = ParseFloat(GetAttributeValue(element, "cx"), 0.0f);
            var cy = ParseFloat(GetAttributeValue(element, "cy"), 0.0f);
            var path = new SKPath();
            path.AddCircle(cx, cy, radius);
            return path;
        }

        private static SKPath? CreateEllipse(XElement element)
        {
            var rx = ParseFloat(GetAttributeValue(element, "rx"), 0.0f);
            var ry = ParseFloat(GetAttributeValue(element, "ry"), 0.0f);
            if (rx <= 0.0f || ry <= 0.0f)
                return null;

            var cx = ParseFloat(GetAttributeValue(element, "cx"), 0.0f);
            var cy = ParseFloat(GetAttributeValue(element, "cy"), 0.0f);
            var path = new SKPath();
            path.AddOval(new SKRect(cx - rx, cy - ry, cx + rx, cy + ry));
            return path;
        }

        private static SKPath? CreateLine(XElement element)
        {
            var x1 = ParseFloat(GetAttributeValue(element, "x1"), 0.0f);
            var y1 = ParseFloat(GetAttributeValue(element, "y1"), 0.0f);
            var x2 = ParseFloat(GetAttributeValue(element, "x2"), 0.0f);
            var y2 = ParseFloat(GetAttributeValue(element, "y2"), 0.0f);

            var path = new SKPath();
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            return path;
        }

        private static SKPath? CreatePolyline(XElement element, bool close)
        {
            var points = ParsePoints(GetAttributeValue(element, "points"));
            if (points.Count < 2)
                return null;

            var path = new SKPath();
            path.MoveTo(points[0]);
            for (var i = 1; i < points.Count; i++)
                path.LineTo(points[i]);

            if (close)
                path.Close();

            return path;
        }

        private static List<SKPoint> ParsePoints(string? pointsText)
        {
            var values = SplitNumbers(pointsText);
            var points = new List<SKPoint>();

            for (var i = 0; i + 1 < values.Count; i += 2)
                points.Add(new SKPoint(values[i], values[i + 1]));

            return points;
        }

        private static bool TryParseViewBox(string? viewBoxText, out SKRect viewBox)
        {
            viewBox = SKRect.Empty;
            var values = SplitNumbers(viewBoxText);
            if (values.Count != 4 || values[2] <= 0.0f || values[3] <= 0.0f)
                return false;

            viewBox = new SKRect(values[0], values[1], values[0] + values[2], values[1] + values[3]);
            return true;
        }

        private static List<float> SplitNumbers(string? text)
        {
            var values = new List<float>();
            if (string.IsNullOrWhiteSpace(text))
                return values;

            var parts = text
                .Replace(",", " ", StringComparison.Ordinal)
                .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    values.Add(value);
            }

            return values;
        }

        private static Dictionary<string, string> ParseStyle(string? styleText)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(styleText))
                return result;

            var entries = styleText.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var entry in entries)
            {
                var separatorIndex = entry.IndexOf(':');
                if (separatorIndex <= 0 || separatorIndex >= entry.Length - 1)
                    continue;

                var key = entry[..separatorIndex].Trim();
                var value = entry[(separatorIndex + 1)..].Trim();
                if (key.Length > 0)
                    result[key] = value;
            }

            return result;
        }

        private static string? GetAttributeValue(XElement element, string name, Dictionary<string, string>? style = null)
        {
            if (element.Attribute(name)?.Value is { Length: > 0 } attributeValue)
                return attributeValue;

            return style != null && style.TryGetValue(name, out var styleValue)
                ? styleValue
                : null;
        }

        private static bool TryParsePaint(string? value, SKColor defaultColor, out bool enabled, out SKColor color)
        {
            enabled = true;
            color = defaultColor;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (string.Equals(value, "none", StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                color = SKColors.Transparent;
                return true;
            }

            if (string.Equals(value, "currentColor", StringComparison.OrdinalIgnoreCase))
            {
                color = defaultColor;
                return true;
            }

            return TryParseColor(value, out color);
        }

        private static bool TryParseColor(string value, out SKColor color)
        {
            color = SKColors.Black;
            var trimmed = value.Trim();
            if (trimmed.Length == 0)
                return false;

            if (trimmed.StartsWith('#'))
            {
                var hex = trimmed[1..];
                if (hex.Length == 3)
                {
                    color = new SKColor(
                        ExpandHex(hex[0]),
                        ExpandHex(hex[1]),
                        ExpandHex(hex[2]));
                    return true;
                }

                if (hex.Length == 4)
                {
                    color = new SKColor(
                        ExpandHex(hex[0]),
                        ExpandHex(hex[1]),
                        ExpandHex(hex[2]),
                        ExpandHex(hex[3]));
                    return true;
                }

                if (hex.Length == 6 && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                {
                    color = new SKColor(
                        (byte)((rgb >> 16) & 0xFF),
                        (byte)((rgb >> 8) & 0xFF),
                        (byte)(rgb & 0xFF));
                    return true;
                }

                if (hex.Length == 8 && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgba))
                {
                    color = new SKColor(
                        (byte)((rgba >> 24) & 0xFF),
                        (byte)((rgba >> 16) & 0xFF),
                        (byte)((rgba >> 8) & 0xFF),
                        (byte)(rgba & 0xFF));
                    return true;
                }

                return false;
            }

            if (trimmed.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(')'))
            {
                var parts = trimmed[4..^1].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                    return false;

                if (byte.TryParse(parts[0], CultureInfo.InvariantCulture, out var red)
                    && byte.TryParse(parts[1], CultureInfo.InvariantCulture, out var green)
                    && byte.TryParse(parts[2], CultureInfo.InvariantCulture, out var blue))
                {
                    color = new SKColor(red, green, blue);
                    return true;
                }
            }

            return false;
        }

        private static byte ExpandHex(char value)
        {
            var expanded = string.Concat(value, value);
            return byte.Parse(expanded, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        private static float ParseFloat(string? value, float fallback)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : fallback;
        }

        private static SKRect CalculateContentBounds(IReadOnlyList<SvgIconShape> shapes, SKRect fallbackBounds)
        {
            var hasBounds = false;
            var bounds = SKRect.Empty;

            foreach (var shape in shapes)
            {
                if (shape.Bounds.IsEmpty)
                    continue;

                var inflated = shape.InflatedBounds;
                bounds = hasBounds
                    ? UnionBounds(bounds, inflated)
                    : inflated;
                hasBounds = true;
            }

            if (!hasBounds || bounds.Width <= 0.0f || bounds.Height <= 0.0f)
                return fallbackBounds;

            return bounds;
        }

        private static SKRect UnionBounds(SKRect left, SKRect right)
        {
            return new SKRect(
                MathF.Min(left.Left, right.Left),
                MathF.Min(left.Top, right.Top),
                MathF.Max(left.Right, right.Right),
                MathF.Max(left.Bottom, right.Bottom));
        }

        private static string NormalizeScientificNotation(string value)
        {
            return Regex.Replace(
                value,
                @"[-+]?(?:\d+\.?\d*|\.\d+)[eE][-+]?\d+",
                static match =>
                {
                    return double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                        ? parsed.ToString("0.#############################", CultureInfo.InvariantCulture)
                        : match.Value;
                });
        }

        private static SKColor ApplyOpacity(SKColor color, float opacity)
        {
            return color.WithAlpha((byte)Math.Clamp(MathF.Round(color.Alpha * Math.Clamp(opacity, 0.0f, 1.0f)), 0.0f, 255.0f));
        }
    }

    private sealed class SvgIconShape(
        SKPath path,
        string kind,
        SKRect bounds,
        bool fillEnabled,
        SKColor fillColor,
        bool strokeEnabled,
        SKColor strokeColor,
        float strokeWidth)
    {
        public SKPath Path { get; } = path;
        public string Kind { get; } = kind;
        public SKRect Bounds { get; } = bounds;
        public SKRect InflatedBounds { get; } = strokeEnabled && strokeWidth > 0.0f
            ? new SKRect(
                bounds.Left - (strokeWidth * 0.5f),
                bounds.Top - (strokeWidth * 0.5f),
                bounds.Right + (strokeWidth * 0.5f),
                bounds.Bottom + (strokeWidth * 0.5f))
            : bounds;
        public bool FillEnabled { get; } = fillEnabled;
        public SKColor FillColor { get; } = fillColor;
        public bool StrokeEnabled { get; } = strokeEnabled;
        public SKColor StrokeColor { get; } = strokeColor;
        public float StrokeWidth { get; } = strokeWidth;

        public bool IsViewBoxBackground(SKRect viewBox)
        {
            if (!string.Equals(Kind, "rect", StringComparison.OrdinalIgnoreCase))
                return false;

            return AreClose(Bounds.Left, viewBox.Left)
                   && AreClose(Bounds.Top, viewBox.Top)
                   && AreClose(Bounds.Right, viewBox.Right)
                   && AreClose(Bounds.Bottom, viewBox.Bottom);
        }

        private static bool AreClose(float left, float right)
        {
            return MathF.Abs(left - right) < 0.01f;
        }
    }
}
