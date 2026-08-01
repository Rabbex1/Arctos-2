using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Defines a renderable visual node in the Skia visual tree.
/// </summary>
public interface IVisual
{
    /// <summary>Gets the child visuals for this visual node.</summary>
    IReadOnlyList<IVisual> Children { get; }

    /// <summary>Gets or sets the current layout bounds for this visual node.</summary>
    SKRect Bounds { get; set; }

    /// <summary>Renders this visual node to the provided Skia canvas.</summary>
    /// <param name="canvas">The canvas to render to.</param>
    void Render(SKCanvas canvas);
}
