using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.Components;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Layout;

/// <summary>
/// Stores the resolved transient layout information for a single node component.
/// </summary>
public sealed class NodeComponentLayoutSnapshot
{
    /// <summary>
    /// Gets the component that owns this layout entry.
    /// </summary>
    public required NodeComponentVM Component { get; init; }

    /// <summary>
    /// Gets the resolved component position in minor node units.
    /// </summary>
    public required NodePoint PositionNu { get; init; }

    /// <summary>
    /// Gets the resolved zero-based layout column used by the component.
    /// </summary>
    public required int ColumnIndex { get; init; }

    /// <summary>
    /// Gets the resolved zero-based row order used by the component.
    /// </summary>
    public required int YOrder { get; init; }

    /// <summary>
    /// Gets the resolved zero-based inline order used by the component.
    /// </summary>
    public required int XOrder { get; init; }

    /// <summary>
    /// Gets the resolved component bounds in world-space units.
    /// </summary>
    public required SKRect Bounds { get; init; }

    /// <summary>
    /// Gets the rendered component bounds in world-space units.
    /// </summary>
    public required SKRect VisualBounds { get; init; }
}
