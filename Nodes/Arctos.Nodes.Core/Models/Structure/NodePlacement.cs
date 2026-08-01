namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a node definition placed on a map surface.
/// </summary>
public sealed class NodePlacement
{
    #region Properties

    /// <summary>
    /// Gets or sets the stable placement identifier unique within the owning graph.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the stable node type name resolved against the owning map definitions.
    /// </summary>
    public required string NodeTypeName { get; set; }

    /// <summary>
    /// Gets or sets the placement position in map space.
    /// </summary>
    public NodePoint Position { get; set; }

    /// <summary>
    /// Gets or sets the optional explicit size override for the placed node.
    /// </summary>
    public NodeSize? Size { get; set; }

    #endregion Properties
}
