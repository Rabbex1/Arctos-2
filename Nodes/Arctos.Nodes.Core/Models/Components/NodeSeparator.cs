namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a separator component inside a detailed node.
/// </summary>
public sealed class NodeSeparator : NodeComponent
{
    #region Properties

    /// <summary>
    /// Gets or sets the color source used to render the separator line.
    /// </summary>
    public NodeSeparatorColorMode ColorMode { get; set; } = NodeSeparatorColorMode.Border;

    #endregion Properties
}

