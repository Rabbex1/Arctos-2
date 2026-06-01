using System.Text.Json.Serialization;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a placed node in the document graph.
/// </summary>
[JsonDerivedType(typeof(LeafNode), typeDiscriminator: "LeafNode")]
[JsonDerivedType(typeof(CompositeNode), typeDiscriminator: "CompositeNode")]
public class LeafNode
{
    #region Properties

    /// <summary>
    /// Gets or sets the stable node identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the node definition name used to resolve the node from the document library.
    /// </summary>
    public required string NodeDefinitionName { get; set; }

    /// <summary>
    /// Gets or sets the node position in document space.
    /// </summary>
    public NodePoint Position { get; set; }

    /// <summary>
    /// Gets or sets the optional explicit node size in document space.
    /// </summary>
    public NodeSize? Size { get; set; }

    #endregion Properties
}
