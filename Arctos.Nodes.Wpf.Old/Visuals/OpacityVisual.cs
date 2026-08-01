using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Wraps a child visual and renders it with a uniform opacity.
/// </summary>
public sealed class OpacityVisual : IVisual
{
    private readonly IReadOnlyList<IVisual> _children;

    /// <summary>
    /// Creates a new opacity wrapper for the supplied child visual.
    /// </summary>
    /// <param name="child">The child visual to render.</param>
    /// <param name="opacity">The uniform opacity scale to apply.</param>
    public OpacityVisual(IVisual child, float opacity)
    {
        ArgumentNullException.ThrowIfNull(child);

        Child = child;
        Opacity = Math.Clamp(opacity, 0.0f, 1.0f);
        _children = [child];
    }

    /// <summary>
    /// Gets the wrapped child visual.
    /// </summary>
    public IVisual Child { get; }

    /// <summary>
    /// Gets the uniform opacity applied to the child visual.
    /// </summary>
    public float Opacity { get; }

    /// <inheritdoc />
    public IReadOnlyList<IVisual> Children => _children;

    /// <inheritdoc />
    public SKRect Bounds
    {
        get => Child.Bounds;
        set => Child.Bounds = value;
    }

    /// <inheritdoc />
    public void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        if (Opacity <= 0.0f)
            return;

        if (Opacity >= 1.0f)
        {
            Child.Render(canvas);
            return;
        }

        using var opacityPaint = new SKPaint
        {
            Color = SKColors.White.WithAlpha((byte)Math.Clamp(MathF.Round(Opacity * 255.0f), 0.0f, 255.0f)),
        };

        canvas.SaveLayer(Bounds, opacityPaint);
        Child.Render(canvas);
        canvas.Restore();
    }
}
