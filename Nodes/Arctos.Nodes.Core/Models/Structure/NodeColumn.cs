namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a persisted column container inside a node definition.
/// </summary>
public sealed class NodeColumn
{
    /// <summary>
    /// Gets or sets the stable column identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the zero-based horizontal order of the column within the node.
    /// </summary>
    public int XOrder { get; set; }

    /// <summary>
    /// Gets or sets how the column resolves its width.
    /// </summary>
    public NodeColumnWidthMode WidthMode { get; set; } = NodeColumnWidthMode.Auto;

    /// <summary>
    /// Gets or sets the explicit width of the column in node units when <see cref="WidthMode"/> is <see cref="NodeColumnWidthMode.Fixed"/>.
    /// </summary>
    public float WidthNu { get; set; }

    /// <summary>
    /// Gets the rows owned by this column.
    /// </summary>
    public List<NodeRow> Rows { get; set; } = [];
}

