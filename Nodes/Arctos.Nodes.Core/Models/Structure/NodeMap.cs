namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a self-contained persisted map, including its embedded node definitions, child maps, and visual layout.
/// </summary>
public sealed class NodeMap
{
    #region Properties

    /// <summary>
    /// Gets the stable identifier of the map.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the map settings.
    /// </summary>
    public NodeMapSettings Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the document-level visual language and layout rules used by the map.
    /// </summary>
    public MapStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets the node definitions embedded directly in the map.
    /// </summary>
    public List<NodeDefinition> Definitions { get; set; } = [];

    /// <summary>
    /// Gets or sets the child maps embedded directly in the map.
    /// </summary>
    public List<NodeMap> SubMaps { get; set; } = [];

    /// <summary>
    /// Gets or sets the visual layout owned by the map.
    /// </summary>
    public MapGraph Graph { get; set; } = new();

    #endregion Properties

    #region Public Methods

    /// <summary>
    /// Attempts to resolve an embedded node definition by type name.
    /// </summary>
    public bool TryGetDefinition(string typeName, out NodeDefinition? definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        definition = Definitions.FirstOrDefault(item => string.Equals(item.TypeName, typeName, StringComparison.Ordinal));
        return definition != null;
    }

    /// <summary>
    /// Attempts to resolve an embedded child map by identifier.
    /// </summary>
    public bool TryGetSubMap(Guid mapId, out NodeMap? map)
    {
        map = SubMaps.FirstOrDefault(item => item.Id == mapId);
        return map != null;
    }

    #endregion Public Methods
}

