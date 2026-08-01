using Arctos.Nodes.Wpf.ViewModels.Components;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Represents the resolved metadata for a single hidden-layer hit test entry.
/// </summary>
public readonly record struct HitTestResult(object? Target, HitRegionFlags Flags)
{
    /// <summary>
    /// Gets an empty hit result.
    /// </summary>
    public static HitTestResult None => new(null, HitRegionFlags.None);

    /// <summary>
    /// Gets the resolved component when the target is a node component.
    /// </summary>
    public NodeComponentVM? Component => Target as NodeComponentVM;
}
