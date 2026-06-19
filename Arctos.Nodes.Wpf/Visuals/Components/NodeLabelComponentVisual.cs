using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a simple text label component inside a node.
/// </summary>
public sealed class NodeLabelComponentVisual : NodeComponentVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly string? _fontFamily;
    private readonly float _opacity;
    private readonly string _text;
    private readonly NodeColor _textColor;
    private readonly float _textSize;
    private readonly NodeFontWeight _fontWeight;
    private readonly bool _showDebugBounds;
    private readonly bool _isHovered;
    private readonly bool _isSelected;
    private readonly SKColor _selectionColor;
    private readonly float _selectionDashLength;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node label component visual.
    /// </summary>
    /// <param name="text">The label text to render.</param>
    /// <param name="fontFamily">The font family used for the label.</param>
    /// <param name="textSize">The font size used for the label.</param>
    /// <param name="textColor">The text color used for the label.</param>
    /// <param name="fontWeight">The font weight used for the label.</param>
    public NodeLabelComponentVisual(string text, string? fontFamily, float textSize, NodeColor textColor, NodeFontWeight fontWeight, float opacity = 1.0f, bool showDebugBounds = false, bool isHovered = false, bool isSelected = false, SKColor defaultSelectionColor = default, float selectionDashLength = 1.0f)
    {
        _text = text ?? string.Empty;
        _fontFamily = fontFamily;
        _opacity = Math.Clamp(opacity, 0.0f, 1.0f);
        _textSize = textSize;
        _textColor = textColor;
        _fontWeight = fontWeight;
        _showDebugBounds = showDebugBounds;
        _isHovered = isHovered;
        _isSelected = isSelected;
        _selectionColor = defaultSelectionColor == default ? SKColors.Gray : defaultSelectionColor;
        _selectionDashLength = selectionDashLength;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Renders this label component visual.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        if (!string.IsNullOrWhiteSpace(_text))
        {
            using var textPaint = CreateTextPaint();
            var textBounds = SKRect.Empty;
            textPaint.MeasureText(_text, ref textBounds);
            var baseline = Bounds.MidY - textBounds.MidY;

            canvas.DrawText(_text, Bounds.Left, baseline, textPaint);
        }

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false);
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private SKPaint CreateTextPaint()
    {
        return new SKPaint
        {
            Color = _textColor.ToSkColor().WithAlpha((byte)Math.Clamp(MathF.Round(_opacity * 255.0f), 0.0f, 255.0f)),
            IsAntialias = true,
            TextSize = _textSize,
            TextAlign = SKTextAlign.Left,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };
    }

    private static SKTypeface? CreateTypeface(string? fontFamily, NodeFontWeight fontWeight)
    {
        var skiaWeight = fontWeight switch
        {
            NodeFontWeight.Light => SKFontStyleWeight.Light,
            NodeFontWeight.Medium => SKFontStyleWeight.Medium,
            NodeFontWeight.SemiBold => SKFontStyleWeight.SemiBold,
            NodeFontWeight.Bold => SKFontStyleWeight.Bold,
            _ => SKFontStyleWeight.Normal,
        };

        return string.IsNullOrWhiteSpace(fontFamily)
            ? SKTypeface.FromFamilyName(null, skiaWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            : SKTypeface.FromFamilyName(fontFamily, skiaWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    }

    #endregion Private Methods
}
