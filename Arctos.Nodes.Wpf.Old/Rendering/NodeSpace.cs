using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Represents the logical node-space used when rendering the current Skia surface.
/// </summary>
public readonly record struct NodeSpace(float Width, float Height)
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>Gets half the current node-space width.</summary>
    public float HalfWidth => Width / 2.0f;

    /// <summary>Gets half the current node-space height.</summary>
    public float HalfHeight => Height / 2.0f;

    /// <summary>Gets the visible node-space bounds centered on the origin.</summary>
    public SKRect Bounds => new(-HalfWidth, -HalfHeight, HalfWidth, HalfHeight);

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Creates the view matrix used to render the current node-space to the Skia surface.
    /// </summary>
    /// <param name="pan">The current node-space pan offset.</param>
    /// <param name="zoom">The current node-space zoom factor.</param>
    /// <returns>The composed node-space view matrix.</returns>
    public SKMatrix CreateViewMatrix(NodePoint pan, float zoom)
    {
        return new SKMatrix
        {
            ScaleX = zoom,
            SkewX = 0.0f,
            TransX = HalfWidth + (pan.X * zoom),
            SkewY = 0.0f,
            ScaleY = zoom,
            TransY = HalfHeight + (pan.Y * zoom),
            Persp0 = 0.0f,
            Persp1 = 0.0f,
            Persp2 = 1.0f,
        };
    }

    /// <summary>
    /// Gets the visible node-space bounds after applying the current pan and zoom.
    /// </summary>
    /// <param name="pan">The current node-space pan offset.</param>
    /// <param name="zoom">The current node-space zoom factor.</param>
    /// <returns>The visible node-space bounds.</returns>
    public SKRect GetVisibleBounds(NodePoint pan, float zoom)
    {
        return new SKRect(
            (-HalfWidth / zoom) - pan.X,
            (-HalfHeight / zoom) - pan.Y,
            (HalfWidth / zoom) - pan.X,
            (HalfHeight / zoom) - pan.Y);
    }

    /// <summary>
    /// Converts a screen-space point into the centered node-space.
    /// </summary>
    /// <param name="x">The screen-space x-coordinate.</param>
    /// <param name="y">The screen-space y-coordinate.</param>
    /// <param name="pan">The current node-space pan offset.</param>
    /// <param name="zoom">The current node-space zoom factor.</param>
    /// <returns>The converted node-space point.</returns>
    public SKPoint ToNodePoint(double x, double y, NodePoint pan, float zoom)
    {
        return new SKPoint(
            (((float)x - HalfWidth) / zoom) - pan.X,
            (((float)y - HalfHeight) / zoom) - pan.Y);
    }

    #endregion Public Methods
}
