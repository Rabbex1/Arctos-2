using System.Collections.Generic;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the node graph content of a node map.
/// </summary>
public sealed class NodeGraph
{
    #region Properties

    /// <summary>
    /// Gets or sets the root nodes of the map graph.
    /// </summary>
    public List<LeafNode> Nodes { get; set; } = [];

    #endregion Properties
}
