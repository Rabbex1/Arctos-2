using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Nodes.Wpf.ViewModels.Components;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Layout;

/// <summary>
/// Provides the first-pass structured layout engine for detailed node content.
/// </summary>
public sealed class NodeLayoutEngine : ILayoutEngine
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private const float GridPadding = 2.0f;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Rebuilds the detailed node layout snapshot for the provided host context.
    /// </summary>
    /// <param name="host">The current model host shown on the canvas.</param>
    /// <param name="overview"><see langword="true"/> when the host is in overview mode; otherwise, <see langword="false"/>.</param>
    /// <param name="mainGridSize">The current <c>1nl</c> size in world-space units.</param>
    /// <param name="minorGridSize">The current <c>1nu</c> size in world-space units.</param>
    /// <returns>The rebuilt transient node layout snapshot, or <see langword="null"/> when the host does not use node layout.</returns>
    public ILayoutSnapshot? Rebuild(IModelHost? host, bool overview, float mainGridSize, float minorGridSize, NodeCanvasStyleContext styleContext)
    {
        if (overview || host is not NodeDefinitionVM definitionHost)
            return null;

        var paddingNu = MathF.Max(0.0f, styleContext.Padding);
        var rowSpacingNu = MathF.Max(0.0f, styleContext.RowSpacing);
        var lineHeightNu = mainGridSize / minorGridSize;
        var size = definitionHost.Size;
        var headerHeightNu = definitionHost.ShapeRules.HeaderPosition == LabelPosition.Inside
            ? MeasureHeaderHeightNu(styleContext, definitionHost, minorGridSize)
            : 0.0f;
        var headerWidthNu = definitionHost.ShapeRules.HeaderPosition == LabelPosition.Inside
            ? MeasureHeaderWidthNu(styleContext, definitionHost, minorGridSize)
            : 0.0f;
        var topContentOffsetNu = headerHeightNu > 0.0f
            ? headerHeightNu
            : paddingNu;
        var horizontalBorderClearanceNu = GetHalfBorderClearanceNu(definitionHost, minorGridSize);
        var bottomBorderClearanceNu = GetBottomBorderClearanceNu(definitionHost, minorGridSize);
        var inputLabelWidthNu = MeasureInputLabelWidthNu(definitionHost, styleContext, minorGridSize);
        var requiredContentHeightNu = GetRequiredContentHeightNu(definitionHost, lineHeightNu, rowSpacingNu, styleContext);
        var requiredContentWidthNu = GetRequiredContentWidthNu(definitionHost, styleContext, minorGridSize, inputLabelWidthNu);
        var requiredHeightNu = MathF.Ceiling(topContentOffsetNu + requiredContentHeightNu + bottomBorderClearanceNu);
        var requiredWidthNu = MathF.Ceiling(MathF.Max(headerWidthNu, requiredContentWidthNu) + ((paddingNu + horizontalBorderClearanceNu) * 2.0f));
        var hasHeader = headerHeightNu > 0.0f;
        var hasComponents = definitionHost.NodeComponents.Count > 0;
        var minimumWidthNu = UsesContainerMinimumWidth(definitionHost.NodeShape)
            ? MathF.Max(0.0f, styleContext.MinimumColumnWidth)
            : 0.0f;
        var resolvedWidthNu = (int)MathF.Max(MathF.Max(1.0f, requiredWidthNu), minimumWidthNu);
        var resolvedHeightNu = !hasHeader || !hasComponents
            ? NodeSize.CreateDefault(definitionHost.NodeShape).Height
            : (int)MathF.Max(1.0f, requiredHeightNu);

        if (resolvedWidthNu != size.Width || resolvedHeightNu != size.Height)
        {
            size = new NodeSize(resolvedWidthNu, resolvedHeightNu);
            definitionHost.SetSize(size);
        }

        var nodeBounds = CreateNodeBounds(size, minorGridSize);
        var contentLeftNu = (-size.Width / 2.0f) + paddingNu;
        var contentTopNu = (-size.Height / 2.0f) + topContentOffsetNu;
        var contentRightNu = (size.Width / 2.0f) - paddingNu;
        var contentBottomNu = (size.Height / 2.0f) - bottomBorderClearanceNu;

        SKRect? headerBounds = null;
        if (headerHeightNu > 0.0f)
        {
            headerBounds = new SKRect(
                nodeBounds.Left,
                nodeBounds.Top,
                nodeBounds.Right,
                nodeBounds.Top + (headerHeightNu * minorGridSize));
        }

        var nextRowTopNu = contentTopNu;
        var componentLayouts = new List<NodeComponentLayoutSnapshot>();
        var orderedComponents = definitionHost.NodeComponents
            .Select((component, index) => new { component, index })
            .OrderBy(item => item.component.Position.Y)
            .ThenBy(item => item.index)
            .ToList();

        foreach (var item in orderedComponents)
        {
            var componentHeightNu = GetComponentHeightNu(item.component, lineHeightNu, styleContext);
            var resolvedY = nextRowTopNu;
            var resolvedPosition = new NodePoint(contentLeftNu, resolvedY);
            item.component.SetPosition(resolvedPosition, notifyComponentChanged: false);

            var bounds = MeasureComponentBounds(item.component, styleContext, minorGridSize, resolvedPosition, contentRightNu, lineHeightNu, componentHeightNu);
            if (item.component is NodePortComponentVM portComponent)
            {
                portComponent.Attach = new NodePoint(
                    portComponent.Side == NodePortSide.Left ? nodeBounds.Left : nodeBounds.Right,
                    bounds.MidY);
            }

            componentLayouts.Add(new NodeComponentLayoutSnapshot
            {
                Component = item.component,
                PositionNu = resolvedPosition,
                Bounds = bounds,
            });

            nextRowTopNu = resolvedY + componentHeightNu + rowSpacingNu;
        }

        return new NodeLayoutSnapshot
        {
            NodeBounds = nodeBounds,
            HeaderBounds = headerBounds,
            ContentBounds = new SKRect(
                contentLeftNu * minorGridSize,
                contentTopNu * minorGridSize,
                contentRightNu * minorGridSize,
                contentBottomNu * minorGridSize),
            LineHeight = mainGridSize,
            InputLabelWidth = inputLabelWidthNu * minorGridSize,
            Components = componentLayouts,
        };
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private static SKRect CreateNodeBounds(NodeSize size, float minorGridSize)
    {
        var halfWidth = (size.Width * minorGridSize) / 2.0f;
        var halfHeight = (size.Height * minorGridSize) / 2.0f;
        return new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight);
    }

    private static SKRect MeasureComponentBounds(NodeComponentVM component, NodeCanvasStyleContext styleContext, float minorGridSize, NodePoint positionNu, float contentRightNu, float lineHeightNu, float componentHeightNu)
    {
        return component switch
        {
            NodeLabelComponentVM labelComponent => MeasureLabelBounds(labelComponent, styleContext, minorGridSize, positionNu, contentRightNu, lineHeightNu),
            NodeInputComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodePortComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodeSeparatorComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodeSpacerComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            _ => SKRect.Empty,
        };
    }

    private static SKRect MeasureLabelBounds(NodeLabelComponentVM labelComponent, NodeCanvasStyleContext styleContext, float minorGridSize, NodePoint positionNu, float contentRightNu, float lineHeightNu)
    {
        var x = positionNu.X * minorGridSize;
        var y = positionNu.Y * minorGridSize;
        var right = contentRightNu * minorGridSize;
        var height = lineHeightNu * minorGridSize;
        return new SKRect(x, y, right, y + height);
    }

    private static SKRect MeasureFullWidthBounds(NodePoint positionNu, float contentRightNu, float minorGridSize, float componentHeightNu)
    {
        var x = positionNu.X * minorGridSize;
        var y = positionNu.Y * minorGridSize;
        var right = contentRightNu * minorGridSize;
        var height = componentHeightNu * minorGridSize;
        return new SKRect(x, y, right, y + height);
    }

    private static float MeasureHeaderHeightNu(NodeCanvasStyleContext styleContext, NodeDefinitionVM definitionHost, float minorGridSize)
    {
        if (string.IsNullOrWhiteSpace(definitionHost.ResolvedHeaderText))
            return 0.0f;

        using var typeface = string.IsNullOrWhiteSpace(styleContext.FontFamily)
            ? null
            : SKTypeface.FromFamilyName(styleContext.FontFamily);

        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = styleContext.HeaderFontSize,
            IsAntialias = true,
        };

        var textBounds = SKRect.Empty;
        textPaint.MeasureText("Mj", ref textBounds);
        var textHeight = MathF.Abs(textBounds.Height);

        if (textHeight <= 0.0f)
            textHeight = styleContext.HeaderFontSize;

        var headerLineHeight = MathF.Ceiling(textHeight + (GridPadding * 2.0f));
        return MathF.Ceiling(headerLineHeight / minorGridSize);
    }

    private static float MeasureHeaderWidthNu(NodeCanvasStyleContext styleContext, NodeDefinitionVM definitionHost, float minorGridSize)
    {
        if (string.IsNullOrWhiteSpace(definitionHost.ResolvedHeaderText))
            return 0.0f;

        using var typeface = string.IsNullOrWhiteSpace(styleContext.FontFamily)
            ? null
            : SKTypeface.FromFamilyName(styleContext.FontFamily);

        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = styleContext.HeaderFontSize,
            IsAntialias = true,
        };

        var textWidth = textPaint.MeasureText(definitionHost.ResolvedHeaderText);
        if (textWidth <= 0.0f)
            return 0.0f;

        return MathF.Ceiling(textWidth / minorGridSize);
    }

    private static float GetRequiredContentHeightNu(NodeDefinitionVM definitionHost, float lineHeightNu, float rowSpacingNu, NodeCanvasStyleContext styleContext)
    {
        if (definitionHost.NodeComponents.Count == 0)
            return 0.0f;

        var totalHeightNu = definitionHost.NodeComponents.Sum(component => GetComponentHeightNu(component, lineHeightNu, styleContext));
        return totalHeightNu + ((definitionHost.NodeComponents.Count - 1) * rowSpacingNu);
    }

    private static float GetRequiredContentWidthNu(NodeDefinitionVM definitionHost, NodeCanvasStyleContext styleContext, float minorGridSize, float inputLabelWidthNu)
    {
        if (definitionHost.NodeComponents.Count == 0)
            return 0.0f;

        return definitionHost.NodeComponents.Max(component => component switch
        {
            NodeLabelComponentVM labelComponent => MeasureLabelWidthNu(labelComponent, styleContext, minorGridSize),
            NodeInputComponentVM inputComponent => MeasureInputWidthNu(inputComponent, styleContext, minorGridSize, inputLabelWidthNu),
            NodePortComponentVM portComponent => MeasurePortWidthNu(portComponent, styleContext, minorGridSize),
            _ => 0.0f,
        });
    }

    private static float MeasureInputLabelWidthNu(NodeDefinitionVM definitionHost, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var inputs = definitionHost.NodeComponents.OfType<NodeInputComponentVM>().ToList();
        if (inputs.Count == 0)
            return 0.0f;

        return inputs.Max(input => MeasureTextWidthNu(input.Label, styleContext, minorGridSize));
    }

    private static float MeasureLabelWidthNu(NodeLabelComponentVM labelComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        if (string.IsNullOrWhiteSpace(labelComponent.Label))
            return 0.0f;

        using var typeface = string.IsNullOrWhiteSpace(styleContext.FontFamily)
            ? null
            : SKTypeface.FromFamilyName(styleContext.FontFamily);

        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = styleContext.FontSize,
            IsAntialias = true,
        };

        var textWidth = textPaint.MeasureText(labelComponent.Label);
        if (textWidth <= 0.0f)
            return 0.0f;

        return MathF.Ceiling(textWidth / minorGridSize);
    }

    private static float MeasureInputWidthNu(NodeInputComponentVM inputComponent, NodeCanvasStyleContext styleContext, float minorGridSize, float inputLabelWidthNu)
    {
        var fieldMinWidth = styleContext.FontSize * 6.0f;
        var fieldWidth = inputComponent.TypeId == InputTypeIds.Boolean
            ? styleContext.FontSize
            : fieldMinWidth;
        var gap = inputLabelWidthNu > 0.0f ? styleContext.FontSize : 0.0f;
        return MathF.Ceiling(inputLabelWidthNu + (gap / minorGridSize) + (fieldWidth / minorGridSize));
    }

    private static float MeasurePortWidthNu(NodePortComponentVM portComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var labelWidthNu = MeasureTextWidthNu(portComponent.Label, styleContext, minorGridSize);
        var gapNu = labelWidthNu > 0.0f ? 1.0f : 0.0f;
        return MathF.Ceiling(labelWidthNu + gapNu + MathF.Max(0.0f, styleContext.PortSize));
    }

    private static float MeasureTextWidthNu(string? text, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0.0f;

        using var typeface = string.IsNullOrWhiteSpace(styleContext.FontFamily)
            ? null
            : SKTypeface.FromFamilyName(styleContext.FontFamily);

        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = styleContext.FontSize,
            IsAntialias = true,
        };

        var textWidth = textPaint.MeasureText(text);
        if (textWidth <= 0.0f)
            return 0.0f;

        return MathF.Ceiling(textWidth / minorGridSize);
    }

    private static float GetComponentHeightNu(NodeComponentVM component, float lineHeightNu, NodeCanvasStyleContext styleContext)
    {
        return component switch
        {
            NodeSpacerComponentVM spacerComponent => MathF.Max(1.0f, spacerComponent.Height),
            NodePortComponentVM => MathF.Max(lineHeightNu, MathF.Max(1.0f, styleContext.PortSize)),
            NodeSeparatorComponentVM => 2.0f,
            _ => lineHeightNu,
        };
    }

    private static float GetBottomBorderClearanceNu(NodeDefinitionVM definitionHost, float minorGridSize)
    {
        return GetHalfBorderClearanceNu(definitionHost, minorGridSize);
    }

    private static float GetHalfBorderClearanceNu(NodeDefinitionVM definitionHost, float minorGridSize)
    {
        var borderThickness = definitionHost.Model.Style?.ShapeStyle.BorderThickness
                              ?? new NodeStyle().ShapeStyle.BorderThickness;
        var nodeStrokeSize = minorGridSize / 4.0f;
        var halfStrokeWorld = (borderThickness * nodeStrokeSize) / 2.0f;
        return halfStrokeWorld / minorGridSize;
    }

    private static bool UsesContainerMinimumWidth(NodeShape shape)
    {
        return shape == NodeShape.Rectangle
               || shape == NodeShape.RoundedRectangle
               || shape == NodeShape.Capsule;
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
