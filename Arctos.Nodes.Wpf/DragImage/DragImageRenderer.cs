using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Nodes.Wpf.Visuals;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.DragImage;

/// <summary>
/// Renders fixed-size drag image artifacts for node-editor components using the current document visual language.
/// </summary>
public sealed class DragImageRenderer
{
    private const float GridPadding = 2.0f;

    /// <summary>
    /// Renders a drag image for the supplied component.
    /// </summary>
    /// <param name="component">The component to render.</param>
    /// <param name="styleContext">The effective document canvas style.</param>
    /// <param name="nodeStyle">The effective node style.</param>
    /// <param name="options">Optional render options.</param>
    /// <returns>The rendered drag image artifact.</returns>
    public DragImage RenderComponent(NodeComponentVM component, NodeCanvasStyleContext styleContext, NodeStyle nodeStyle, DragImageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(styleContext);
        ArgumentNullException.ThrowIfNull(nodeStyle);

        options ??= new DragImageOptions();

        var mainGridSize = MeasureMainGridSize(styleContext);
        var minorGridSize = mainGridSize / 4.0f;
        var textHeight = MeasureTextHeight(styleContext.FontFamily, styleContext.FontSize, NodeFontWeight.Normal);
        var baseScale = options.TargetTextHeightPx / MathF.Max(1.0f, textHeight);
        var targetLogicalWidth = options.MaxWidthPx / MathF.Max(0.01f, baseScale);
        var visual = CreateComponentVisual(component, styleContext, nodeStyle, minorGridSize, targetLogicalWidth);
        var bounds = visual.Bounds;
        var scale = ClampScale(bounds, baseScale, options);
        var widthPx = Math.Max(1, options.MaxWidthPx);
        var heightPx = Math.Max(1, options.MaxHeightPx);

        using var bitmap = new SKBitmap(widthPx, heightPx, false);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        var scaledWidth = bounds.Width * scale;
        var scaledHeight = bounds.Height * scale;
        var offsetXPx = 0.0f;
        var offsetYPx = MathF.Max(0.0f, (heightPx - scaledHeight) * 0.5f);

        canvas.Translate(offsetXPx, offsetYPx);
        canvas.Scale(scale, scale);
        canvas.Translate(-bounds.Left, -bounds.Top);
        visual.Render(canvas);

        ApplyFade(bitmap, options);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return new DragImage(
            data.ToArray(),
            bitmap.Width,
            bitmap.Height,
            0,
            0);
    }

    private static NodeComponentVisual CreateComponentVisual(NodeComponentVM component, NodeCanvasStyleContext styleContext, NodeStyle nodeStyle, float minorGridSize, float targetLogicalWidth)
    {
        var fontWeight = nodeStyle.ShapeStyle.FontWeight;
        var textColor = styleContext.FontColor;
        var textHeight = MeasureTextHeight(styleContext.FontFamily, styleContext.FontSize, fontWeight);
        var lineHeight = MathF.Ceiling(textHeight + (GridPadding * 2.0f));
        var separatorWidth = MathF.Max(styleContext.MinimumColumnWidth, 12.0f) * minorGridSize;

        return component switch
        {
            NodeLabelComponentVM labelComponent => CreateLabelVisual(
                labelComponent.Label,
                styleContext.FontFamily,
                styleContext.FontSize,
                textColor,
                fontWeight,
                lineHeight),

            NodeInputComponentVM inputComponent => CreateInputVisual(
                inputComponent,
                styleContext,
                nodeStyle,
                minorGridSize,
                targetLogicalWidth,
                lineHeight),

            NodeValueDisplayComponentVM valueDisplayComponent => CreateValueDisplayVisual(
                valueDisplayComponent,
                styleContext,
                nodeStyle,
                minorGridSize,
                targetLogicalWidth,
                lineHeight),

            NodeButtonComponentVM buttonComponent => CreateButtonVisual(
                buttonComponent,
                styleContext,
                nodeStyle,
                minorGridSize,
                lineHeight),

            NodeIconButtonComponentVM iconButtonComponent => CreateIconButtonVisual(
                iconButtonComponent,
                styleContext,
                nodeStyle,
                minorGridSize,
                lineHeight),

            NodeIndicatorComponentVM indicatorComponent => CreateIndicatorVisual(
                indicatorComponent,
                styleContext,
                nodeStyle,
                minorGridSize,
                targetLogicalWidth,
                lineHeight),

            NodePortComponentVM portComponent => CreatePortVisual(
                portComponent,
                styleContext,
                nodeStyle,
                minorGridSize,
                targetLogicalWidth,
                lineHeight),

            NodeSpacerComponentVM => CreateSpacerVisual(
                textColor,
                mainGridSize: lineHeight,
                strokeWidth: minorGridSize / 4.0f),

            NodeSeparatorComponentVM separatorComponent => CreateSeparatorVisual(
                separatorComponent.ColorMode,
                nodeStyle,
                styleContext,
                minorGridSize,
                separatorWidth),

            _ => CreateLabelVisual(
                component.ComponentTypeName,
                styleContext.FontFamily,
                styleContext.FontSize,
                textColor,
                fontWeight,
                lineHeight),
        };
    }

