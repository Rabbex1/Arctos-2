namespace Arctos.Forms;

/// <summary>
/// Dependency-free colour-wheel geometry. RGB mixing is in sRGB byte space.
/// The web canvas mirrors these equations in colourTriangle.js.
/// </summary>
public static class ColourTriangle
{
    public const double Radius = 0.39;
    public readonly record struct Point(double X, double Y);
    public readonly record struct Weights(double Hue, double White, double Black);

    public static double WrapHue(double hue) => (hue % 360 + 360) % 360;

    public static Point Vertex(double hue, double offset = 0, double radius = Radius)
    {
        var angle = (hue + offset - 90) * Math.PI / 180;
        return new(0.5 + radius * Math.Cos(angle), 0.5 + radius * Math.Sin(angle));
    }

    public static Weights GetWeights(Point p, double hue)
    {
        var a = Vertex(hue); var b = Vertex(hue, 120); var c = Vertex(hue, 240);
        var denominator = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
        var h = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denominator;
        var w = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denominator;
        return new(h, w, 1 - h - w);
    }

    public static bool Contains(Weights weights) => weights.Hue >= 0 && weights.White >= 0 && weights.Black >= 0;

    public static Weights ClampToTriangle(Point point, double hue)
    {
        var weights = GetWeights(point, hue);
        if (Contains(weights)) return weights;
        var a = Vertex(hue); var b = Vertex(hue, 120); var c = Vertex(hue, 240);
        var closest = new[] { Project(point, a, b), Project(point, b, c), Project(point, c, a) }
            .MinBy(p => DistanceSquared(point, p));
        weights = GetWeights(closest, hue);
        var h = Math.Max(0, weights.Hue); var w = Math.Max(0, weights.White); var k = Math.Max(0, weights.Black);
        var total = h + w + k;
        return new(h / total, w / total, k / total);
    }

    public static Point Position(double hue, Weights weights)
    {
        var a = Vertex(hue); var b = Vertex(hue, 120); var c = Vertex(hue, 240);
        return new(a.X * weights.Hue + b.X * weights.White + c.X * weights.Black,
            a.Y * weights.Hue + b.Y * weights.White + c.Y * weights.Black);
    }

    public static InputColor PureHue(double hue)
    {
        var h = WrapHue(hue) / 60;
        var x = 1 - Math.Abs(h % 2 - 1);
        var (r, g, b) = h switch
        {
            < 1 => (1d, x, 0d), < 2 => (x, 1d, 0d), < 3 => (0d, 1d, x),
            < 4 => (0d, x, 1d), < 5 => (x, 0d, 1d), _ => (1d, 0d, x)
        };
        return new(Channel(r), Channel(g), Channel(b));
    }

    public static InputColor Mix(InputColor hue, Weights weights, byte alpha = 255) => new(
        Channel(hue.Red / 255d * weights.Hue + weights.White),
        Channel(hue.Green / 255d * weights.Hue + weights.White),
        Channel(hue.Blue / 255d * weights.Hue + weights.White), alpha);

    public static (double Hue, Weights Weights) FromColor(InputColor color, double previousHue = 0)
    {
        var r = color.Red / 255d; var g = color.Green / 255d; var b = color.Blue / 255d;
        var max = Math.Max(r, Math.Max(g, b)); var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        var hue = delta == 0 ? previousHue : max == r ? 60 * ((g - b) / delta)
            : max == g ? 60 * ((b - r) / delta + 2) : 60 * ((r - g) / delta + 4);
        return (WrapHue(hue), new(delta, min, 1 - max));
    }

    private static byte Channel(double value) => (byte)Math.Clamp(Math.Floor(value * 255 + 0.5), 0, 255);
    private static double DistanceSquared(Point a, Point b) => (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
    private static Point Project(Point p, Point a, Point b)
    {
        var dx = b.X - a.X; var dy = b.Y - a.Y;
        var t = Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy), 0, 1);
        return new(a.X + dx * t, a.Y + dy * t);
    }
}
