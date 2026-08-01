namespace Arctos.Nodes.Wpf.DragImage;

/// <summary>
/// Exposes an owned cached drag image for a model host.
/// </summary>
public interface IDragImageHost
{
    /// <summary>
    /// Gets or sets the current cached drag image for this host.
    /// </summary>
    DragImage? DragImage { get; set; }
}
