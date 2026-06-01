using Arctos.Nodes.Models;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Defines the minimum contract a shell document must expose to participate in the shared nodes session.
/// </summary>
public interface INodesDocumentHost
{
    /// <summary>
    /// Gets the graph document presented by the host document.
    /// </summary>
    NodeDocument NodeDocument { get; }
}
