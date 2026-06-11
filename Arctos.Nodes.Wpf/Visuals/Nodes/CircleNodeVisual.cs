using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Represents a circle node visual.
/// </summary>
public sealed class CircleNodeVisual : NodeVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new circle node visual.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    public CircleNodeVisual(NodeShapeStyle shapeStyle)
        : base(shapeStyle)
    {
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Renders this circle node visual.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var fillPaint = CreateFillPaint();
        using var strokePaint = CreateStrokePaint();

        canvas.DrawOval(Bounds, fillPaint);
        canvas.DrawOval(Bounds, strokePaint);
        RenderChildren(canvas);
    }

    #endregion Public Methods
}
