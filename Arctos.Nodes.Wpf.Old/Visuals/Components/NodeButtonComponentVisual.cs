using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a passive button component inside a node definition.
/// </summary>
public sealed class NodeButtonComponentVisual : NodeComponentVisual
{
    private const float BodyHeightScale = 0.8f;
    private const float TextScale = 0.6f;

    private readonly string _label;
    private readonly string? _fontFamily;
    private readonly float _textSize;
    private readonly NodeColor _textColor;
    private readonly NodeFontWeight _fontWeight;
    private readonly NodeColor _borderColor;
    private readonly float _cornerRadius;
    private readonly float _nodeUnitSize;
    private readonly float _previewOpacity;
    private readonly bool _isPreview;
    private readonly bool _showDebugBounds;
    private readonly bool _isHovered;
    private readonly bool _isSelected;
    private readonly SKColor _selectionColor;
    private readonly float _selectionDashLength;
    private readonly float _selectionHorizontalPadding;

    public NodeButtonComponentVisual(
        string label,
        string? fontFamily,
        float textSize,
        NodeColor textColor,
        NodeFontWeight fontWeight,
        NodeColor borderColor,
        float cornerRadius,
        float nodeUnitSize,
        float previewOpacity = 1.0f,
        bool showDebugBounds = false,
        bool isHovered = false,
        bool isSelected = false,
        bool isPreview = false,
        SKColor defaultSelectionColor = default,
        float selectionDashLength = 1.0f,
        float selectionHorizontalPadding = 0.0f)
    {
        _label = label ?? string.Empty;
        _fontFamily = fontFamily;
        _textSize = textSize;
        _textColor = textColor;
        _fontWeight = fontWeight;
        _borderColor = borderColor;
        _cornerRadius = MathF.Max(0.0f, cornerRadius);
        _nodeUnitSize = nodeUnitSize;
        _previewOpacity = Math.Clamp(previewOpacity, 0.0f, 1.0f);
        _showDebugBounds = showDebugBounds;
        _isHovered = isHovered;
        _isSelected = isSelected;
        _isPreview = isPreview;
        _selectionColor = defaultSelectionColor == default ? SKColors.Gray : defaultSelectionColor;
        _selectionDashLength = selectionDashLength;
        _selectionHorizontalPadding = MathF.Max(0.0f, selectionHorizontalPadding);
    }

    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        var bodyHeight = Bounds.Height * BodyHeightScale;
        var buttonBounds = new SKRect(
            Bounds.Left,
            Bounds.MidY - (bodyHeight / 2.0f),
            Bounds.Right,
            Bounds.MidY + (bodyHeight / 2.0f));
        var cornerRadius = _cornerRadius;
        using var fillPaint = new SKPaint
        {
            Color = _borderColor.ToSkColor().WithAlpha((byte)(_isPreview ? 26 : 60)),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var borderPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_borderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _nodeUnitSize / 4.0f,
        };
        using var textPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_textColor.ToSkColor()),
            IsAntialias = true,
            TextSize = _textSize * TextScale,
            TextAlign = SKTextAlign.Center,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };

        canvas.DrawRoundRect(buttonBounds, cornerRadius, cornerRadius, fillPaint);
        canvas.DrawRoundRect(buttonBounds, cornerRadius, cornerRadius, borderPaint);

        var textBounds = SKRect.Empty;
        textPaint.MeasureText(_label, ref textBounds);
        canvas.DrawText(_label, buttonBounds.MidX, buttonBounds.MidY - textBounds.MidY, textPaint);

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true, _selectionHorizontalPadding);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false, _selectionHorizontalPadding);
    }

    private SKColor ApplyPreviewAlpha(SKColor color)
    {
        return !_isPreview
            ? color
            : color.WithAlpha((byte)Math.Clamp(MathF.Round(color.Alpha * _previewOpacity), 0.0f, 255.0f));
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
}
