using Arctos.Nodes.Wpf.Layout;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders passive column handles along the bottom edge of the node header.
/// </summary>
public sealed class NodeColumnHandlesVisual : IVisual
{
    private static readonly IReadOnlyList<IVisual> EmptyChildren = [];
    private readonly IReadOnlyList<NodeColumnLayoutSnapshot> _columns;
    private readonly SKColor _fillColor;
    private readonly float _handleHeight;
    private readonly float _handleWidth;
    private readonly float _strokeWidth;
    private readonly SKColor _strokeColor;
    private readonly float _topEdgeY;
    private SKRect _bounds;

    /// <summary>
    /// Creates a new column handle visual.
    /// </summary>
    /// <param name="columns">The resolved column layout entries.</param>
    /// <param name="topEdgeY">The world-space Y position of the header bottom edge.</param>
    /// <param name="fillColor">The handle fill color.</param>
    /// <param name="strokeColor">The handle stroke color.</param>
    /// <param name="handleWidth">The world-space handle width.</param>
    /// <param name="handleHeight">The world-space handle height.</param>
    /// <param name="strokeWidth">The world-space stroke width.</param>
    public NodeColumnHandlesVisual(
        IReadOnlyList<NodeColumnLayoutSnapshot> columns,
        float topEdgeY,
        SKColor fillColor,
        SKColor strokeColor,
        float handleWidth,
        float handleHeight,
        float strokeWidth)
    {
        _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        _topEdgeY = topEdgeY;
        _fillColor = fillColor;
        _strokeColor = strokeColor;
        _handleWidth = MathF.Max(1.0f, handleWidth);
        _handleHeight = MathF.Max(1.0f, handleHeight);
        _strokeWidth = MathF.Max(0.0f, strokeWidth);
        _bounds = GetBounds(columns, topEdgeY, _handleWidth, _handleHeight);
    }

    /// <inheritdoc/>
    public IReadOnlyList<IVisual> Children => EmptyChildren;

    /// <inheritdoc/>
    public SKRect Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }

    /// <inheritdoc/>
    public void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        if (_columns.Count == 0)
            return;

        var radius = _handleHeight * 0.5f;
        using var fillPaint = new SKPaint
        {
            Color = _fillColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var strokePaint = new SKPaint
        {
            Color = _strokeColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _strokeWidth,
        };

        foreach (var column in _columns)
        {
            var handleBounds = GetHandleBounds(column, _topEdgeY, _handleWidth, _handleHeight);
            canvas.DrawRoundRect(handleBounds, radius, radius, fillPaint);
            canvas.DrawRoundRect(handleBounds, radius, radius, strokePaint);
        }
    }

    private static SKRect GetHandleBounds(NodeColumnLayoutSnapshot column, float topEdgeY, float handleWidth, float handleHeight)
    {
        var centerX = column.ContentBounds.MidX;
        var halfWidth = handleWidth * 0.5f;
        var halfHeight = handleHeight * 0.5f;
        return new SKRect(
            centerX - halfWidth,
            topEdgeY - halfHeight,
            centerX + halfWidth,
            topEdgeY + halfHeight);
    }

    private static SKRect GetBounds(IReadOnlyList<NodeColumnLayoutSnapshot> columns, float topEdgeY, float handleWidth, float handleHeight)
    {
        if (columns.Count == 0)
            return SKRect.Empty;

        var bounds = GetHandleBounds(columns[0], topEdgeY, handleWidth, handleHeight);
        for (var index = 1; index < columns.Count; index++)
        {
            var next = GetHandleBounds(columns[index], topEdgeY, handleWidth, handleHeight);
            bounds = new SKRect(
                MathF.Min(bounds.Left, next.Left),
                MathF.Min(bounds.Top, next.Top),
                MathF.Max(bounds.Right, next.Right),
                MathF.Max(bounds.Bottom, next.Bottom));
        }

        return bounds;
    }
}
