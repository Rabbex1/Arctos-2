namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a persisted row container inside a node column.
/// </summary>
public sealed class NodeRow
{
    /// <summary>
    /// Gets or sets the stable row identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the zero-based row order within the owning column.
    /// </summary>
    public int YOrder { get; set; }

    /// <summary>
    /// Gets or sets the number of columns this row spans, starting from its owning column.
    /// </summary>
    public int ColumnSpan { get; set; } = 1;

    /// <summary>
    /// Gets the components owned by this row.
    /// </summary>
    public List<NodeComponent> Components { get; set; } = [];
}
