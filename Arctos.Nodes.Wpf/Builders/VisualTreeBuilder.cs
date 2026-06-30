using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Nodes.Wpf.Visuals;
using Arctos.Nodes.Wpf.Layout;
using Arctos.Nodes.Wpf.Rendering;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Builders;

/// <summary>
/// Builds the visual tree for the current Skia host context.
/// </summary>
public sealed class VisualTreeBuilder
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Builds a visual tree for the provided host context.
    /// </summary>
    /// <param name="host">The current model host for the canvas.</param>
    /// <param name="overview">A value indicating whether the host is being shown in overview mode.</param>
    /// <param name="backgroundColor">The effective background color for the root canvas.</param>
    /// <param name="minorGridSize">The current minor grid spacing in node-space units.</param>
    /// <returns>The built root visual for the current canvas scene.</returns>
    public CanvasVisual Build(IModelHost? host, bool overview, NodeColor backgroundColor, float minorGridSize, NodeCanvasStyleContext styleContext, ILayoutSnapshot? layoutSnapshot = null, NodeComponentVM? previewComponent = null, bool showDebugBounds = false)
    {
        var rootVisual = new CanvasVisual
        {
            BackgroundColor = backgroundColor,
        };

        if (host is NodeDefinitionVM definitionHost)
            rootVisual.AddChild(CreateNodeVisual(definitionHost, minorGridSize, styleContext, layoutSnapshot as NodeLayoutSnapshot, previewComponent, showDebugBounds));

        return rootVisual;
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private static NodeVisual CreateNodeVisual(NodeDefinitionVM definitionHost, float minorGridSize, NodeCanvasStyleContext styleContext, NodeLayoutSnapshot? layoutSnapshot, NodeComponentVM? previewComponent, bool showDebugBounds)
    {
        ArgumentNullException.ThrowIfNull(definitionHost);

        var nodeStyle = definitionHost.Model.Style ?? new NodeStyle();
        var shapeStyle = nodeStyle.ShapeStyle;
        var headerStyle = definitionHost.ShapeRules.HeaderPosition == LabelPosition.Inside
            ? nodeStyle.HeaderStyle ?? new NodeHeaderStyle()
            : null;
        var headerText = definitionHost.ShapeRules.HeaderPosition == LabelPosition.Inside
            ? definitionHost.ResolvedHeaderText
            : null;
        var nodeStrokeSize = minorGridSize / 4.0f;
        var headerHeight = layoutSnapshot?.HeaderBounds?.Height ?? (minorGridSize * 4.0f);
        var headerHorizontalPadding = styleContext.Padding * minorGridSize;
        NodeVisual nodeVisual = definitionHost.NodeShape switch
        {
            NodeShape.RoundedRectangle => new RoundedRectangleNodeVisual(shapeStyle, headerStyle, headerText, styleContext.FontFamily, styleContext.HeaderFontSize, styleContext.FontColor, headerHorizontalPadding, minorGridSize, nodeStrokeSize, headerHeight, styleContext.RoundedCornerRadius * minorGridSize),
            NodeShape.Rectangle => new RectangleNodeVisual(shapeStyle, headerStyle, headerText, styleContext.FontFamily, styleContext.HeaderFontSize, styleContext.FontColor, headerHorizontalPadding, minorGridSize, nodeStrokeSize, headerHeight),
            NodeShape.Diamond => new DiamondNodeVisual(shapeStyle, headerStyle, headerText, styleContext.FontFamily, styleContext.HeaderFontSize, styleContext.FontColor, headerHorizontalPadding, minorGridSize, nodeStrokeSize, headerHeight),
            NodeShape.Circle => new CircleNodeVisual(shapeStyle, headerStyle, headerText, styleContext.FontFamily, styleContext.HeaderFontSize, styleContext.FontColor, headerHorizontalPadding, minorGridSize, nodeStrokeSize, headerHeight),
            NodeShape.Capsule => new CapsuleNodeVisual(shapeStyle, headerStyle, headerText, styleContext.FontFamily, styleContext.HeaderFontSize, styleContext.FontColor, headerHorizontalPadding, minorGridSize, nodeStrokeSize, headerHeight),
            _ => new RoundedRectangleNodeVisual(shapeStyle, headerStyle, headerText, styleContext.FontFamily, styleContext.HeaderFontSize, styleContext.FontColor, headerHorizontalPadding, minorGridSize, nodeStrokeSize, headerHeight, styleContext.RoundedCornerRadius * minorGridSize),
        };

        nodeVisual.Bounds = CreateOverviewBounds(definitionHost.Size, minorGridSize);
        AddComponentVisuals(nodeVisual, definitionHost, nodeStyle, styleContext, minorGridSize, layoutSnapshot, previewComponent, showDebugBounds);
        return nodeVisual;
    }

    private static void AddComponentVisuals(NodeVisual nodeVisual, NodeDefinitionVM definitionHost, NodeStyle nodeStyle, NodeCanvasStyleContext styleContext, float minorGridSize, NodeLayoutSnapshot? layoutSnapshot, NodeComponentVM? previewComponent, bool showDebugBounds)
    {
        ArgumentNullException.ThrowIfNull(nodeVisual);
        ArgumentNullException.ThrowIfNull(definitionHost);
        ArgumentNullException.ThrowIfNull(nodeStyle);

        nodeVisual.ClearChildren();
        var componentBounds = layoutSnapshot?.Components.ToDictionary(layout => layout.Component, layout => layout.Bounds);
        var inputLabelWidth = layoutSnapshot?.InputLabelWidth ?? 0.0f;
        var nodeBounds = layoutSnapshot?.NodeBounds ?? nodeVisual.Bounds;

        foreach (var component in definitionHost.NodeComponents)
        {
            AddComponentVisual(nodeVisual, component, nodeStyle, styleContext, minorGridSize, componentBounds, inputLabelWidth, nodeBounds, false, showDebugBounds);
        }

        if (previewComponent != null)
            AddComponentVisual(nodeVisual, previewComponent, nodeStyle, styleContext, minorGridSize, componentBounds, inputLabelWidth, nodeBounds, true, showDebugBounds);

        if (layoutSnapshot?.Columns.Count > 0)
        {
            var headerBottom = layoutSnapshot.HeaderBounds?.Bottom ?? nodeBounds.Top;
            nodeVisual.AddChild(new NodeColumnHandlesVisual(
                layoutSnapshot.Columns,
                headerBottom,
                nodeStyle.ShapeStyle.BackgroundColor.ToSkColor(),
                NodeVisualDrawing.GetNeutralColor(nodeStyle.ShapeStyle.BackgroundColor),
                handleWidth: minorGridSize * 4.0f,
                handleHeight: minorGridSize,
                strokeWidth: minorGridSize / 8.0f));
        }
    }

    private static void AddComponentVisual(NodeVisual nodeVisual, NodeComponentVM component, NodeStyle nodeStyle, NodeCanvasStyleContext styleContext, float minorGridSize, Dictionary<NodeComponentVM, SKRect>? componentBounds, float inputLabelWidth, SKRect nodeBounds, bool isPreview, bool showDebugBounds)
    {
        var bounds = componentBounds != null && componentBounds.TryGetValue(component, out var componentBound)
            ? componentBound
            : CreateFallbackBounds(component, styleContext, minorGridSize);
        var selectionColor = NodeVisualDrawing.GetSelectionColor(nodeStyle.ShapeStyle.BackgroundColor);
        var selectionDashLength = NodeVisualDrawing.GetSelectionDashLength(minorGridSize);
        var selectionHorizontalPadding = minorGridSize * 0.5f;

        switch (component)
        {
            case NodeLabelComponentVM labelComponent:
                var labelVisual = new NodeLabelComponentVisual(
                    labelComponent.Label,
                    styleContext.FontFamily,
                    styleContext.FontSize,
                    styleContext.FontColor,
                    nodeStyle.ShapeStyle.FontWeight,
                    isPreview ? 0.6f : 1.0f,
                    showDebugBounds,
                    labelComponent.IsHovered && !isPreview,
                    labelComponent.IsSelected && !isPreview,
                    selectionColor,
                    selectionDashLength,
                    selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(labelVisual);
                break;

            case NodeInputComponentVM inputComponent:
                var inputVisual = new NodeInputComponentVisual(
                    inputComponent.Label,
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
                    inputLabelWidth: inputLabelWidth,
                    dateTimeDisplayMode: inputComponent.DateTimeDisplayMode,
                    showDebugBounds: showDebugBounds,
                    isHovered: inputComponent.IsHovered && !isPreview,
                    isSelected: inputComponent.IsSelected && !isPreview,
                    isPreview: isPreview,
                    defaultSelectionColor: selectionColor,
                    selectionDashLength: selectionDashLength,
                    selectionHorizontalPadding: selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(inputVisual);
                break;

            case NodeValueDisplayComponentVM valueDisplayComponent:
                var valueDisplayVisual = new NodeInputComponentVisual(
                    valueDisplayComponent.Label,
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
                    inputLabelWidth: inputLabelWidth,
                    valueTextColor: valueDisplayComponent.ValueColorMode == NodeValueDisplayColorMode.Custom
                        ? valueDisplayComponent.ValueColor
                        : null,
                    showDebugBounds: showDebugBounds,
                    isHovered: valueDisplayComponent.IsHovered && !isPreview,
                    isSelected: valueDisplayComponent.IsSelected && !isPreview,
                    isPreview: isPreview,
                    defaultSelectionColor: selectionColor,
                    selectionDashLength: selectionDashLength,
                    selectionHorizontalPadding: selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(valueDisplayVisual);
                break;

            case NodeButtonComponentVM buttonComponent:
                var buttonVisual = new NodeButtonComponentVisual(
                    buttonComponent.Label,
                    styleContext.FontFamily,
                    styleContext.FontSize,
                    styleContext.FontColor,
                    nodeStyle.ShapeStyle.FontWeight,
                    nodeStyle.ShapeStyle.BorderColor,
                    styleContext.ControlCornerRadius * minorGridSize,
                    minorGridSize,
                    previewOpacity: isPreview ? 0.6f : 1.0f,
                    showDebugBounds: showDebugBounds,
                    isHovered: buttonComponent.IsHovered && !isPreview,
                    isSelected: buttonComponent.IsSelected && !isPreview,
                    isPreview: isPreview,
                    defaultSelectionColor: selectionColor,
                    selectionDashLength: selectionDashLength,
                    selectionHorizontalPadding: selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(buttonVisual);
                break;

            case NodeIconButtonComponentVM iconButtonComponent:
                var iconButtonVisual = new NodeIconButtonComponentVisual(
                    iconButtonComponent.Svg,
                    iconButtonComponent.ColorMode,
                    nodeStyle.ShapeStyle.BackgroundColor,
                    nodeStyle.ShapeStyle.BorderColor,
                    styleContext.FontColor,
                    styleContext.ControlCornerRadius * minorGridSize,
                    minorGridSize,
                    previewOpacity: isPreview ? 0.6f : 1.0f,
                    showDebugBounds: showDebugBounds,
                    isHovered: iconButtonComponent.IsHovered && !isPreview,
                    isSelected: iconButtonComponent.IsSelected && !isPreview,
                    isPreview: isPreview,
                    defaultSelectionColor: selectionColor,
                    selectionDashLength: selectionDashLength,
                    selectionHorizontalPadding: selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(iconButtonVisual);
                break;

            case NodeIndicatorComponentVM indicatorComponent:
                var indicatorVisual = new NodeIndicatorComponentVisual(
                    indicatorComponent.Label,
                    indicatorComponent.Shape,
                    indicatorComponent.Color,
                    nodeStyle.ShapeStyle.BorderColor,
                    styleContext.FontFamily,
                    styleContext.FontSize,
                    styleContext.FontColor,
                    nodeStyle.ShapeStyle.FontWeight,
                    styleContext.ControlCornerRadius,
                    minorGridSize,
                    showDebugBounds: showDebugBounds,
                    isHovered: indicatorComponent.IsHovered && !isPreview,
                    isSelected: indicatorComponent.IsSelected && !isPreview,
                    isPreview: isPreview,
                    defaultSelectionColor: selectionColor,
                    selectionDashLength: selectionDashLength,
                    selectionHorizontalPadding: selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(indicatorVisual);
                break;

            case NodeGridInputComponentVM gridInputComponent:
                var gridInputVisual = new NodeGridInputComponentVisual(
                    gridInputComponent.Label,
                    gridInputComponent.Rows,
                    gridInputComponent.Columns,
                    gridInputComponent.CellWidth,
                    gridInputComponent.CellTypeId,
                    gridInputComponent.DefaultValues,
                    styleContext.FontFamily,
                    styleContext.FontSize,
                    styleContext.FontColor,
                    nodeStyle.ShapeStyle.FontWeight,
                    nodeStyle.ShapeStyle.BorderColor,
                    styleContext.ControlCornerRadius,
                    minorGridSize,
                    showDebugBounds: showDebugBounds,
                    isHovered: gridInputComponent.IsHovered && !isPreview,
                    isSelected: gridInputComponent.IsSelected && !isPreview,
                    isPreview: isPreview,
                    defaultSelectionColor: selectionColor,
                    selectionDashLength: selectionDashLength,
                    selectionHorizontalPadding: selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(gridInputVisual);
                break;

            case NodeSeparatorComponentVM separatorComponent:
                var separatorVisual = new NodeSeparatorComponentVisual(
                    GetSeparatorColor(separatorComponent.ColorMode, nodeStyle.ShapeStyle.BackgroundColor, nodeStyle.ShapeStyle.BorderColor, styleContext.FontColor)
                        .WithAlpha((byte)(isPreview ? 153 : 255)),
                    minorGridSize / 8.0f,
                    showDebugBounds,
                    separatorComponent.IsHovered && !isPreview,
                    separatorComponent.IsSelected && !isPreview,
                    selectionColor,
                    selectionDashLength,
                    selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(separatorVisual);
                break;

            case NodePortComponentVM portComponent:
                var portVisual = new NodePortComponentVisual(
                    portComponent.Label,
                    portComponent.Side,
                    portComponent.Shape,
                    portComponent.Shape == NodePortShape.Hidden ? NodePortPlacement.Over : styleContext.PortPlacement,
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
                    nodeBounds.Left,
                    nodeBounds.Right,
                    opacity: 1.0f,
                    showDebugBounds: showDebugBounds,
                    isHovered: portComponent.IsHovered && !isPreview,
                    isSelected: portComponent.IsSelected && !isPreview,
                    defaultSelectionColor: selectionColor,
                    selectionDashLength: selectionDashLength,
                    selectionHorizontalPadding: selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(portVisual);
                break;

            case NodeSpacerComponentVM spacerComponent:
                var spacerVisual = new NodeSpacerComponentVisual(
                    showDebugBounds,
                    spacerComponent.IsHovered && !isPreview,
                    spacerComponent.IsSelected && !isPreview,
                    selectionColor,
                    selectionDashLength,
                    selectionHorizontalPadding)
                {
                    Bounds = bounds,
                };
                nodeVisual.AddChild(spacerVisual);
                break;
        }
    }

    private static SKRect CreateFallbackBounds(NodeComponentVM component, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        return component switch
        {
            NodeLabelComponentVM labelComponent => CreateLabelBounds(labelComponent, styleContext, minorGridSize),
            NodeInputComponentVM => CreateFullWidthBounds(component.Position, 4.0f, minorGridSize),
            NodeValueDisplayComponentVM => CreateFullWidthBounds(component.Position, 4.0f, minorGridSize),
            NodeGridInputComponentVM gridInputComponent => CreateFullWidthBounds(component.Position, MathF.Max(4.0f, gridInputComponent.Rows * 4.0f), minorGridSize),
            NodeButtonComponentVM => CreateFullWidthBounds(component.Position, 4.0f, minorGridSize),
            NodeIconButtonComponentVM => CreateFullWidthBounds(component.Position, 4.0f, minorGridSize),
            NodeIndicatorComponentVM => CreateFullWidthBounds(component.Position, 4.0f, minorGridSize),
            NodePortComponentVM => CreateFullWidthBounds(component.Position, MathF.Max(4.0f, styleContext.PortSize), minorGridSize),
            NodeSpacerComponentVM spacerComponent => CreateFullWidthBounds(component.Position, MathF.Max(1.0f, spacerComponent.Height), minorGridSize),
            NodeSeparatorComponentVM => CreateFullWidthBounds(component.Position, 2.0f, minorGridSize),
            _ => CreateFullWidthBounds(component.Position, 4.0f, minorGridSize),
        };
    }

    private static SKRect CreateLabelBounds(NodeLabelComponentVM labelComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            TextSize = styleContext.FontSize,
            Typeface = CreateTypeface(styleContext.FontFamily, NodeFontWeight.Normal),
        };

        var textBounds = SKRect.Empty;
        var text = labelComponent.Label ?? string.Empty;
        textPaint.MeasureText(text, ref textBounds);

        var x = labelComponent.Position.X * minorGridSize;
        var y = labelComponent.Position.Y * minorGridSize;
        var width = MathF.Max(1.0f, textBounds.Width);
        var height = MathF.Max(styleContext.FontSize, MathF.Abs(textBounds.Height));
        return new SKRect(
            x,
            y,
            x + width,
            y + height);
    }

    private static SKRect CreateFullWidthBounds(NodePoint position, float heightNu, float minorGridSize)
    {
        var x = position.X * minorGridSize;
        var y = position.Y * minorGridSize;
        return new SKRect(x, y, x + (24.0f * minorGridSize), y + (heightNu * minorGridSize));
    }

    private static SKRect CreateOverviewBounds(NodeSize size, float minorGridSize)
    {
        var halfWidth = (size.Width * minorGridSize) / 2.0f;
        var halfHeight = (size.Height * minorGridSize) / 2.0f;
        return new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight);
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
            NodeSeparatorColorMode.Neutral => NodeVisualDrawing.GetNeutralColor(nodeBackgroundColor),
            _ => borderColor.ToSkColor(),
        };
    }

    #endregion Private Methods
}
