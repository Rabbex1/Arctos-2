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

    private const float InputFieldHeightNodeStrokeUnits = 14.0f;
    private const float InputFieldHorizontalPaddingNodeUnits = 1.0f;

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
    private readonly float _nodeUnitSize;
    private readonly float _previewOpacity;

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
        float nodeUnitSize,
        float inputLabelWidth,
        bool showDebugBounds = false,
        bool isHovered = false,
        bool isSelected = false,
        bool isPreview = false,
        float previewOpacity = 0.6f,
        SKColor defaultSelectionColor = default,
        float selectionDashLength = 1.0f)
    {
        _label = label ?? string.Empty;
        _typeId = typeId;
        _value = value;
        _fontFamily = fontFamily;
        _textSize = textSize;
        _textColor = textColor;
        _fontWeight = fontWeight;
        _fieldBorderColor = fieldBorderColor;
        _nodeUnitSize = nodeUnitSize;
        _inputLabelWidth = inputLabelWidth;
        _showDebugBounds = showDebugBounds;
        _isHovered = isHovered;
        _isSelected = isSelected;
        _isPreview = isPreview;
        _previewOpacity = Math.Clamp(previewOpacity, 0.0f, 1.0f);
        _selectionColor = defaultSelectionColor == default ? SKColors.Gray : defaultSelectionColor;
        _selectionDashLength = selectionDashLength;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var textPaint = CreateTextPaint();
        var labelBounds = SKRect.Empty;
        textPaint.MeasureText(_label, ref labelBounds);
        var baseline = Bounds.MidY - labelBounds.MidY;

        if (!string.IsNullOrWhiteSpace(_label))
            canvas.DrawText(_label, Bounds.Left, baseline, textPaint);

        var gap = _textSize * 0.5f;
        var labelWidth = MathF.Max(labelBounds.Width, _inputLabelWidth);
        var valueLeft = string.IsNullOrWhiteSpace(_label) && _inputLabelWidth <= 0.0f
            ? Bounds.Left
            : Bounds.Left + labelWidth + gap;
        var fieldHeight = MathF.Min(Bounds.Height, (InputFieldHeightNodeStrokeUnits / 4.0f) * _nodeUnitSize);
        var valueBounds = new SKRect(
            MathF.Min(valueLeft, Bounds.Right),
            Bounds.MidY - (fieldHeight / 2.0f),
            Bounds.Right,
            Bounds.MidY + (fieldHeight / 2.0f));

        if (_typeId == InputTypeIds.Boolean)
            RenderBooleanValue(canvas, valueBounds);
        else
            RenderBoxValue(canvas, valueBounds, textPaint, _typeId == InputTypeIds.StringSelection);

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

    private void RenderBooleanValue(SKCanvas canvas, SKRect valueBounds)
    {
        var size = MathF.Min(valueBounds.Height * 0.7f, valueBounds.Width);
        var boxBounds = new SKRect(
            valueBounds.Left,
            valueBounds.MidY - (size / 2.0f),
            valueBounds.Left + size,
            valueBounds.MidY + (size / 2.0f));

        using var fillPaint = new SKPaint
        {
            Color = _fieldBorderColor.ToSkColor().WithAlpha((byte)(_isPreview ? 26 : 40)),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var borderPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_fieldBorderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
        };

        canvas.DrawRect(boxBounds, fillPaint);
        canvas.DrawRect(boxBounds, borderPaint);

        if (_value is not bool booleanValue || !booleanValue)
            return;

        using var checkPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_textColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = MathF.Max(1.0f, size * 0.12f),
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
        };

        using var checkPath = new SKPath();
        checkPath.MoveTo(boxBounds.Left + (size * 0.18f), boxBounds.MidY + (size * 0.02f));
        checkPath.LineTo(boxBounds.Left + (size * 0.42f), boxBounds.Bottom - (size * 0.22f));
        checkPath.LineTo(boxBounds.Right - (size * 0.14f), boxBounds.Top + (size * 0.2f));
        canvas.DrawPath(checkPath, checkPaint);
    }

    private void RenderBoxValue(SKCanvas canvas, SKRect valueBounds, SKPaint textPaint, bool showDropdownArrow)
    {
        if (valueBounds.Width <= 0.0f)
            return;

        using var fillPaint = new SKPaint
        {
            Color = _fieldBorderColor.ToSkColor().WithAlpha((byte)(_isPreview ? 26 : 40)),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var borderPaint = new SKPaint
        {
            Color = ApplyPreviewAlpha(_fieldBorderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
        };

        canvas.DrawRect(valueBounds, fillPaint);
        canvas.DrawRect(valueBounds, borderPaint);

        var valueText = FormatValue(_value);
        var textBounds = SKRect.Empty;
        textPaint.MeasureText(valueText, ref textBounds);
        var horizontalPadding = InputFieldHorizontalPaddingNodeUnits * _nodeUnitSize;
        var textLeft = valueBounds.Left + horizontalPadding;
        var textRight = valueBounds.Right - horizontalPadding;

        if (showDropdownArrow)
        {
            DrawDropdownArrow(canvas, valueBounds, horizontalPadding);
            textRight -= horizontalPadding * 1.6f;
        }

        using var clipPath = new SKPath();
        clipPath.AddRect(new SKRect(textLeft, valueBounds.Top, textRight, valueBounds.Bottom));
        canvas.Save();
        canvas.ClipPath(clipPath, antialias: false);
        canvas.DrawText(valueText, textLeft, valueBounds.MidY - textBounds.MidY, textPaint);
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
            TextSize = _textSize,
            TextAlign = SKTextAlign.Left,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };
    }

    private SKColor ApplyPreviewAlpha(SKColor color)
    {
        return !_isPreview
            ? color
            : color.WithAlpha((byte)Math.Clamp(MathF.Round(color.Alpha * _previewOpacity), 0.0f, 255.0f));
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            bool booleanValue => booleanValue ? "True" : "False",
            StringSelectionValue selection => selection.Value ?? string.Empty,
            DateTime dateTime => dateTime.ToString("g"),
            TimeSpan timeSpan => timeSpan.ToString(),
            _ => value.ToString() ?? string.Empty,
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
