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
    /// <param name="headerStyle">The style used to render the optional node header.</param>
    /// <param name="headerText">The resolved header text.</param>
    /// <param name="fontFamily">The font family used for node text when one is defined.</param>
    /// <param name="headerFontSize">The header font size used for node header text.</param>
    /// <param name="nodeUnitSize">The world-space size of <c>1nu</c> for the current document typography.</param>
    /// <param name="nodeStrokeSize">The world-space size of <c>1ns</c> for the current document typography.</param>
    public DiamondNodeVisual(NodeShapeStyle shapeStyle, NodeHeaderStyle? headerStyle, string? headerText, string? fontFamily, float headerFontSize, NodeColor fontColor, float headerHorizontalPadding, float nodeUnitSize, float nodeStrokeSize, float headerHeight)
        : base(shapeStyle, headerStyle, headerText, fontFamily, headerFontSize, fontColor, headerHorizontalPadding, nodeUnitSize, nodeStrokeSize, headerHeight)
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
