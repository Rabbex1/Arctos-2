using Arctos.Nodes.Wpf.Layout;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders edit-mode column guides for a detailed node layout.
/// </summary>
public sealed class NodeColumnGuidesVisual : IVisual
{
    private static readonly IReadOnlyList<IVisual> EmptyChildren = [];
    private readonly IReadOnlyList<NodeColumnLayoutSnapshot> _columns;
    private readonly float _dashLength;
    private readonly SKColor _strokeColor;
    private SKRect _bounds;

    /// <summary>
    /// Creates a new column guide visual.
    /// </summary>
    /// <param name="columns">The resolved column layout entries.</param>
    /// <param name="strokeColor">The guide stroke color.</param>
    /// <param name="dashLength">The dash and gap length in world-space units.</param>
    public NodeColumnGuidesVisual(IReadOnlyList<NodeColumnLayoutSnapshot> columns, SKColor strokeColor, float dashLength)
    {
        _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        _strokeColor = strokeColor;
        _dashLength = MathF.Max(1.0f, dashLength);
        _bounds = GetBounds(columns);
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

        if (_columns.Count <= 1)
            return;

        using var paint = new SKPaint
        {
            Color = _strokeColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
            PathEffect = SKPathEffect.CreateDash(
            [
                _dashLength,
                _dashLength,
            ], 0.0f),
        };

        foreach (var column in _columns)
        {
            if (!column.GuideBounds.IsEmpty)
                canvas.DrawRect(column.GuideBounds, paint);
        }
    }

    private static SKRect GetBounds(IReadOnlyList<NodeColumnLayoutSnapshot> columns)
    {
        if (columns.Count == 0)
            return SKRect.Empty;

        var bounds = columns[0].GuideBounds;
        for (var index = 1; index < columns.Count; index++)
        {
            bounds = new SKRect(
                MathF.Min(bounds.Left, columns[index].GuideBounds.Left),
                MathF.Min(bounds.Top, columns[index].GuideBounds.Top),
                MathF.Max(bounds.Right, columns[index].GuideBounds.Right),
                MathF.Max(bounds.Bottom, columns[index].GuideBounds.Bottom));
        }

        return bounds;
    }
}
