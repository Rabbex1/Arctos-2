namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the persisted <c>.nmap</c> file shape.
/// </summary>
public sealed class NodeMapFile
{
    #region Properties

    public required Guid Id { get; set; }

    public required NodeMapSettings Settings { get; set; }

    public required MapStyle Style { get; set; }

    public NodeLibrary? LocalLibrary { get; set; }

    public List<NodeLibraryReference> LibraryReferences { get; set; } = [];

    public required NodeGraph Graph { get; set; }

    #endregion Properties

    #region Public Methods

    public NodeMap ToMap(Func<NodeLibraryReference, NodeLibrary?> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        var libraries = new List<NodeLibrary>();

        if (LocalLibrary != null)
            libraries.Add(LocalLibrary);

        foreach (var reference in LibraryReferences)
        {
            var library = resolver(reference);
            if (library == null)
                throw new InvalidOperationException(
                    $"The required node library '{reference.Name}' version '{reference.Version}' could not be resolved.");

            libraries.Add(library);
        }

        if (libraries.Count == 0)
            throw new InvalidOperationException("A node map file must resolve at least one library.");

        return new NodeMap(Settings, Graph, libraries)
        {
            Id = Id,
            Style = Style,
        };
    }

    public static NodeMapFile FromMap(NodeMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        NodeLibrary? localLibrary = null;
        var references = new List<NodeLibraryReference>();

        foreach (var library in map.Libraries)
        {
            if (string.IsNullOrWhiteSpace(library.Name) || string.IsNullOrWhiteSpace(library.Version))
            {
                if (localLibrary != null)
                {
                    throw new InvalidOperationException(
                        "A node map can only contain one embedded local library. Additional libraries must provide a Name and Version.");
                }

                localLibrary = library;
                continue;
            }

            references.Add(new NodeLibraryReference
            {
                Name = library.Name,
                Version = library.Version,
            });
        }

        if (localLibrary == null && references.Count == 0)
            throw new InvalidOperationException("A node map must contain at least one library.");

        return new NodeMapFile
        {
            Id = map.Id,
            Settings = map.Settings,
            Style = map.Style,
            LocalLibrary = localLibrary,
            LibraryReferences = references,
            Graph = map.Graph,
        };
    }

    #endregion Public Methods
}
