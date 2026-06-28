namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a passive indicator component used to display application state.
/// </summary>
public sealed class NodeIndicator : NodeComponent
{
    /// <summary>
    /// Gets or sets the default indicator color.
    /// </summary>
    public NodeColor Color { get; set; } = NodeColor.White;

    /// <summary>
    /// Gets or sets the indicator shape.
    /// </summary>
    public NodeIndicatorShape Shape { get; set; } = NodeIndicatorShape.Round;
}
