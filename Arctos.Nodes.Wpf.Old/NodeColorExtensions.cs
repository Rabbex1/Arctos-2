using System.Windows.Media;
using Arctos.Nodes;
using SkiaSharp;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Provides WPF color conversion helpers for <see cref="NodeColor"/>.
/// </summary>
public static class NodeColorExtensions
{
    #region Public Methods

    /// <summary>
    /// Converts a portable <see cref="NodeColor"/> to a WPF <see cref="Color"/>.
    /// </summary>
    public static Color ToMediaColor(this NodeColor color)
    {
        return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    }

    /// <summary>
    /// Converts a WPF <see cref="Color"/> to a portable <see cref="NodeColor"/>.
    /// </summary>
    public static NodeColor ToNodeColor(this Color color)
    {
        return new NodeColor(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Converts a portable <see cref="NodeColor"/> to a Skia <see cref="SKColor"/>.
    /// </summary>
    public static SKColor ToSkColor(this NodeColor color)
    {
        return new SKColor(color.Red, color.Green, color.Blue, color.Alpha);
    }

    #endregion Public Methods
}
