namespace Arctos.Nodes.Models;

/// <summary>
/// Describes a reference to a shared node library.
/// </summary>
public sealed class NodeLibraryReference
{
    public required string Name { get; set; }

    public required string Version { get; set; }
}
