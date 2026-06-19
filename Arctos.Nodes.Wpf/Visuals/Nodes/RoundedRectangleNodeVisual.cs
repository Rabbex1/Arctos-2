using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Represents a rounded rectangle node visual.
/// </summary>
public sealed class RoundedRectangleNodeVisual : NodeVisual
{
    private readonly float _cornerRadius;

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new rounded rectangle node visual.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    /// <param name="headerStyle">The style used to render the optional node header.</param>
    /// <param name="headerText">The resolved header text.</param>
    /// <param name="fontFamily">The font family used for node text when one is defined.</param>
    /// <param name="headerFontSize">The header font size used for node header text.</param>
    /// <param name="nodeUnitSize">The world-space size of <c>1nu</c> for the current document typography.</param>
    /// <param name="nodeStrokeSize">The world-space size of <c>1ns</c> for the current document typography.</param>
    public RoundedRectangleNodeVisual(NodeShapeStyle shapeStyle, NodeHeaderStyle? headerStyle, string? headerText, string? fontFamily, float headerFontSize, NodeColor fontColor, float headerHorizontalPadding, float nodeUnitSize, float nodeStrokeSize, float headerHeight, float cornerRadius)
        : base(shapeStyle, headerStyle, headerText, fontFamily, headerFontSize, fontColor, headerHorizontalPadding, nodeUnitSize, nodeStrokeSize, headerHeight)
    {
        _cornerRadius = MathF.Max(0.0f, cornerRadius);
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
        var headerBounds = GetHeaderBounds();

        canvas.DrawRoundRect(Bounds, _cornerRadius, _cornerRadius, fillPaint);

        if (headerBounds.HasValue)
        {
            using var headerFillPaint = CreateHeaderFillPaint();
            var clipRoundRect = new SKRoundRect(Bounds, _cornerRadius, _cornerRadius);
            canvas.Save();
            canvas.ClipRoundRect(clipRoundRect, SKClipOperation.Intersect, true);
            canvas.DrawRect(headerBounds.Value, headerFillPaint);
            canvas.Restore();
        }

        canvas.DrawRoundRect(Bounds, _cornerRadius, _cornerRadius, strokePaint);

        if (headerBounds.HasValue)
            RenderHeaderText(canvas, headerBounds.Value);

        RenderChildren(canvas);
    }

    #endregion Public Methods
}
