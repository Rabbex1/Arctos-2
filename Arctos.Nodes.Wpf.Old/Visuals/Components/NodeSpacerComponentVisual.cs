using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a spacer component inside a node.
/// </summary>
public sealed class NodeSpacerComponentVisual : NodeComponentVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly bool _isSelected;
    private readonly bool _isHovered;
    private readonly float _selectionDashLength;
    private readonly float _selectionHorizontalPadding;
    private readonly SKColor _selectionColor;
    private readonly bool _showDebugBounds;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new spacer component visual.
    /// </summary>
    public NodeSpacerComponentVisual(bool showDebugBounds = false, bool isHovered = false, bool isSelected = false, SKColor defaultSelectionColor = default, float selectionDashLength = 1.0f, float selectionHorizontalPadding = 0.0f)
    {
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

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true, _selectionHorizontalPadding);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false, _selectionHorizontalPadding);
    }

    #endregion Public Methods
}
