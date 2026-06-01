using System.Collections.Generic;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the persisted node graph content of a graph document.
/// </summary>
public sealed class NodeGraph
{
    #region Properties

    /// <summary>
    /// Gets or sets the root nodes of the document graph.
    /// </summary>
    public List<LeafNode> Nodes { get; set; } = [];

    #endregion Properties
}
