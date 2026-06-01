using System.Collections.Generic;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a placed node that owns child nodes within the document graph hierarchy.
/// </summary>
public sealed class CompositeNode : LeafNode
{
    #region Properties

    /// <summary>
    /// Gets or sets the child nodes contained by the composite node.
    /// </summary>
    public List<LeafNode> Children { get; set; } = [];

    #endregion Properties
}