    private static NodeLabelComponentVisual CreateLabelVisual(string? text, string? fontFamily, float fontSize, NodeColor textColor, NodeFontWeight fontWeight, float lineHeight)
    {
        var label = text ?? string.Empty;
        var width = MeasureTextWidth(fontFamily, fontSize, fontWeight, label);
        var visual = new NodeLabelComponentVisual(label, fontFamily, fontSize, textColor, fontWeight)
        {
            Bounds = new SKRect(0.0f, 0.0f, MathF.Max(1.0f, width), MathF.Max(1.0f, lineHeight)),
        };

        return visual;
    }

    private static NodeInputComponentVisual CreateInputVisual(
        NodeInputComponentVM inputComponent,
        NodeCanvasStyleContext styleContext,
        NodeStyle nodeStyle,
        float minorGridSize,
        float targetLogicalWidth,
        float lineHeight)
    {
        var visual = new NodeInputComponentVisual(
            string.IsNullOrWhiteSpace(inputComponent.Label) ? "Input" : inputComponent.Label,
            inputComponent.TypeId,
            inputComponent.DefaultValue,
            styleContext.FontFamily,
            styleContext.FontSize,
            styleContext.FontColor,
            nodeStyle.ShapeStyle.FontWeight,
            nodeStyle.ShapeStyle.BorderColor,
            nodeStyle.ShapeStyle.BackgroundColor,
            styleContext.ControlCornerRadius,
            minorGridSize,
            inputLabelWidth: 0.0f,
            dateTimeDisplayMode: inputComponent.DateTimeDisplayMode,
            labelValueGapNodeUnits: 1.0f,
            fieldStrokeWidth: minorGridSize / 2.0f)
        {
            Bounds = new SKRect(0.0f, 0.0f, MathF.Max(1.0f, targetLogicalWidth), MathF.Max(1.0f, lineHeight)),
        };

        return visual;
    }

    private static NodePortComponentVisual CreatePortVisual(
        NodePortComponentVM portComponent,
        NodeCanvasStyleContext styleContext,
        NodeStyle nodeStyle,
        float minorGridSize,
        float targetLogicalWidth,
        float lineHeight)
    {
        var visual = new NodePortComponentVisual(
            string.IsNullOrWhiteSpace(portComponent.Label) ? "Port" : portComponent.Label,
            NodePortSide.Left,
            portComponent.Shape,
            NodePortPlacement.Inside,
            styleContext.PortSize,
            styleContext.FontFamily,
            styleContext.FontSize,
            styleContext.FontColor,
            nodeStyle.ShapeStyle.FontWeight,
            portComponent.FillColor,
            nodeStyle.ShapeStyle.BorderColor,
            nodeStyle.ShapeStyle.BorderThickness,
            minorGridSize,
            minorGridSize / 4.0f,
            0.0f,
            MathF.Max(1.0f, targetLogicalWidth))
        {
            Bounds = new SKRect(0.0f, 0.0f, MathF.Max(1.0f, targetLogicalWidth), MathF.Max(1.0f, lineHeight)),
        };

        return visual;
    }

    private static NodeInputComponentVisual CreateValueDisplayVisual(
        NodeValueDisplayComponentVM valueDisplayComponent,
        NodeCanvasStyleContext styleContext,
        NodeStyle nodeStyle,
        float minorGridSize,
        float targetLogicalWidth,
        float lineHeight)
    {
        var visual = new NodeInputComponentVisual(
            string.IsNullOrWhiteSpace(valueDisplayComponent.Label) ? "Value Display" : valueDisplayComponent.Label,
            InputTypeIds.String,
            valueDisplayComponent.DefaultValue,
            styleContext.FontFamily,
            styleContext.FontSize,
            styleContext.FontColor,
            nodeStyle.ShapeStyle.FontWeight,
            nodeStyle.ShapeStyle.BackgroundColor,
            nodeStyle.ShapeStyle.BackgroundColor,
            styleContext.ControlCornerRadius,
            minorGridSize,
            inputLabelWidth: 0.0f,
            valueTextColor: valueDisplayComponent.ValueColorMode == NodeValueDisplayColorMode.Custom
                ? valueDisplayComponent.ValueColor
                : null,
            labelValueGapNodeUnits: 1.0f,
            fieldStrokeWidth: minorGridSize / 2.0f)
        {
            Bounds = new SKRect(0.0f, 0.0f, MathF.Max(1.0f, targetLogicalWidth), MathF.Max(1.0f, lineHeight)),
        };

        return visual;
    }

