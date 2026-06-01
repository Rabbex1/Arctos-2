namespace Arctos.Nodes.Models;

/// <summary>
/// Provides event data for node document manager add/remove notifications.
/// </summary>
public sealed class NodeDocumentEventArgs : EventArgs
{
    #region Properties

    /// <summary>
    /// Gets the affected node document.
    /// </summary>
    public NodeDocument Document { get; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new event argument instance for a node document manager event.
    /// </summary>
    /// <param name="document">The affected document.</param>
    public NodeDocumentEventArgs(NodeDocument document)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
    }

    #endregion Constructors
}
