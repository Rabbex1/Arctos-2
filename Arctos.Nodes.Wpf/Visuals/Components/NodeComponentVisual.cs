using Arctos.Nodes.Wpf.Rendering;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Provides the common base for detailed node component visuals.
/// </summary>
public abstract class NodeComponentVisual : IVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private static readonly IReadOnlyList<IVisual> EmptyChildren = [];
    private SKRect _bounds;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the child visuals for this component visual.
    /// </summary>
    public IReadOnlyList<IVisual> Children => EmptyChildren;

    /// <summary>
    /// Gets or sets the current layout bounds for this component visual.
    /// </summary>
    public SKRect Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Renders this component visual to the provided Skia canvas.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public abstract void Render(SKCanvas canvas);

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    /// <summary>
    /// Renders a temporary debug outline around the current component bounds.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    protected void RenderDebugBounds(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var debugPaint = new SKPaint
        {
            Color = SKColors.Red,
            IsAntialias = false,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
        };

        canvas.DrawRect(Bounds, debugPaint);
    }

    /// <summary>
    /// Renders a selection outline around the current component bounds.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="selectionColor">The color used for the selection outline.</param>
    /// <param name="dashLength">The dash and gap length in world-space units.</param>
    /// <param name="strokeWidth">The stroke width used for the selection outline.</param>
    /// <param name="dashed"><see langword="true"/> to use a dashed outline; otherwise <see langword="false"/>.</param>
    protected void RenderSelectionBounds(SKCanvas canvas, SKColor selectionColor, float dashLength, float strokeWidth, bool dashed, float horizontalPadding = 0.0f)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        var selectionBounds = horizontalPadding > 0.0f
            ? new SKRect(Bounds.Left - horizontalPadding, Bounds.Top, Bounds.Right + horizontalPadding, Bounds.Bottom)
            : Bounds;

        NodeVisualDrawing.DrawOutlineSelector(
            canvas,
            dashed ? NodeVisualDrawing.OutlineSelectorMode.Dashed : NodeVisualDrawing.OutlineSelectorMode.Solid,
            selectionBounds,
            selectionColor,
            dashLength,
            strokeWidth);
    }

    #endregion Protected Methods
}
