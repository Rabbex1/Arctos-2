using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Represents a diamond node visual.
/// </summary>
public sealed class DiamondNodeVisual : NodeVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new diamond node visual.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    public DiamondNodeVisual(NodeShapeStyle shapeStyle)
        : base(shapeStyle)
    {
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Renders this diamond node visual.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var fillPaint = CreateFillPaint();
        using var strokePaint = CreateStrokePaint();
        using var path = new SKPath();

        path.MoveTo(Bounds.MidX, Bounds.Top);
        path.LineTo(Bounds.Right, Bounds.MidY);
        path.LineTo(Bounds.MidX, Bounds.Bottom);
        path.LineTo(Bounds.Left, Bounds.MidY);
        path.Close();

        canvas.DrawPath(path, fillPaint);
        canvas.DrawPath(path, strokePaint);
        RenderChildren(canvas);
    }

    #endregion Public Methods
}
