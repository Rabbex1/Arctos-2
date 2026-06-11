namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the resolved in-memory node map used by <c>Arctos.Nodes</c>.
/// </summary>
public sealed class NodeMap
{
    #region Fields

    private readonly List<NodeLibrary> _libraries = [];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the stable identifier of the map.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the map settings.
    /// </summary>
    public NodeMapSettings Settings { get; }

    /// <summary>
    /// Gets or sets the default node style used by the map.
    /// </summary>
    public NodeStyle Style { get; set; } = new();

    /// <summary>
    /// Gets the resolved in-memory libraries used by the map.
    /// </summary>
    public IReadOnlyList<NodeLibrary> Libraries => _libraries;

    /// <summary>
    /// Gets the graph owned by the map.
    /// </summary>
    public NodeGraph Graph { get; }

    #endregion Properties

    #region Constructors

    public NodeMap(NodeMapSettings settings, NodeGraph graph, IEnumerable<NodeLibrary> libraries)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));

        ArgumentNullException.ThrowIfNull(libraries);
        _libraries.AddRange(libraries);

        if (_libraries.Count == 0)
            throw new ArgumentException("A node map must contain at least one library.", nameof(libraries));

        Settings.ContainsGraph = true;
    }

    #endregion Constructors
}
