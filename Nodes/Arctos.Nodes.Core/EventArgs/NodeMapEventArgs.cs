namespace Arctos.Nodes.Core.EventArgs;

/// <summary>
/// Provides event data for node map manager add/remove notifications.
/// </summary>
public sealed class NodeMapEventArgs : System.EventArgs
{
    #region Properties

    /// <summary>
    /// Gets the affected node map.
    /// </summary>
    public NodeMap Map { get; }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new event argument instance for a node map manager event.
    /// </summary>
    /// <param name="map">The affected map.</param>
    public NodeMapEventArgs(NodeMap map)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
    }

    #endregion Constructors
}

