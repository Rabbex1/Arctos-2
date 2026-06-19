namespace Arctos.Nodes.Models;

/// <summary>
/// Describes the per-node style overrides that may be applied at definition or instance level.
/// </summary>
public sealed class NodeStyle
{
    #region Properties

    /// <summary>
    /// Gets or sets the body and main text styling for the node.
    /// </summary>
    public NodeShapeStyle ShapeStyle { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional header styling for the node. When <see langword="null"/>, the node has no header style defined at this level.
    /// </summary>
    public NodeHeaderStyle? HeaderStyle { get; set; }

    /// <summary>
    /// Gets or sets the override rules that control whether lower levels may override this style.
    /// </summary>
    public NodeStyleOverrideRules OverrideRules { get; set; } = new();

    #endregion Properties
}
