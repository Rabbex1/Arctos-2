namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a connection between two placements on a map surface.
/// </summary>
public sealed class MapConnection
{
    #region Properties

    /// <summary>
    /// Gets or sets the stable connection identifier unique within the owning graph.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the source placement identifier.
    /// </summary>
    public required string SourcePlacementId { get; set; }

    /// <summary>
    /// Gets or sets the optional source component or port UID used by the connection.
    /// </summary>
    public string? SourcePortUid { get; set; }

    /// <summary>
    /// Gets or sets the target placement identifier.
    /// </summary>
    public required string TargetPlacementId { get; set; }

    /// <summary>
    /// Gets or sets the optional target component or port UID used by the connection.
    /// </summary>
    public string? TargetPortUid { get; set; }

    #endregion Properties
}
