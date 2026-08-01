namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a port component inside a detailed node.
/// </summary>
public sealed class NodePort : NodeComponent
{
    #region Properties

    /// <summary>
    /// Gets or sets the persisted port side anchor.
    /// </summary>
    public NodePortSide Side { get; set; } = NodePortSide.Left;

    /// <summary>
    /// Gets or sets the persisted port shape preset.
    /// </summary>
    public NodePortShape Shape { get; set; } = NodePortShape.Circle;

    /// <summary>
    /// Gets or sets the authored fill color for the port glyph.
    /// </summary>
    public NodeColor FillColor { get; set; } = NodeColor.White;

    #endregion Properties
}

