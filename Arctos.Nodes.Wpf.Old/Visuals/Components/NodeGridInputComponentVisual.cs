using Arctos.Nodes.Models;
using SkiaSharp;
using System.Globalization;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a passive grid input component inside a node definition.
/// </summary>
public sealed class NodeGridInputComponentVisual : NodeComponentVisual
{
    private const float BodyHeightScale = 0.8f;
    private const float TextScale = 0.6f;
    private const float HorizontalPaddingNodeUnits = 0.5f;
    private const float InternalGapNodeUnits = 0.5f;

    private readonly string _label;
    private readonly int _rows;
    private readonly int _columns;
    private readonly float _cellWidthNu;
    private readonly InputTypeId _cellTypeId;
    private readonly double[] _values;
    private readonly string? _fontFamily;
    private readonly float _textSize;
    private readonly NodeColor _textColor;
    private readonly NodeFontWeight _fontWeight;
    private readonly NodeColor _fieldBorderColor;
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

    public NodeGridInputComponentVisual(
        string label,
        int rows,
        int columns,
        float cellWidthNu,
        InputTypeId cellTypeId,
        double[] values,
        string? fontFamily,
        float textSize,
        NodeColor textColor,
        NodeFontWeight fontWeight,
        NodeColor fieldBorderColor,
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
        _rows = Math.Max(1, rows);
        _columns = Math.Max(1, columns);
        _cellWidthNu = MathF.Max(NodeGridInput.MinimumCellWidth, cellWidthNu);
        _cellTypeId = cellTypeId;
        _values = values ?? [];
        _fontFamily = fontFamily;
        _textSize = textSize;
        _textColor = textColor;
        _fontWeight = fontWeight;
        _fieldBorderColor = fieldBorderColor;
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

        var horizontalGap = _nodeUnitSize;
        var verticalGap = 0.0f;
        var slotHeight = Bounds.Height / _rows;
        var outerPadding = HorizontalPaddingNodeUnits * _nodeUnitSize;
        var internalGap = InternalGapNodeUnits * _nodeUnitSize;
        var bodyHeight = slotHeight * BodyHeightScale;
        var cornerRadius = (_controlCornerRadius * _nodeUnitSize) * 0.5f;

        using var fillPaint = new SKPaint
        {
            Color = _fieldBorderColor.ToSkColor().WithAlpha((byte)(_isPreview ? 26 : 60)),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var textPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_textColor.ToSkColor()),
            IsAntialias = true,
            TextSize = _textSize * TextScale,
            TextAlign = SKTextAlign.Center,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };
        using var labelPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_textColor.ToSkColor()),
            IsAntialias = true,
            TextSize = _textSize * TextScale,
            TextAlign = SKTextAlign.Left,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };

        var labelWidth = string.IsNullOrWhiteSpace(_label)
            ? 0.0f
            : labelPaint.MeasureText(_label);
        var labelLeft = Bounds.Left + outerPadding;
        var minimumGridLeft = labelLeft;
        if (labelWidth > 0.0f)
            minimumGridLeft += labelWidth + internalGap;

        var gridRight = Bounds.Right - outerPadding;
        var availableWidth = MathF.Max(0.0f, gridRight - minimumGridLeft - ((_columns - 1) * horizontalGap));
        var requestedCellWidth = _cellWidthNu * _nodeUnitSize;
        var cellWidth = MathF.Max(0.0f, MathF.Min(requestedCellWidth, availableWidth / _columns));
        var totalGridWidth = (cellWidth * _columns) + ((_columns - 1) * horizontalGap);
        var gridLeft = MathF.Max(minimumGridLeft, gridRight - totalGridWidth);

        if (labelWidth > 0.0f)
        {
            var firstRowMidY = Bounds.Top + (slotHeight * 0.5f);
            var labelBounds = SKRect.Empty;
            labelPaint.MeasureText(_label, ref labelBounds);
            canvas.DrawText(_label, labelLeft, firstRowMidY - labelBounds.MidY, labelPaint);
        }

        for (var row = 0; row < _rows; row++)
        {
            var slotTop = Bounds.Top + (row * (slotHeight + verticalGap));
            var bodyTop = slotTop + ((slotHeight - bodyHeight) * 0.5f);

            for (var column = 0; column < _columns; column++)
            {
                var left = gridLeft + (column * (cellWidth + horizontalGap));
                var cellBounds = new SKRect(left, bodyTop, left + cellWidth, bodyTop + bodyHeight);
                canvas.DrawRoundRect(cellBounds, cornerRadius, cornerRadius, fillPaint);

                var text = FormatValue(row, column);
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var textBounds = SKRect.Empty;
                textPaint.MeasureText(text, ref textBounds);
                canvas.Save();
                canvas.ClipRect(cellBounds);
                canvas.DrawText(text, cellBounds.MidX, cellBounds.MidY - textBounds.MidY, textPaint);
                canvas.Restore();
            }
        }

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true, _selectionHorizontalPadding);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false, _selectionHorizontalPadding);
    }

    private string FormatValue(int row, int column)
    {
        var index = (row * _columns) + column;
        if (index < 0 || index >= _values.Length)
            return string.Empty;

        var value = _values[index];
        return _cellTypeId == InputTypeIds.Integer
            ? Math.Round(value, MidpointRounding.AwayFromZero).ToString(CultureInfo.CurrentCulture)
            : value.ToString("0.###", CultureInfo.CurrentCulture);
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
