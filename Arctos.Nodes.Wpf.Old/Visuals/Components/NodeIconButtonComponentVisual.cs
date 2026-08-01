using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Rendering;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a passive icon button component inside a node definition.
/// </summary>
public sealed class NodeIconButtonComponentVisual : NodeComponentVisual
{
    private const float BodyHeightScale = 0.8f;
    private const float IconSizeScale = 0.68f;

    private readonly string? _svg;
    private readonly NodeIconButtonColorMode _colorMode;
    private readonly NodeColor _backgroundColor;
    private readonly NodeColor _borderColor;
    private readonly NodeColor _textColor;
    private readonly float _cornerRadius;
    private readonly float _nodeUnitSize;
    private readonly float _previewOpacity;
    private readonly bool _showDebugBounds;
    private readonly bool _isHovered;
    private readonly bool _isSelected;
    private readonly bool _isPreview;
    private readonly SKColor _selectionColor;
    private readonly float _selectionDashLength;
    private readonly float _selectionHorizontalPadding;

    public NodeIconButtonComponentVisual(
        string? svg,
        NodeIconButtonColorMode colorMode,
        NodeColor backgroundColor,
        NodeColor borderColor,
        NodeColor textColor,
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
        _svg = svg;
        _colorMode = colorMode;
        _backgroundColor = backgroundColor;
        _borderColor = borderColor;
        _textColor = textColor;
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
        var iconSize = MathF.Min(buttonBounds.Width, buttonBounds.Height) * IconSizeScale;
        var iconBounds = new SKRect(
            buttonBounds.MidX - (iconSize / 2.0f),
            buttonBounds.MidY - (iconSize / 2.0f),
            buttonBounds.MidX + (iconSize / 2.0f),
            buttonBounds.MidY + (iconSize / 2.0f));

        using var fillPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_backgroundColor.ToSkColor()),
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

        canvas.DrawRoundRect(buttonBounds, cornerRadius, cornerRadius, fillPaint);
        canvas.DrawRoundRect(buttonBounds, cornerRadius, cornerRadius, borderPaint);

        var useNodeColors = _colorMode != NodeIconButtonColorMode.Svg;
        var iconColor = _colorMode == NodeIconButtonColorMode.Text
            ? _textColor.ToSkColor()
            : _borderColor.ToSkColor();
        if (!SvgIconRenderer.Render(canvas, _svg, iconBounds, useNodeColors, iconColor, _isPreview ? _previewOpacity : 1.0f))
            RenderFallbackGlyph(canvas, iconBounds, borderPaint.Color);

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true, _selectionHorizontalPadding);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false, _selectionHorizontalPadding);
    }

    private void RenderFallbackGlyph(SKCanvas canvas, SKRect iconBounds, SKColor color)
    {
        using var paint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _nodeUnitSize / 4.0f,
        };

        canvas.DrawRect(iconBounds, paint);
        canvas.DrawLine(iconBounds.Left, iconBounds.Top, iconBounds.Right, iconBounds.Bottom, paint);
        canvas.DrawLine(iconBounds.Left, iconBounds.Bottom, iconBounds.Right, iconBounds.Top, paint);
    }

    private SKColor ApplyPreviewAlpha(SKColor color)
    {
        return !_isPreview
            ? color
            : color.WithAlpha((byte)Math.Clamp(MathF.Round(color.Alpha * _previewOpacity), 0.0f, 255.0f));
    }
}
