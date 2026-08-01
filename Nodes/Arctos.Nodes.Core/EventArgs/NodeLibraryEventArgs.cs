namespace Arctos.Nodes.Core.EventArgs;

/// <summary>
/// Provides event data for shared node library add/remove notifications.
/// </summary>
public sealed class NodeLibraryEventArgs : System.EventArgs
{
    #region Properties

    /// <summary>
    /// Gets the affected shared library.
    /// </summary>
    public NodeLibrary Library { get; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new event argument instance for a node library manager event.
    /// </summary>
    /// <param name="library">The affected library.</param>
    public NodeLibraryEventArgs(NodeLibrary library)
    {
        Library = library ?? throw new ArgumentNullException(nameof(library));
    }

    #endregion Constructors
}

