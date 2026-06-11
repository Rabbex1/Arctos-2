using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Represents a rounded rectangle node visual.
/// </summary>
public sealed class RoundedRectangleNodeVisual : NodeVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new rounded rectangle node visual.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    public RoundedRectangleNodeVisual(NodeShapeStyle shapeStyle)
        : base(shapeStyle)
    {
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Renders this rounded rectangle node visual.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var fillPaint = CreateFillPaint();
        using var strokePaint = CreateStrokePaint();

        canvas.DrawRoundRect(Bounds, 10.0f, 10.0f, fillPaint);
        canvas.DrawRoundRect(Bounds, 10.0f, 10.0f, strokePaint);
        RenderChildren(canvas);
    }

    #endregion Public Methods
}
