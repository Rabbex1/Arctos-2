using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Provides shared color and outline helpers for node visuals.
/// </summary>
public static class NodeVisualDrawing
{
    /// <summary>
    /// Identifies the outline style used when drawing selection helpers.
    /// </summary>
    public enum OutlineSelectorMode
    {
        Solid,
        Dashed,
    }

    /// <summary>
    /// Resolves a neutral contrast color from the supplied reference color.
    /// </summary>
    /// <param name="colorReference">The reference color used to derive contrast.</param>
    /// <param name="alpha">The alpha channel to apply to the resolved color.</param>
    /// <returns>The resolved neutral contrast color.</returns>
    public static SKColor GetNeutralColor(NodeColor colorReference, byte alpha = byte.MaxValue)
    {
        var grayscale = ((colorReference.Red * 0.299f) + (colorReference.Green * 0.587f) + (colorReference.Blue * 0.114f));
        SKColor baseColor;

        if (grayscale <= 64.0f || grayscale >= 191.0f)
            baseColor = new SKColor(128, 128, 128);
        else if (grayscale <= 127.0f)
            baseColor = new SKColor(192, 192, 192);
        else
            baseColor = new SKColor(72, 72, 72);

        return baseColor.WithAlpha(alpha);
    }

    /// <summary>
    /// Resolves the default selection outline color from the supplied reference color.
    /// </summary>
    /// <param name="colorReference">The reference color used to derive contrast.</param>
    /// <returns>The resolved selection color.</returns>
    public static SKColor GetSelectionColor(NodeColor colorReference)
    {
        return GetNeutralColor(colorReference, 216);
    }

    /// <summary>
    /// Resolves the shared dash length used by selection-style dashed outlines.
    /// </summary>
    /// <param name="minorGridSize">The current minor grid size in world-space units.</param>
    /// <returns>The resolved dash and gap length.</returns>
    public static float GetSelectionDashLength(float minorGridSize)
    {
        return MathF.Max(1.0f, minorGridSize / 4.0f);
    }

    /// <summary>
    /// Draws a solid or dashed outline rectangle using the shared selection visual language.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="mode">The outline mode to render.</param>
    /// <param name="bounds">The bounds of the outline.</param>
    /// <param name="strokeColor">The outline stroke color.</param>
    /// <param name="dashLength">The dash and gap length in world-space units when dashed.</param>
    /// <param name="strokeWidth">The outline stroke width. Use <c>0.0f</c> for a Skia hairline.</param>
    public static void DrawOutlineSelector(SKCanvas canvas, OutlineSelectorMode mode, SKRect bounds, SKColor strokeColor, float dashLength, float strokeWidth = 0.0f)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var outlinePaint = new SKPaint
        {
            Color = strokeColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
        };

        if (mode == OutlineSelectorMode.Dashed)
        {
            var resolvedDashLength = MathF.Max(1.0f, dashLength);
            outlinePaint.PathEffect = SKPathEffect.CreateDash(
            [
                resolvedDashLength,
                resolvedDashLength,
            ], 0.0f);
        }

        canvas.DrawRect(bounds, outlinePaint);
    }
}