    private static NodeButtonComponentVisual CreateButtonVisual(
        NodeButtonComponentVM buttonComponent,
        NodeCanvasStyleContext styleContext,
        NodeStyle nodeStyle,
        float minorGridSize,
        float lineHeight)
    {
        var width = MeasureButtonWidth(buttonComponent.Label, styleContext, minorGridSize) * minorGridSize;
        var visual = new NodeButtonComponentVisual(
            string.IsNullOrWhiteSpace(buttonComponent.Label) ? "Button" : buttonComponent.Label,
            styleContext.FontFamily,
            styleContext.FontSize,
            styleContext.FontColor,
            nodeStyle.ShapeStyle.FontWeight,
            nodeStyle.ShapeStyle.BorderColor,
            styleContext.ControlCornerRadius * minorGridSize,
            minorGridSize,
            previewOpacity: 1.0f,
            isPreview: true)
        {
            Bounds = new SKRect(0.0f, 0.0f, MathF.Max(1.0f, width), MathF.Max(1.0f, lineHeight)),
        };

        return visual;
    }

    private static NodeIndicatorComponentVisual CreateIndicatorVisual(
        NodeIndicatorComponentVM indicatorComponent,
        NodeCanvasStyleContext styleContext,
        NodeStyle nodeStyle,
        float minorGridSize,
        float targetLogicalWidth,
        float lineHeight)
    {
        var visual = new NodeIndicatorComponentVisual(
            string.IsNullOrWhiteSpace(indicatorComponent.Label) ? "Indicator" : indicatorComponent.Label,
            indicatorComponent.Shape,
            indicatorComponent.Color,
            nodeStyle.ShapeStyle.BorderColor,
            styleContext.FontFamily,
            styleContext.FontSize,
            styleContext.FontColor,
            nodeStyle.ShapeStyle.FontWeight,
            styleContext.ControlCornerRadius,
            minorGridSize,
            isPreview: true,
            previewOpacity: 1.0f)
        {
            Bounds = new SKRect(0.0f, 0.0f, MathF.Max(1.0f, targetLogicalWidth), MathF.Max(1.0f, lineHeight)),
        };

        return visual;
    }

    private static NodeIconButtonComponentVisual CreateIconButtonVisual(
        NodeIconButtonComponentVM iconButtonComponent,
        NodeCanvasStyleContext styleContext,
        NodeStyle nodeStyle,
        float minorGridSize,
        float lineHeight)
    {
        var width = MathF.Max(1.0f, (iconButtonComponent.Model.FixedWidth > 0.0f ? iconButtonComponent.Model.FixedWidth : NodeIconButton.DefaultWidthNu) * minorGridSize);
        var visual = new NodeIconButtonComponentVisual(
            iconButtonComponent.Svg,
            iconButtonComponent.ColorMode,
            nodeStyle.ShapeStyle.BackgroundColor,
            nodeStyle.ShapeStyle.BorderColor,
            styleContext.FontColor,
            styleContext.ControlCornerRadius * minorGridSize,
            minorGridSize,
            previewOpacity: 1.0f,
            isPreview: true)
        {
            Bounds = new SKRect(0.0f, 0.0f, width, MathF.Max(1.0f, lineHeight)),
        };

        return visual;
    }

    private static NodeComponentVisual CreateSpacerVisual(NodeColor color, float mainGridSize, float strokeWidth)
    {
        var height = MathF.Max(1.0f, mainGridSize);
        var width = MathF.Max(1.0f, height * 0.8f);
        return new SpacerDragImageVisual(color.ToSkColor(), strokeWidth)
        {
            Bounds = new SKRect(0.0f, 0.0f, width, height),
        };
    }

    private static NodeSeparatorComponentVisual CreateSeparatorVisual(NodeSeparatorColorMode colorMode, NodeStyle nodeStyle, NodeCanvasStyleContext styleContext, float minorGridSize, float width)
    {
        var visual = new NodeSeparatorComponentVisual(
            GetSeparatorColor(colorMode, nodeStyle.ShapeStyle.BackgroundColor, nodeStyle.ShapeStyle.BorderColor, styleContext.FontColor),
            minorGridSize / 2.0f)
        {
            Bounds = new SKRect(0.0f, 0.0f, MathF.Max(1.0f, width), 2.0f * minorGridSize),
        };

        return visual;
    }

    private static float ClampScale(SKRect bounds, float baseScale, DragImageOptions options)
    {
        var widthScale = options.MaxWidthPx / MathF.Max(1.0f, bounds.Width);
        var heightScale = options.MaxHeightPx / MathF.Max(1.0f, bounds.Height);
        return MathF.Max(0.01f, MathF.Min(baseScale, MathF.Min(widthScale, heightScale)));
    }

