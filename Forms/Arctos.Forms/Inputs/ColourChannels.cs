namespace Arctos.Forms;

public static class ColourChannels
{
    public static double[] Read(InputColor color, string format)
    {
        if (format == "RGB") return [color.Red, color.Green, color.Blue];
        var (hue, weights) = ColourTriangle.FromColor(color);
        var max = 1 - weights.Black; var min = weights.White; var delta = weights.Hue;
        var light = (max + min) / 2;
        return format == "HSV" ? [hue, max == 0 ? 0 : delta / max * 100, max * 100]
            : [hue, delta == 0 ? 0 : delta / (1 - Math.Abs(2 * light - 1)) * 100, light * 100];
    }
    public static InputColor Write(string format, double a, double b, double c, byte alpha)
    {
        static byte Byte(double v) => (byte)Math.Clamp(Math.Round(v), 0, 255);
        if (format == "RGB") return new(Byte(a), Byte(b), Byte(c), alpha);
        var s = Math.Clamp(b / 100, 0, 1); var v = Math.Clamp(c / 100, 0, 1);
        var chroma = format == "HSV" ? v * s : (1 - Math.Abs(2 * v - 1)) * s;
        var white = format == "HSV" ? v - chroma : v - chroma / 2;
        return ColourTriangle.Mix(ColourTriangle.PureHue(a), new(chroma, white, 1 - chroma - white), alpha);
    }
}
