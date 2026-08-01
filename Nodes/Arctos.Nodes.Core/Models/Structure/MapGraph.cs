namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents the visual layout owned by a map, including placed child maps, placed nodes, and their connections.
/// </summary>
public sealed class MapGraph
{
    #region Properties

    /// <summary>
    /// Gets or sets the child maps placed on the current map surface.
    /// </summary>
    public List<MapPlacement> MapPlacements { get; set; } = [];

    /// <summary>
    /// Gets or sets the node definitions placed on the current map surface.
    /// </summary>
    public List<NodePlacement> NodePlacements { get; set; } = [];

    /// <summary>
    /// Gets or sets the connections owned by the current map surface.
    /// </summary>
    public List<MapConnection> Connections { get; set; } = [];

    #endregion Properties
}
