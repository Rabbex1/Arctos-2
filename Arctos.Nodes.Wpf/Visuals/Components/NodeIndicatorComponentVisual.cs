using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a passive indicator component inside a node definition.
/// </summary>
public sealed class NodeIndicatorComponentVisual : NodeComponentVisual
{
    private const float TextScale = 0.6f;
    private const float HorizontalPaddingNodeUnits = 1.0f;
    private const float InternalGapNodeUnits = 0.5f;
    private const float IndicatorSizeScale = 0.6f;

    private readonly string _label;
    private readonly NodeIndicatorShape _shape;
    private readonly NodeColor _color;
    private readonly NodeColor _borderColor;
    private readonly string? _fontFamily;
    private readonly float _textSize;
    private readonly NodeColor _textColor;
    private readonly NodeFontWeight _fontWeight;
    private readonly float _controlCornerRadius;
    private readonly float _nodeUnitSize;
    private readonly bool _showDebugBounds;
    private readonly bool _isHovered;
    private readonly bool _isSelected;
    private readonly bool _isPreview;
    private readonly float _previewOpacity;
    private readonly SKColor _selectionColor;
    private readonly float _selectionDashLength;
    private readonly float _selectionHorizontalPadding;

    public NodeIndicatorComponentVisual(
        string label,
        NodeIndicatorShape shape,
        NodeColor color,
        NodeColor borderColor,
        string? fontFamily,
        float textSize,
        NodeColor textColor,
        NodeFontWeight fontWeight,
        float controlCornerRadius,
        float nodeUnitSize,
        bool showDebugBounds = false,
        bool isHovered = false,
        bool isSelected = false,
        bool isPreview = false,
        float previewOpacity = 0.6f,
        SKColor defaultSelectionColor = default,
        float selectionDashLength = 1.0f,
        float selectionHorizontalPadding = 0.0f)
    {
        _label = label ?? string.Empty;
        _shape = shape;
        _color = color;
        _borderColor = borderColor;
        _fontFamily = fontFamily;
        _textSize = textSize;
        _textColor = textColor;
        _fontWeight = fontWeight;
        _controlCornerRadius = MathF.Max(0.0f, controlCornerRadius);
        _nodeUnitSize = nodeUnitSize;
        _showDebugBounds = showDebugBounds;
        _isHovered = isHovered;
        _isSelected = isSelected;
        _isPreview = isPreview;
        _previewOpacity = Math.Clamp(previewOpacity, 0.0f, 1.0f);
        _selectionColor = defaultSelectionColor == default ? SKColors.Gray : defaultSelectionColor;
        _selectionDashLength = selectionDashLength;
        _selectionHorizontalPadding = MathF.Max(0.0f, selectionHorizontalPadding);
    }

    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        var horizontalPadding = HorizontalPaddingNodeUnits * _nodeUnitSize;
        var internalGap = InternalGapNodeUnits * _nodeUnitSize;
        var indicatorSize = Bounds.Height * IndicatorSizeScale;
        var borderStrokeWidth = _nodeUnitSize / 4.0f;
        var indicatorBounds = new SKRect(
            Bounds.Right - horizontalPadding - indicatorSize,
            Bounds.MidY - (indicatorSize * 0.5f),
            Bounds.Right - horizontalPadding,
            Bounds.MidY + (indicatorSize * 0.5f));

        using var fillPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_color.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var borderPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_borderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = borderStrokeWidth,
        };
        using var textPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_textColor.ToSkColor()),
            IsAntialias = true,
            TextSize = _textSize * TextScale,
            TextAlign = SKTextAlign.Left,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };

        DrawIndicator(canvas, indicatorBounds, fillPaint, borderPaint);

        if (!string.IsNullOrWhiteSpace(_label))
        {
            var labelLeft = Bounds.Left + horizontalPadding;
            var labelRight = MathF.Max(labelLeft, indicatorBounds.Left - internalGap);
            var labelBounds = SKRect.Empty;
            textPaint.MeasureText(_label, ref labelBounds);

            using var labelClipPath = new SKPath();
            labelClipPath.AddRect(new SKRect(labelLeft, Bounds.Top, labelRight, Bounds.Bottom));
            canvas.Save();
            canvas.ClipPath(labelClipPath, antialias: false);
            canvas.DrawText(_label, labelLeft, Bounds.MidY - labelBounds.MidY, textPaint);
            canvas.Restore();
        }

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true, _selectionHorizontalPadding);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false, _selectionHorizontalPadding);
    }

    private void DrawIndicator(SKCanvas canvas, SKRect bounds, SKPaint fillPaint, SKPaint borderPaint)
    {
        switch (_shape)
        {
            case NodeIndicatorShape.Round:
                canvas.DrawOval(bounds, fillPaint);
                canvas.DrawOval(bounds, borderPaint);
                break;

            case NodeIndicatorShape.Rectangle:
                var cornerRadius = (_controlCornerRadius * _nodeUnitSize) * 0.5f;
                canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, fillPaint);
                canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, borderPaint);
                break;
        }
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
