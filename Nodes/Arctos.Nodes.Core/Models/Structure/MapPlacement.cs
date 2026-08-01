namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a child map placed on a parent map surface.
/// </summary>
public sealed class MapPlacement
{
    #region Properties

    /// <summary>
    /// Gets or sets the stable placement identifier unique within the owning graph.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the child map this placement represents.
    /// </summary>
    public Guid MapId { get; set; }

    /// <summary>
    /// Gets or sets the placement position in map space.
    /// </summary>
    public NodePoint Position { get; set; }

    /// <summary>
    /// Gets or sets the optional explicit size override for the placed child map.
    /// </summary>
    public NodeSize? Size { get; set; }

    #endregion Properties
}
