using Arctos.Nodes.Wpf.ViewModels.Components;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Stores the resolved component and hit-region metadata for a single hidden hit-test color.
/// </summary>
internal readonly record struct ComponentHitResult(NodeComponentVM? Component, ComponentHitFlags Flags)
{
    /// <summary>
    /// Gets the empty no-hit result.
    /// </summary>
    public static ComponentHitResult None => new(null, ComponentHitFlags.None);
}
