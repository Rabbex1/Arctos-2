using SkiaSharp;

namespace Arctos.Nodes.Wpf.Layout;

/// <summary>
/// Stores the resolved transient layout information for a node content column.
/// </summary>
public sealed class NodeColumnLayoutSnapshot
{
    /// <summary>
    /// Gets the zero-based column index.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Gets the full column slot bounds in world-space units.
    /// </summary>
    public required SKRect Bounds { get; init; }

    /// <summary>
    /// Gets the padded content bounds for the column in world-space units.
    /// </summary>
    public required SKRect ContentBounds { get; init; }

    /// <summary>
    /// Gets the edit-mode guide bounds for the column in world-space units.
    /// </summary>
    public required SKRect GuideBounds { get; init; }
}
