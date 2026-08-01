using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a separator component inside a node.
/// </summary>
public sealed class NodeSeparatorComponentVisual : NodeComponentVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly bool _isSelected;
    private readonly bool _isHovered;
    private readonly SKColor _selectionColor;
    private readonly float _selectionDashLength;
    private readonly float _selectionHorizontalPadding;
    private readonly bool _showDebugBounds;
    private readonly SKColor _strokeColor;
    private readonly float _strokeWidth;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new separator component visual.
    /// </summary>
    public NodeSeparatorComponentVisual(SKColor strokeColor, float strokeWidth, bool showDebugBounds = false, bool isHovered = false, bool isSelected = false, SKColor defaultSelectionColor = default, float selectionDashLength = 1.0f, float selectionHorizontalPadding = 0.0f)
    {
        _strokeColor = strokeColor;
        _strokeWidth = MathF.Max(0.0f, strokeWidth);
        _showDebugBounds = showDebugBounds;
        _isHovered = isHovered;
        _isSelected = isSelected;
        _selectionColor = defaultSelectionColor == default ? SKColors.Gray : defaultSelectionColor;
        _selectionDashLength = selectionDashLength;
        _selectionHorizontalPadding = MathF.Max(0.0f, selectionHorizontalPadding);
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <inheritdoc />
    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var linePaint = new SKPaint
        {
            Color = _strokeColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _strokeWidth,
        };

        canvas.DrawLine(Bounds.Left, Bounds.MidY, Bounds.Right, Bounds.MidY, linePaint);

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true, _selectionHorizontalPadding);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false, _selectionHorizontalPadding);
    }

    #endregion Public Methods
}
