using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Represents a rectangle node visual.
/// </summary>
public sealed class RectangleNodeVisual : NodeVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new rectangle node visual.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    public RectangleNodeVisual(NodeShapeStyle shapeStyle)
        : base(shapeStyle)
    {
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Renders this rectangle node visual.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var fillPaint = CreateFillPaint();
        using var strokePaint = CreateStrokePaint();

        canvas.DrawRect(Bounds, fillPaint);
        canvas.DrawRect(Bounds, strokePaint);
        RenderChildren(canvas);
    }

    #endregion Public Methods
}
