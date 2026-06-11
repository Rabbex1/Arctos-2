using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Represents a capsule node visual.
/// </summary>
public sealed class CapsuleNodeVisual : NodeVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new capsule node visual.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    public CapsuleNodeVisual(NodeShapeStyle shapeStyle)
        : base(shapeStyle)
    {
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Renders this capsule node visual.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var fillPaint = CreateFillPaint();
        using var strokePaint = CreateStrokePaint();
        var radius = Bounds.Height / 2.0f;

        canvas.DrawRoundRect(Bounds, radius, radius, fillPaint);
        canvas.DrawRoundRect(Bounds, radius, radius, strokePaint);
        RenderChildren(canvas);
    }

    #endregion Public Methods
}
