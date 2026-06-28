using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Nodes.Wpf.Visuals;
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
    private const float ButtonHorizontalPaddingNu = 0.0f;
    private const float ButtonMinimumWidthNu = 10.0f;
    private const float InputTextScale = 0.6f;
    private const float InputFieldHorizontalPaddingNu = 1.0f;
    private const float InputFieldInternalGapNu = 0.5f;
    private const float IndicatorTextScale = 0.6f;
    private const float IndicatorHorizontalPaddingNu = 1.0f;
    private const float IndicatorInternalGapNu = 0.5f;
    private const float IndicatorSizeScale = 0.6f;
    private const float GridInputTextScale = 0.6f;
    private const float GridInputOuterPaddingNu = 0.5f;
    private const float GridInputInternalGapNu = 0.5f;

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
        var topContentPaddingNu = paddingNu;
        var topContentOffsetNu = headerHeightNu > 0.0f
            ? headerHeightNu + topContentPaddingNu
            : paddingNu;
        var horizontalBorderClearanceNu = GetHalfBorderClearanceNu(definitionHost, minorGridSize);
        var bottomBorderClearanceNu = GetBottomBorderClearanceNu(definitionHost, minorGridSize);
        var borderThickness = definitionHost.Model.Style?.ShapeStyle.BorderThickness
                              ?? new NodeStyle().ShapeStyle.BorderThickness;
        var columnCount = Math.Max(1, definitionHost.ColumnCount);
        var inputLabelWidthNu = MeasureInputLabelWidthNu(definitionHost, styleContext, minorGridSize);
        var rows = BuildRows(definitionHost, styleContext, lineHeightNu, columnCount);
        var requiredContentHeightNu = GetRequiredContentHeightNu(rows, rowSpacingNu);
        var singleColumnContentWidthNu = GetRequiredContentWidthNu(rows, styleContext, minorGridSize, inputLabelWidthNu);
        var requiredHeightNu = MathF.Ceiling(topContentOffsetNu + requiredContentHeightNu + bottomBorderClearanceNu);
        var hasHeader = headerHeightNu > 0.0f;
        var hasComponents = definitionHost.NodeComponents.Count > 0;
        var minimumColumnWidthNu = UsesContainerMinimumWidth(definitionHost.NodeShape)
            ? MathF.Max(0.0f, styleContext.MinimumColumnWidth)
            : 0.0f;
        var columnContentWidthNu = MathF.Max(singleColumnContentWidthNu, minimumColumnWidthNu);
        var columnPaddingNu = columnCount > 1
            ? paddingNu
            : 0.0f;
        var columnGutterNu = columnPaddingNu * 2.0f;
        var requiredColumnsWidthNu = columnCount > 0
            ? (columnContentWidthNu * columnCount) + (columnGutterNu * MathF.Max(0, columnCount - 1))
            : 0.0f;
        var requiredWidthNu = MathF.Ceiling(MathF.Max(headerWidthNu, requiredColumnsWidthNu) + ((paddingNu + horizontalBorderClearanceNu) * 2.0f));
        var resolvedWidthNu = (int)MathF.Max(1.0f, requiredWidthNu);
        var resolvedHeightNu = !hasHeader || !hasComponents
            ? NodeSize.CreateDefault(definitionHost.NodeShape).Height
            : (int)MathF.Max(1.0f, requiredHeightNu);
        resolvedHeightNu = EnsureEven(resolvedHeightNu);

        if (resolvedWidthNu != size.Width || resolvedHeightNu != size.Height)
        {
            size = new NodeSize(resolvedWidthNu, resolvedHeightNu);
            definitionHost.SetSize(size);
        }

        var nodeBounds = CreateNodeBounds(size, minorGridSize);
        var contentLeftNu = MathF.Ceiling(((-size.Width) / 2.0f) + paddingNu);
        var contentTopNu = ((-size.Height) / 2.0f) + topContentOffsetNu;
        var contentRightNu = MathF.Floor((size.Width / 2.0f) - paddingNu);
        var contentBottomNu = MathF.Floor((size.Height / 2.0f) - bottomBorderClearanceNu);
        var primaryContentRightNu = MathF.Min(contentRightNu, contentLeftNu + columnContentWidthNu);
        var columns = BuildColumns(contentLeftNu, contentTopNu, primaryContentRightNu, contentBottomNu, columnCount, columnPaddingNu, topContentPaddingNu, minorGridSize);

        SKRect? headerBounds = null;
        if (headerHeightNu > 0.0f)
        {
            headerBounds = new SKRect(
                nodeBounds.Left,
                nodeBounds.Top,
                nodeBounds.Right,
                nodeBounds.Top + (headerHeightNu * minorGridSize));
        }

        var componentLayouts = new List<NodeComponentLayoutSnapshot>();
        var visualBounds = nodeBounds;

        foreach (var columnLayout in columns)
        {
            var columnLeftNu = columnLayout.ContentBounds.Left / minorGridSize;
            var columnRightNu = columnLayout.ContentBounds.Right / minorGridSize;
            var nextRowTopNu = columnLayout.ContentBounds.Top / minorGridSize;
            var columnRows = rows
                .Where(row => row.ColumnIndex == columnLayout.Index)
                .OrderBy(row => row.YOrder)
                .ToList();

            foreach (var row in columnRows)
            {
                var resolvedY = nextRowTopNu;
                var nextColumnLeftNu = columnLeftNu;
                var buttonCount = row.Items.Count;
                var availableRowWidthNu = MathF.Max(0.0f, columnRightNu - columnLeftNu);
                var buttonGapNu = GetButtonGapNu(styleContext);
                var flexButtonCount = row.IsButtonRow
                    ? row.Items.Count(item => item.Component.Model.FixedWidth <= 0.0f)
                    : 0;
                var fixedButtonWidthNu = row.IsButtonRow
                    ? row.Items.Where(item => item.Component.Model.FixedWidth > 0.0f).Sum(item => item.WidthNu)
                    : 0.0f;
                var totalButtonGapNu = MathF.Max(0, buttonCount - 1) * buttonGapNu;
                var useSharedButtonWidth = row.IsButtonRow && flexButtonCount == buttonCount;
                var sharedButtonWidthNu = useSharedButtonWidth && buttonCount > 0
                    ? MathF.Max(0.0f, (availableRowWidthNu - (MathF.Max(0, buttonCount - 1) * buttonGapNu)) / buttonCount)
                    : 0.0f;
                var sharedFlexibleButtonWidthNu = row.IsButtonRow && flexButtonCount > 0
                    ? MathF.Max(0.0f, (availableRowWidthNu - fixedButtonWidthNu - totalButtonGapNu) / flexButtonCount)
                    : 0.0f;

                foreach (var item in row.Items)
                {
                    var component = item.Component;
                    component.SetColumn(columnLayout.Index, notifyComponentChanged: false);

                    var componentHeightNu = GetComponentHeightNu(component, lineHeightNu, styleContext);
                    var resolvedX = row.IsButtonRow
                        ? nextColumnLeftNu
                        : columnLeftNu;
                    var resolvedPosition = new NodePoint(resolvedX, resolvedY);
                    component.SetPosition(resolvedPosition, notifyComponentChanged: false);

                    var buttonWidthNu = useSharedButtonWidth
                        ? sharedButtonWidthNu
                        : row.IsButtonRow && item.Component.Model.FixedWidth <= 0.0f
                            ? sharedFlexibleButtonWidthNu
                            : item.WidthNu;
                    var bounds = row.IsButtonRow && IsButtonLikeComponent(component)
                        ? MeasureButtonBounds(resolvedPosition, buttonWidthNu, lineHeightNu, minorGridSize)
                        : MeasureComponentBounds(component, styleContext, minorGridSize, resolvedPosition, columnRightNu, lineHeightNu, componentHeightNu);
                    var componentVisualBounds = MeasureComponentVisualBounds(component, styleContext, minorGridSize, bounds, nodeBounds, borderThickness);
                    if (component is NodePortComponentVM portComponent)
                    {
                        portComponent.Attach = new NodePoint(
                            portComponent.Side == NodePortSide.Left ? nodeBounds.Left : nodeBounds.Right,
                            bounds.MidY);
                    }

                    componentLayouts.Add(new NodeComponentLayoutSnapshot
                    {
                        Component = component,
                        PositionNu = resolvedPosition,
                        ColumnIndex = columnLayout.Index,
                        YOrder = component.YOrder,
                        XOrder = component.XOrder,
                        Bounds = bounds,
                        VisualBounds = componentVisualBounds,
                    });

                    visualBounds = UnionBounds(visualBounds, componentVisualBounds);

                    if (row.IsButtonRow)
                        nextColumnLeftNu = resolvedX + buttonWidthNu + buttonGapNu;
                }

                nextRowTopNu = resolvedY + row.HeightNu + rowSpacingNu;
            }
        }

        return new NodeLayoutSnapshot
        {
            NodeBounds = nodeBounds,
            VisualBounds = visualBounds,
            HeaderBounds = headerBounds,
            ContentBounds = new SKRect(
                columns[0].ContentBounds.Left,
                columns[0].ContentBounds.Top,
                columns[^1].ContentBounds.Right,
                columns[^1].ContentBounds.Bottom),
            LineHeight = mainGridSize,
            InputLabelWidth = inputLabelWidthNu * minorGridSize,
            Columns = columns,
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
            NodeValueDisplayComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodeGridInputComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodeButtonComponentVM => MeasureButtonBounds(positionNu, MeasureButtonWidthNu(component, styleContext, minorGridSize), lineHeightNu, minorGridSize),
            NodeIconButtonComponentVM => MeasureButtonBounds(positionNu, MeasureButtonWidthNu(component, styleContext, minorGridSize), lineHeightNu, minorGridSize),
            NodeIndicatorComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodePortComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodeSeparatorComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            NodeSpacerComponentVM => MeasureFullWidthBounds(positionNu, contentRightNu, minorGridSize, componentHeightNu),
            _ => SKRect.Empty,
        };
    }

    private static SKRect MeasureComponentVisualBounds(NodeComponentVM component, NodeCanvasStyleContext styleContext, float minorGridSize, SKRect bounds, SKRect nodeBounds, float borderThickness)
    {
        if (component is not NodePortComponentVM portComponent)
            return bounds;

        var placement = portComponent.Shape == NodePortShape.Hidden
            ? NodePortPlacement.Over
            : styleContext.PortPlacement;
        var nodeStrokeSize = minorGridSize / 4.0f;
        var portBounds = NodePortGeometry.CreatePortBounds(
            bounds,
            portComponent.Side,
            placement,
            styleContext.PortSize,
            nodeBounds.Left,
            nodeBounds.Right,
            borderThickness,
            minorGridSize,
            nodeStrokeSize);

        return UnionBounds(bounds, portBounds);
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

    private static SKRect MeasureButtonBounds(NodePoint positionNu, float widthNu, float lineHeightNu, float minorGridSize)
    {
        var x = positionNu.X * minorGridSize;
        var y = positionNu.Y * minorGridSize;
        var width = widthNu * minorGridSize;
        var height = lineHeightNu * minorGridSize;
        return new SKRect(x, y, x + width, y + height);
    }

    private static SKRect UnionBounds(SKRect current, SKRect next)
    {
        if (current.IsEmpty)
            return next;

        if (next.IsEmpty)
            return current;

        return new SKRect(
            MathF.Min(current.Left, next.Left),
            MathF.Min(current.Top, next.Top),
            MathF.Max(current.Right, next.Right),
            MathF.Max(current.Bottom, next.Bottom));
    }

    private static IReadOnlyList<NodeColumnLayoutSnapshot> BuildColumns(float firstColumnLeftNu, float contentTopNu, float firstColumnRightNu, float contentBottomNu, int columnCount, float columnPaddingNu, float topContentPaddingNu, float minorGridSize)
    {
        var resolvedColumnCount = Math.Max(1, columnCount);
        var columnWidthNu = MathF.Max(0.0f, firstColumnRightNu - firstColumnLeftNu);
        var columnGutterNu = columnPaddingNu * 2.0f;
        var guideInsetNu = columnPaddingNu / 2.0f;
        var topGuideInsetNu = topContentPaddingNu / 2.0f;
        var columns = new List<NodeColumnLayoutSnapshot>(resolvedColumnCount);

        for (var index = 0; index < resolvedColumnCount; index++)
        {
            var contentLeftNu = firstColumnLeftNu + (index * (columnWidthNu + columnGutterNu));
            var contentRightNu = contentLeftNu + columnWidthNu;
            var guideLeftNu = contentLeftNu - guideInsetNu;
            var guideRightNu = contentRightNu + guideInsetNu;
            var guideTopNu = contentTopNu - topGuideInsetNu;

            columns.Add(new NodeColumnLayoutSnapshot
            {
                Index = index,
                Bounds = new SKRect(
                    guideLeftNu * minorGridSize,
                    guideTopNu * minorGridSize,
                    guideRightNu * minorGridSize,
                    contentBottomNu * minorGridSize),
                ContentBounds = new SKRect(
                    contentLeftNu * minorGridSize,
                    contentTopNu * minorGridSize,
                    contentRightNu * minorGridSize,
                    contentBottomNu * minorGridSize),
                GuideBounds = new SKRect(
                    guideLeftNu * minorGridSize,
                    guideTopNu * minorGridSize,
                    guideRightNu * minorGridSize,
                    contentBottomNu * minorGridSize),
            });
        }

        return columns;
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

    private static float GetRequiredContentHeightNu(IReadOnlyList<LayoutRow> rows, float rowSpacingNu)
    {
        if (rows.Count == 0)
            return 0.0f;

        return rows
            .GroupBy(row => row.ColumnIndex)
            .Select(group =>
            {
                var orderedRows = group
                    .OrderBy(row => row.YOrder)
                    .ToList();
                var totalHeightNu = orderedRows.Sum(row => row.HeightNu);
                return totalHeightNu + ((orderedRows.Count - 1) * rowSpacingNu);
            })
            .DefaultIfEmpty(0.0f)
            .Max();
    }

    private static float GetRequiredContentWidthNu(IReadOnlyList<LayoutRow> rows, NodeCanvasStyleContext styleContext, float minorGridSize, float inputLabelWidthNu)
    {
        if (rows.Count == 0)
            return 0.0f;

        return rows.Max(row =>
        {
            if (row.IsButtonRow)
            {
                if (row.Items.All(item => item.Component.Model.FixedWidth <= 0.0f))
                    return row.Items.Max(item => item.WidthNu);

                var fixedWidthNu = row.Items
                    .Where(item => item.Component.Model.FixedWidth > 0.0f)
                    .Sum(item => item.WidthNu);
                var flexibleWidthNu = row.Items
                    .Where(item => item.Component.Model.FixedWidth <= 0.0f)
                    .Select(item => item.WidthNu)
                    .DefaultIfEmpty(0.0f)
                    .Max();
                var totalGapNu = MathF.Max(0, row.Items.Count - 1) * GetButtonGapNu(styleContext);
                return MathF.Max(flexibleWidthNu, fixedWidthNu + totalGapNu);
            }

            var component = row.Items[0].Component;
            return component switch
            {
                NodeLabelComponentVM labelComponent => MeasureLabelWidthNu(labelComponent, styleContext, minorGridSize),
                NodeInputComponentVM inputComponent => MeasureInputWidthNu(inputComponent, styleContext, minorGridSize, inputLabelWidthNu),
                NodeValueDisplayComponentVM valueDisplayComponent => MeasureValueDisplayWidthNu(valueDisplayComponent, styleContext, minorGridSize),
                NodeGridInputComponentVM gridInputComponent => MeasureGridInputWidthNu(gridInputComponent, styleContext, minorGridSize),
                NodePortComponentVM portComponent => MeasurePortWidthNu(portComponent, styleContext, minorGridSize),
                NodeButtonComponentVM => MeasureButtonWidthNu(component, styleContext, minorGridSize),
                NodeIconButtonComponentVM => MeasureButtonWidthNu(component, styleContext, minorGridSize),
                NodeIndicatorComponentVM indicatorComponent => MeasureIndicatorWidthNu(indicatorComponent, styleContext, minorGridSize),
                _ => 0.0f,
            };
        });
    }

    private static float MeasureInputLabelWidthNu(NodeDefinitionVM definitionHost, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var labeledValues = definitionHost.NodeComponents
            .Where(component => component is NodeInputComponentVM or NodeValueDisplayComponentVM)
            .ToList();
        if (labeledValues.Count == 0)
            return 0.0f;

        return labeledValues.Max(component => MeasureTextWidthNu(component.Label, styleContext, minorGridSize));
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
        var labelWidthNu = MeasureTextWidthNu(inputComponent.Label, styleContext, minorGridSize, InputTextScale);
        var valueWidthNu = MeasureInputValueWidthNu(inputComponent, styleContext, minorGridSize);
        var horizontalPaddingNu = InputFieldHorizontalPaddingNu * 2.0f;
        var gapNu = labelWidthNu > 0.0f ? InputFieldInternalGapNu : 0.0f;
        return MathF.Ceiling(labelWidthNu + gapNu + valueWidthNu + horizontalPaddingNu);
    }

    private static float MeasureInputValueWidthNu(NodeInputComponentVM inputComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        if (inputComponent.TypeId == InputTypeIds.Boolean)
            return MathF.Max(1.0f, styleContext.FontSize / minorGridSize);

        var valueWidthNu = MeasureTextWidthNu(FormatInputValue(inputComponent.DefaultValue, inputComponent.DateTimeDisplayMode), styleContext, minorGridSize, InputTextScale);
        if (inputComponent.TypeId == InputTypeIds.StringSelection)
            valueWidthNu += 1.5f;

        return MathF.Max(1.0f, valueWidthNu);
    }

    private static float MeasureValueDisplayWidthNu(NodeValueDisplayComponentVM valueDisplayComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var labelWidthNu = MeasureTextWidthNu(valueDisplayComponent.Label, styleContext, minorGridSize, InputTextScale);
        var valueWidthNu = MathF.Max(1.0f, MeasureTextWidthNu(valueDisplayComponent.DefaultValue, styleContext, minorGridSize, InputTextScale));
        var horizontalPaddingNu = InputFieldHorizontalPaddingNu * 2.0f;
        var gapNu = labelWidthNu > 0.0f ? InputFieldInternalGapNu : 0.0f;
        return MathF.Ceiling(labelWidthNu + gapNu + valueWidthNu + horizontalPaddingNu);
    }

    private static float MeasureGridInputWidthNu(NodeGridInputComponentVM gridInputComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var labelWidthNu = MeasureTextWidthNu(gridInputComponent.Label, styleContext, minorGridSize, GridInputTextScale);
        var cellWidthNu = MathF.Max(NodeGridInput.MinimumCellWidth, gridInputComponent.CellWidth);
        var gapNu = MathF.Max(0.0f, styleContext.Padding);
        var totalGapNu = MathF.Max(0, gridInputComponent.Columns - 1) * gapNu;
        var outerPaddingNu = GridInputOuterPaddingNu * 2.0f;
        var labelGapNu = labelWidthNu > 0.0f ? GridInputInternalGapNu : 0.0f;
        var gridWidthNu = (gridInputComponent.Columns * cellWidthNu) + totalGapNu;
        return MathF.Ceiling(labelWidthNu + labelGapNu + gridWidthNu + outerPaddingNu);
    }

    private static float MeasureIndicatorWidthNu(NodeIndicatorComponentVM indicatorComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var labelWidthNu = MeasureTextWidthNu(indicatorComponent.Label, styleContext, minorGridSize, IndicatorTextScale);
        var indicatorSizeNu = (styleContext.FontSize / minorGridSize) * IndicatorSizeScale;
        var horizontalPaddingNu = IndicatorHorizontalPaddingNu * 2.0f;
        var gapNu = labelWidthNu > 0.0f ? IndicatorInternalGapNu : 0.0f;
        return MathF.Ceiling(labelWidthNu + gapNu + indicatorSizeNu + horizontalPaddingNu);
    }

    private static float MeasurePortWidthNu(NodePortComponentVM portComponent, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        var labelWidthNu = MeasureTextWidthNu(portComponent.Label, styleContext, minorGridSize);
        var gapNu = labelWidthNu > 0.0f ? 1.0f : 0.0f;
        return MathF.Ceiling(labelWidthNu + gapNu + MathF.Max(0.0f, styleContext.PortSize));
    }

    private static float MeasureButtonWidthNu(NodeComponentVM component, NodeCanvasStyleContext styleContext, float minorGridSize)
    {
        if (component.Model.FixedWidth > 0.0f)
            return component.Model.FixedWidth;

        var labelWidthNu = MeasureTextWidthNu(component.Label, styleContext, minorGridSize);
        return MathF.Ceiling(MathF.Max(ButtonMinimumWidthNu, labelWidthNu + (ButtonHorizontalPaddingNu * 2.0f)));
    }

    private static float GetButtonGapNu(NodeCanvasStyleContext styleContext)
    {
        return MathF.Max(0.0f, styleContext.Padding);
    }

    private static float MeasureTextWidthNu(string? text, NodeCanvasStyleContext styleContext, float minorGridSize, float textScale = 1.0f)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0.0f;

        using var typeface = string.IsNullOrWhiteSpace(styleContext.FontFamily)
            ? null
            : SKTypeface.FromFamilyName(styleContext.FontFamily);

        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = styleContext.FontSize * textScale,
            IsAntialias = true,
        };

        var textWidth = textPaint.MeasureText(text);
        if (textWidth <= 0.0f)
            return 0.0f;

        return MathF.Ceiling(textWidth / minorGridSize);
    }

    private static string FormatInputValue(object? value, DateTimeDisplayMode dateTimeDisplayMode)
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

    private static float GetComponentHeightNu(NodeComponentVM component, float lineHeightNu, NodeCanvasStyleContext styleContext)
    {
        return component switch
        {
            NodeSpacerComponentVM spacerComponent => MathF.Max(1.0f, spacerComponent.Height),
            NodeGridInputComponentVM gridInputComponent => MathF.Max(lineHeightNu, gridInputComponent.Rows * lineHeightNu),
            NodePortComponentVM => MathF.Max(lineHeightNu, MathF.Max(1.0f, styleContext.PortSize)),
            NodeSeparatorComponentVM => 2.0f,
            _ => lineHeightNu,
        };
    }

    private static IReadOnlyList<LayoutRow> BuildRows(NodeDefinitionVM definitionHost, NodeCanvasStyleContext styleContext, float lineHeightNu, int columnCount)
    {
        var orderedComponents = definitionHost.NodeComponents
            .Select((component, index) => new OrderedComponent(component, index))
            .OrderBy(item => ResolveLayoutColumnIndex(item.Component, columnCount))
            .ThenBy(item => item.Component.YOrder)
            .ThenBy(item => IsButtonLikeComponent(item.Component) ? item.Component.XOrder : 0)
            .ThenBy(item => item.Index)
            .ToList();

        var rows = new List<LayoutRow>();

        foreach (var item in orderedComponents)
        {
            var columnIndex = ResolveLayoutColumnIndex(item.Component, columnCount);
            if (IsButtonLikeComponent(item.Component))
            {
                var existingRow = rows.LastOrDefault();
                if (existingRow != null
                    && existingRow.ColumnIndex == columnIndex
                    && existingRow.IsButtonRow
                    && existingRow.YOrder == item.Component.YOrder)
                {
                    existingRow.Items.Add(new LayoutRowItem(item.Component, MeasureButtonWidthNu(item.Component, styleContext, 1.0f)));
                    continue;
                }

                rows.Add(new LayoutRow(columnIndex, item.Component.YOrder, true, GetComponentHeightNu(item.Component, lineHeightNu, styleContext))
                {
                    Items =
                    [
                        new LayoutRowItem(item.Component, MeasureButtonWidthNu(item.Component, styleContext, 1.0f))
                    ]
                });
                continue;
            }

            rows.Add(new LayoutRow(columnIndex, item.Component.YOrder, false, GetComponentHeightNu(item.Component, lineHeightNu, styleContext))
            {
                Items =
                [
                    new LayoutRowItem(item.Component, 0.0f)
                ]
            });
        }

        return rows;
    }

    private static int ResolveLayoutColumnIndex(NodeComponentVM component, int columnCount)
    {
        var resolvedColumnCount = Math.Max(1, columnCount);

        if (component is NodePortComponentVM portComponent)
        {
            return portComponent.Side == NodePortSide.Left
                ? 0
                : resolvedColumnCount - 1;
        }

        return Math.Clamp(component.Column, 0, resolvedColumnCount - 1);
    }

    private static bool IsButtonLikeComponent(NodeComponentVM component)
    {
        return component is NodeButtonComponentVM or NodeIconButtonComponentVM;
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

    private static int EnsureEven(int value)
    {
        return value % 2 == 0 ? value : value + 1;
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

    private static bool AreClose(float left, float right)
    {
        return MathF.Abs(left - right) < 0.01f;
    }

    private sealed record OrderedComponent(NodeComponentVM Component, int Index);

    private sealed class LayoutRow(int columnIndex, int yOrder, bool isButtonRow, float heightNu)
    {
        public int ColumnIndex { get; } = columnIndex;
        public int YOrder { get; } = yOrder;
        public bool IsButtonRow { get; } = isButtonRow;
        public float HeightNu { get; } = heightNu;
        public List<LayoutRowItem> Items { get; init; } = [];
    }

    private sealed record LayoutRowItem(NodeComponentVM Component, float WidthNu);

    #endregion Private Methods
}
