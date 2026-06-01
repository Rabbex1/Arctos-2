namespace Arctos.Nodes;

/// <summary>
/// Represents a portable RGBA color value for graph and node document data.
/// </summary>
public readonly record struct NodeColor(byte Red, byte Green, byte Blue, byte Alpha = 255)
{
    /// <summary>
    /// Gets an opaque black color.
    /// </summary>
    public static NodeColor Black => new(0, 0, 0);

    /// <summary>
    /// Gets an opaque white color.
    /// </summary>
    public static NodeColor White => new(255, 255, 255);

    /// <summary>
    /// Gets a transparent color.
    /// </summary>
    public static NodeColor Transparent => new(0, 0, 0, 0);

    /// <summary>
    /// Provides the built-in neutral palette colors.
    /// </summary>
    public static class Neutral
    {
        public static NodeColor Canvas => new(245, 245, 242);
        public static NodeColor Surface => new(230, 230, 225);
        public static NodeColor Border => new(96, 96, 92);
        public static NodeColor Text => new(28, 28, 30);
        public static NodeColor MutedText => new(115, 115, 120);
    }

    /// <summary>
    /// Provides the built-in blue accent palette.
    /// </summary>
    public static class Blues
    {
        public static NodeColor Light => new(173, 216, 230);
        public static NodeColor Base => new(52, 152, 219);
        public static NodeColor Dark => new(41, 76, 122);
    }

    /// <summary>
    /// Provides the built-in teal accent palette.
    /// </summary>
    public static class Teals
    {
        public static NodeColor Light => new(165, 230, 223);
        public static NodeColor Base => new(38, 166, 154);
        public static NodeColor Dark => new(20, 88, 90);
    }

    /// <summary>
    /// Provides the built-in green accent palette.
    /// </summary>
    public static class Greens
    {
        public static NodeColor Light => new(186, 230, 176);
        public static NodeColor Base => new(76, 175, 80);
        public static NodeColor Dark => new(46, 92, 56);
    }

    /// <summary>
    /// Provides the built-in lime accent palette.
    /// </summary>
    public static class Limes
    {
        public static NodeColor Light => new(220, 235, 163);
        public static NodeColor Base => new(156, 204, 101);
        public static NodeColor Dark => new(92, 116, 46);
    }

    /// <summary>
    /// Provides the built-in yellow accent palette.
    /// </summary>
    public static class Yellows
    {
        public static NodeColor Light => new(255, 241, 166);
        public static NodeColor Base => new(255, 214, 10);
        public static NodeColor Dark => new(168, 132, 21);
    }

    /// <summary>
    /// Provides the built-in orange accent palette.
    /// </summary>
    public static class Oranges
    {
        public static NodeColor Light => new(255, 208, 150);
        public static NodeColor Base => new(255, 152, 0);
        public static NodeColor Dark => new(158, 84, 19);
    }

    /// <summary>
    /// Provides the built-in red accent palette.
    /// </summary>
    public static class Reds
    {
        public static NodeColor Light => new(245, 171, 171);
        public static NodeColor Base => new(231, 76, 60);
        public static NodeColor Dark => new(126, 40, 42);
    }

    /// <summary>
    /// Provides the built-in pink accent palette.
    /// </summary>
    public static class Pinks
    {
        public static NodeColor Light => new(244, 179, 204);
        public static NodeColor Base => new(233, 30, 99);
        public static NodeColor Dark => new(122, 32, 76);
    }

    /// <summary>
    /// Provides the built-in purple accent palette.
    /// </summary>
    public static class Purples
    {
        public static NodeColor Light => new(210, 179, 242);
        public static NodeColor Base => new(156, 39, 176);
        public static NodeColor Dark => new(79, 36, 106);
    }
}
