using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a passive input component inside a node definition.
/// </summary>
public sealed class NodeInputComponentVisual : NodeComponentVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private const float BodyHeightScale = 0.8f;
    private const float TextScale = 0.6f;
    private const float InputFieldHorizontalPaddingNodeUnits = 1.0f;
    private const float InputFieldInternalGapNodeUnits = 0.5f;

    private readonly string? _fontFamily;
    private readonly NodeFontWeight _fontWeight;
    private readonly bool _isHovered;
    private readonly bool _isPreview;
    private readonly bool _isSelected;
    private readonly string _label;
    private readonly SKColor _selectionColor;
    private readonly float _selectionDashLength;
    private readonly bool _showDebugBounds;
    private readonly NodeColor _textColor;
    private readonly float _textSize;
    private readonly float _inputLabelWidth;
    private readonly InputTypeId _typeId;
    private readonly object? _value;
    private readonly NodeColor _fieldBorderColor;
    private readonly NodeColor _fieldBackgroundColor;
    private readonly float _controlCornerRadius;
    private readonly float _nodeUnitSize;
    private readonly float _previewOpacity;
    private readonly float? _labelValueGapNodeUnits;
    private readonly float _fieldStrokeWidth;
    private readonly float _selectionHorizontalPadding;
    private readonly DateTimeDisplayMode _dateTimeDisplayMode;
    private readonly NodeColor? _valueTextColor;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public NodeInputComponentVisual(
        string label,
        InputTypeId typeId,
        object? value,
        string? fontFamily,
        float textSize,
        NodeColor textColor,
        NodeFontWeight fontWeight,
        NodeColor fieldBorderColor,
        NodeColor fieldBackgroundColor,
        float controlCornerRadius,
        float nodeUnitSize,
        float inputLabelWidth,
        DateTimeDisplayMode dateTimeDisplayMode = DateTimeDisplayMode.Both,
        NodeColor? valueTextColor = null,
        float? labelValueGapNodeUnits = null,
        float fieldStrokeWidth = 0.0f,
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
        _typeId = typeId;
        _value = value;
        _fontFamily = fontFamily;
        _textSize = textSize;
        _textColor = textColor;
        _fontWeight = fontWeight;
        _fieldBorderColor = fieldBorderColor;
        _fieldBackgroundColor = fieldBackgroundColor;
        _controlCornerRadius = MathF.Max(0.0f, controlCornerRadius);
        _nodeUnitSize = nodeUnitSize;
        _inputLabelWidth = inputLabelWidth;
        _dateTimeDisplayMode = dateTimeDisplayMode;
        _valueTextColor = valueTextColor;
        _labelValueGapNodeUnits = labelValueGapNodeUnits;
        _fieldStrokeWidth = Math.Max(0.0f, fieldStrokeWidth);
        _showDebugBounds = showDebugBounds;
        _isHovered = isHovered;
        _isSelected = isSelected;
        _isPreview = isPreview;
        _previewOpacity = Math.Clamp(previewOpacity, 0.0f, 1.0f);
        _selectionColor = defaultSelectionColor == default ? SKColors.Gray : defaultSelectionColor;
        _selectionDashLength = selectionDashLength;
        _selectionHorizontalPadding = MathF.Max(0.0f, selectionHorizontalPadding);
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var textPaint = CreateTextPaint();
        var fieldHeight = Bounds.Height * BodyHeightScale;
        var fieldBounds = new SKRect(
            Bounds.Left,
            Bounds.MidY - (fieldHeight / 2.0f),
            Bounds.Right,
            Bounds.MidY + (fieldHeight / 2.0f));

        if (_typeId == InputTypeIds.Boolean)
            RenderBooleanValue(canvas, fieldBounds, textPaint);
        else
            RenderBoxValue(canvas, fieldBounds, textPaint, _typeId == InputTypeIds.StringSelection);

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true, _selectionHorizontalPadding);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false, _selectionHorizontalPadding);
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void RenderBooleanValue(SKCanvas canvas, SKRect valueBounds, SKPaint textPaint)
    {
        var fieldColor = _fieldBorderColor.ToSkColor();
        var horizontalPadding = InputFieldHorizontalPaddingNodeUnits * _nodeUnitSize;
        var internalGap = InputFieldInternalGapNodeUnits * _nodeUnitSize;
        var fieldCornerRadius = (_controlCornerRadius * _nodeUnitSize) * 0.5f;
        var boxCornerRadius = fieldCornerRadius;

        using var fillPaint = new SKPaint
        {
            Color = fieldColor.WithAlpha((byte)(_isPreview ? 26 : 60)),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        canvas.DrawRoundRect(valueBounds, fieldCornerRadius, fieldCornerRadius, fillPaint);

        var size = MathF.Min(valueBounds.Height * 0.56f, valueBounds.Width);
        var boxBounds = new SKRect(
            valueBounds.Right - horizontalPadding - size,
            valueBounds.MidY - (size / 2.0f),
            valueBounds.Right - horizontalPadding,
            valueBounds.MidY + (size / 2.0f));
        var labelLeft = valueBounds.Left + horizontalPadding;
        var labelRight = MathF.Max(labelLeft, boxBounds.Left - internalGap);

        using var labelClipPath = new SKPath();
        labelClipPath.AddRect(new SKRect(labelLeft, valueBounds.Top, labelRight, valueBounds.Bottom));
        canvas.Save();
        canvas.ClipPath(labelClipPath, antialias: false);
        if (!string.IsNullOrWhiteSpace(_label))
        {
            var labelBounds = SKRect.Empty;
            textPaint.MeasureText(_label, ref labelBounds);
            canvas.DrawText(_label, labelLeft, valueBounds.MidY - labelBounds.MidY, textPaint);
        }
        canvas.Restore();

        using var borderPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_fieldBorderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = MathF.Max(_nodeUnitSize / 4.0f, 1.0f),
        };

        using var boxFillPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_fieldBackgroundColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        canvas.DrawRoundRect(boxBounds, boxCornerRadius, boxCornerRadius, boxFillPaint);
        canvas.DrawRoundRect(boxBounds, boxCornerRadius, boxCornerRadius, borderPaint);

        if (_value is not bool booleanValue || !booleanValue)
            return;

        var innerSize = size * 0.5f;
        var innerSquareBounds = new SKRect(
            boxBounds.MidX - (innerSize / 2.0f),
            boxBounds.MidY - (innerSize / 2.0f),
            boxBounds.MidX + (innerSize / 2.0f),
            boxBounds.MidY + (innerSize / 2.0f));

        using var checkPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_fieldBorderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        canvas.DrawRoundRect(innerSquareBounds, MathF.Min(boxCornerRadius, innerSize / 6.0f), MathF.Min(boxCornerRadius, innerSize / 6.0f), checkPaint);
    }

    private void RenderBoxValue(SKCanvas canvas, SKRect valueBounds, SKPaint textPaint, bool showDropdownArrow)
    {
        if (valueBounds.Width <= 0.0f)
            return;

        var fieldColor = _fieldBorderColor.ToSkColor();
        var horizontalPadding = InputFieldHorizontalPaddingNodeUnits * _nodeUnitSize;
        var internalGap = InputFieldInternalGapNodeUnits * _nodeUnitSize;
        var fieldCornerRadius = (_controlCornerRadius * _nodeUnitSize) * 0.5f;

        if (showDropdownArrow)
        {
            using var borderPaint = new SKPaint
            {
                Color = ApplyPreviewAlpha(fieldColor),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.0f,
            };

            canvas.DrawRoundRect(valueBounds, fieldCornerRadius, fieldCornerRadius, borderPaint);
        }
        else
        {
            using var fillPaint = new SKPaint
            {
                Color = fieldColor.WithAlpha((byte)(_isPreview ? 26 : 60)),
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
            };

            canvas.DrawRoundRect(valueBounds, fieldCornerRadius, fieldCornerRadius, fillPaint);
        }

        var valueText = FormatValue(_value, _dateTimeDisplayMode);
        var labelText = _label ?? string.Empty;
        var labelBounds = SKRect.Empty;
        var valueTextBounds = SKRect.Empty;
        textPaint.MeasureText(labelText, ref labelBounds);
        textPaint.MeasureText(valueText, ref valueTextBounds);

        var textRight = valueBounds.Right - horizontalPadding;
        var valueRight = textRight;
        var arrowLeft = textRight;

        if (showDropdownArrow)
        {
            DrawDropdownArrow(canvas, valueBounds, horizontalPadding);
            var arrowReserve = horizontalPadding * 1.8f;
            arrowLeft = valueBounds.Right - arrowReserve;
            valueRight = arrowLeft - internalGap;
        }

        var labelLeft = valueBounds.Left + horizontalPadding;
        var labelClipRight = MathF.Max(labelLeft, valueRight - MathF.Max(0.0f, valueTextBounds.Width) - internalGap);
        var valueClipLeft = MathF.Min(valueRight, labelClipRight + internalGap);
        var baseline = valueBounds.MidY - valueTextBounds.MidY;

        using var labelClipPath = new SKPath();
        labelClipPath.AddRect(new SKRect(labelLeft, valueBounds.Top, labelClipRight, valueBounds.Bottom));
        canvas.Save();
        canvas.ClipPath(labelClipPath, antialias: false);
        if (!string.IsNullOrWhiteSpace(labelText))
            canvas.DrawText(labelText, labelLeft, valueBounds.MidY - labelBounds.MidY, textPaint);
        canvas.Restore();

        using var valueClipPath = new SKPath();
        valueClipPath.AddRect(new SKRect(valueClipLeft, valueBounds.Top, valueRight, valueBounds.Bottom));
        using var rightAlignedTextPaint = CreateRightAlignedTextPaint(textPaint, _valueTextColor.HasValue ? ApplyPreviewAlpha(_valueTextColor.Value.ToSkColor()) : null);
        canvas.Save();
        canvas.ClipPath(valueClipPath, antialias: false);
        canvas.DrawText(valueText, valueRight, baseline, rightAlignedTextPaint);
        canvas.Restore();
    }

    private void DrawDropdownArrow(SKCanvas canvas, SKRect valueBounds, float horizontalPadding)
    {
        using var arrowPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_textColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        var arrowWidth = _textSize * 0.225f;
        var centerX = valueBounds.Right - (_nodeUnitSize * 0.5f) - arrowWidth;
        var centerY = valueBounds.MidY;

        using var arrowPath = new SKPath();
        arrowPath.MoveTo(centerX - arrowWidth, centerY - (arrowWidth * 0.35f));
        arrowPath.LineTo(centerX + arrowWidth, centerY - (arrowWidth * 0.35f));
        arrowPath.LineTo(centerX, centerY + (arrowWidth * 0.55f));
        arrowPath.Close();
        canvas.DrawPath(arrowPath, arrowPaint);
    }

    private SKPaint CreateTextPaint()
    {
        return new SKPaint
        {
            Color = ApplyPreviewAlpha(_textColor.ToSkColor()),
            IsAntialias = true,
            TextSize = _textSize * TextScale,
            TextAlign = SKTextAlign.Left,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };
    }

    private static SKPaint CreateRightAlignedTextPaint(SKPaint source, SKColor? colorOverride = null)
    {
        return new SKPaint
        {
            Color = colorOverride ?? source.Color,
            IsAntialias = source.IsAntialias,
            TextSize = source.TextSize,
            TextAlign = SKTextAlign.Right,
            Typeface = source.Typeface,
        };
    }

    private SKColor ApplyPreviewAlpha(SKColor color)
    {
        return !_isPreview
            ? color
            : color.WithAlpha((byte)Math.Clamp(MathF.Round(color.Alpha * _previewOpacity), 0.0f, 255.0f));
    }

    private static string FormatValue(object? value, DateTimeDisplayMode dateTimeDisplayMode)
    {
        return value switch
        {
            null => string.Empty,
            bool booleanValue => booleanValue ? "True" : "False",
            StringSelectionValue selection => selection.Value ?? string.Empty,
            DateTime dateTime => FormatDateTime(dateTime, dateTimeDisplayMode),
            TimeSpan timeSpan => timeSpan.ToString(),
            _ => value.ToString() ?? string.Empty,
        };
    }

    private static string FormatDateTime(DateTime value, DateTimeDisplayMode mode)
    {
        return mode switch
        {
            DateTimeDisplayMode.DateOnly => value.ToString("d"),
            DateTimeDisplayMode.TimeOnly => value.ToString("t"),
            _ => value.ToString("g"),
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
