using Arctos.Nodes.Models;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Exposes transient canvas pan and zoom state for hosts that want to preserve their last viewport.
/// </summary>
public interface ICanvasViewStateHost
{
    /// <summary>
    /// Gets or sets the last resolved canvas pan for this host.
    /// </summary>
    NodePoint CanvasPan { get; set; }

    /// <summary>
    /// Gets or sets the last resolved canvas zoom for this host.
    /// A value less than or equal to zero indicates that no viewport state has been initialized yet.
    /// </summary>
    float CanvasZoom { get; set; }
}