    private static void ApplyFade(SKBitmap bitmap, DragImageOptions options)
    {
        var hotspot = new SKPoint(0.0f, 0.0f);
        var fadeRadius = MathF.Max(1.0f, options.FadeRadiusPx);

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                if (color.Alpha == 0)
                    continue;

                var dx = hotspot.X - x;
                var dy = y - hotspot.Y;
                var distance = MathF.Sqrt((dx * dx) + (dy * dy));
                var alphaScale = Math.Clamp(1.0f - (distance / fadeRadius), 0.0f, 1.0f);
                bitmap.SetPixel(x, y, color.WithAlpha((byte)MathF.Round(color.Alpha * alphaScale)));
            }
        }
    }

    private static float MeasureMainGridSize(NodeCanvasStyleContext styleContext)
    {
        var textHeight = MeasureTextHeight(styleContext.FontFamily, styleContext.FontSize, NodeFontWeight.Normal);
        return MathF.Ceiling(textHeight + (GridPadding * 2.0f));
    }

    private static float MeasureTextHeight(string? fontFamily, float fontSize, NodeFontWeight fontWeight)
    {
        using var typeface = CreateTypeface(fontFamily, fontWeight);
        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = fontSize,
            IsAntialias = true,
        };

        var textBounds = SKRect.Empty;
        textPaint.MeasureText("Mj", ref textBounds);
        var textHeight = MathF.Abs(textBounds.Height);
        return textHeight <= 0.0f ? fontSize : textHeight;
    }

    private static float MeasureTextWidth(string? fontFamily, float fontSize, NodeFontWeight fontWeight, string text)
    {
        using var typeface = CreateTypeface(fontFamily, fontWeight);
        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = fontSize,
            IsAntialias = true,
        };

        return textPaint.MeasureText(text);
    }

    private static float MeasureButtonWidth(string? text, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var labelWidth = MeasureTextWidth(styleContext.FontFamily, styleContext.FontSize, NodeFontWeight.Normal, text ?? string.Empty);
        var horizontalPadding = minorGridSize * 4.0f;
        var minimumWidth = minorGridSize * 10.0f;
        return MathF.Max(minimumWidth, labelWidth + horizontalPadding);
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

    private static SKColor GetSeparatorColor(NodeSeparatorColorMode colorMode, NodeColor nodeBackgroundColor, NodeColor borderColor, NodeColor textColor)
    {
        return colorMode switch
        {
            NodeSeparatorColorMode.Text => textColor.ToSkColor(),
            NodeSeparatorColorMode.Neutral => GetNeutralContrastColor(nodeBackgroundColor),
            _ => borderColor.ToSkColor(),
        };
    }

    private static SKColor GetNeutralContrastColor(NodeColor backgroundColor)
    {
        var grayscale = ((backgroundColor.Red * 0.299f) + (backgroundColor.Green * 0.587f) + (backgroundColor.Blue * 0.114f));

        if (grayscale <= 64.0f || grayscale >= 191.0f)
            return new SKColor(128, 128, 128);

        if (grayscale <= 127.0f)
            return new SKColor(192, 192, 192);

        return new SKColor(72, 72, 72);
    }

    private sealed class SpacerDragImageVisual(SKColor strokeColor, float strokeWidth) : NodeComponentVisual
    {
        public override void Render(SKCanvas canvas)
        {
            ArgumentNullException.ThrowIfNull(canvas);

            var width = Bounds.Width;
            var height = Bounds.Height;
            var centerX = Bounds.MidX;
            var topY = Bounds.Top + (strokeWidth * 0.75f);
            var bottomY = Bounds.Bottom - (strokeWidth * 0.75f);
            var barInset = width * 0.18f;
            var arrowInset = height * 0.18f;
            var arrowHalfWidth = width * 0.08f;

            using var paint = new SKPaint
            {
                Color = strokeColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
            };

            canvas.DrawLine(Bounds.Left + barInset, topY, Bounds.Right - barInset, topY, paint);
            canvas.DrawLine(Bounds.Left + barInset, bottomY, Bounds.Right - barInset, bottomY, paint);

            canvas.DrawLine(centerX, topY + arrowInset, centerX, bottomY - arrowInset, paint);

            var upperArrowY = topY + arrowInset;
            canvas.DrawLine(centerX, upperArrowY, centerX - arrowHalfWidth, upperArrowY + arrowInset, paint);
            canvas.DrawLine(centerX, upperArrowY, centerX + arrowHalfWidth, upperArrowY + arrowInset, paint);

            var lowerArrowY = bottomY - arrowInset;
            canvas.DrawLine(centerX, lowerArrowY, centerX - arrowHalfWidth, lowerArrowY - arrowInset, paint);
            canvas.DrawLine(centerX, lowerArrowY, centerX + arrowHalfWidth, lowerArrowY - arrowInset, paint);
        }
    }
}
