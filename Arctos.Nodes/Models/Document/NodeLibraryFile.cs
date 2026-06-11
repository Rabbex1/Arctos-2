namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the persisted <c>.nlib</c> file shape.
/// </summary>
public sealed class NodeLibraryFile
{
    #region Properties

    public required string Name { get; set; }

    public required string Version { get; set; }

    public required NodeLibrary Library { get; set; }

    #endregion Properties

    #region Public Methods

    public NodeLibrary ToLibrary()
    {
        if (!string.IsNullOrWhiteSpace(Library.Name) && !string.Equals(Library.Name, Name, StringComparison.Ordinal))
            throw new InvalidOperationException("The node library file name does not match the contained library name.");

        if (!string.IsNullOrWhiteSpace(Library.Version) && !string.Equals(Library.Version, Version, StringComparison.Ordinal))
            throw new InvalidOperationException("The node library file version does not match the contained library version.");

        Library.Name = Name;
        Library.Version = Version;
        return Library;
    }

    public static NodeLibraryFile FromLibrary(NodeLibrary library)
    {
        ArgumentNullException.ThrowIfNull(library);

        if (string.IsNullOrWhiteSpace(library.Name))
            throw new InvalidOperationException("A shared node library must have a Name before it can be saved.");

        if (string.IsNullOrWhiteSpace(library.Version))
            throw new InvalidOperationException("A shared node library must have a Version before it can be saved.");

        return new NodeLibraryFile
        {
            Name = library.Name,
            Version = library.Version,
            Library = library,
        };
    }

    #endregion Public Methods
}
