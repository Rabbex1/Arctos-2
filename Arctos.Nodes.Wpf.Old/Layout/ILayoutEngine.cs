using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Rendering;

namespace Arctos.Nodes.Wpf.Layout;

/// <summary>
/// Defines a layout engine that resolves persisted placement intent into a transient layout snapshot.
/// </summary>
public interface ILayoutEngine
{
    /// <summary>
    /// Rebuilds the current layout snapshot for the provided host context.
    /// </summary>
    /// <param name="host">The current model host shown on the canvas.</param>
    /// <param name="overview"><see langword="true"/> when the host is in overview mode; otherwise, <see langword="false"/>.</param>
    /// <param name="mainGridSize">The current <c>1nl</c> size in world-space units.</param>
    /// <param name="minorGridSize">The current <c>1nu</c> size in world-space units.</param>
    /// <returns>The rebuilt transient layout snapshot, or <see langword="null"/> when the engine does not apply.</returns>
    ILayoutSnapshot? Rebuild(IModelHost? host, bool overview, float mainGridSize, float minorGridSize, NodeCanvasStyleContext styleContext);
}
