using SkiaSharp;

namespace Arctos.Nodes.Wpf.Layout;

/// <summary>
/// Stores the resolved transient layout information for a detailed node.
/// </summary>
public sealed class NodeLayoutSnapshot : ILayoutSnapshot
{
    /// <summary>
    /// Gets the node outer bounds in world-space units.
    /// </summary>
    public required SKRect NodeBounds { get; init; }

    /// <summary>
    /// Gets the rendered node bounds in world-space units, including component visuals that extend outside the body.
    /// </summary>
    public required SKRect VisualBounds { get; init; }

    /// <summary>
    /// Gets the optional header bounds in world-space units.
    /// </summary>
    public SKRect? HeaderBounds { get; init; }

    /// <summary>
    /// Gets the resolved content bounds in world-space units.
    /// </summary>
    public required SKRect ContentBounds { get; init; }

    /// <summary>
    /// Gets the resolved line height in world-space units.
    /// </summary>
    public required float LineHeight { get; init; }

    /// <summary>
    /// Gets the shared input label width for the current column in world-space units.
    /// </summary>
    public required float InputLabelWidth { get; init; }

    /// <summary>
    /// Gets the resolved component layout entries.
    /// </summary>
    public required IReadOnlyList<NodeComponentLayoutSnapshot> Components { get; init; }
}
