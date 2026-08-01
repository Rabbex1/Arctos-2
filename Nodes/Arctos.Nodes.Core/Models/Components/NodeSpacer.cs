namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents an empty spacing component inside a detailed node.
/// </summary>
public sealed class NodeSpacer : NodeComponent
{
    #region Fields

    /// <summary>
    /// Defines the valid spacer size range in minor node units.
    /// </summary>
    public static readonly IntRange SizeRange = new(1, 32);

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the spacer height expressed in minor node units.
    /// </summary>
    public int Height { get; set; } = 4;

    /// <summary>
    /// Gets or sets a value indicating whether this spacer expands to consume leftover column height.
    /// </summary>
    public bool Expand { get; set; }

    #endregion Properties
}

