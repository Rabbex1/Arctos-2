using System.Collections.Generic;

namespace Arctos.Nodes.Models;

/// <summary>
/// Defines a reusable application-facing node available inside a node library.
/// </summary>
public sealed class NodeDefinition
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the stable node type name used by the owning application and toolbox.
    /// </summary>
    public required string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the human-readable node display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the application-defined domain meaning of the node type, such as <c>Server</c> or <c>Routine</c>.
    /// </summary>
    public required string DomainType { get; set; }

    /// <summary>
    /// Gets or sets the primitive editor shape used by the node.
    /// </summary>
    public NodeShape NodeShape { get; set; } = NodeShape.RoundedRectangle;

    /// <summary>
    /// Gets or sets the intrinsic node size in minor node-space units.
    /// </summary>
    public NodeSize Size { get; set; } = NodeSize.CreateDefault(NodeShape.RoundedRectangle);

    /// <summary>
    /// Gets or sets the rules that constrain how the selected <see cref="NodeShape"/> may host labels, ports, and internal components.
    /// </summary>
    public ShapeRules ShapeRules { get; set; } = ShapeRules.CreateDefault(NodeShape.RoundedRectangle);

    /// <summary>
    /// Gets or sets the optional SVG icon used to visually identify the node.
    /// </summary>
    public string? IconSvg { get; set; }

    /// <summary>
    /// Gets or sets the optional header text override. When <see langword="null"/> or blank, the node <see cref="DisplayName"/> is used as the header text.
    /// </summary>
    public string? HeaderText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the node definition supports a simplified overview projection.
    /// </summary>
    public bool HasOverview { get; set; }

    /// <summary>
    /// Gets or sets the number of layout columns available to the node definition.
    /// </summary>
    public int ColumnCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the optional style override for the node definition. When <see langword="null"/>, the definition inherits the document style.
    /// </summary>
    public NodeStyle? Style { get; set; }

    /// <summary>
    /// Gets the persisted detailed-node components associated with the node definition.
    /// </summary>
    public List<NodeComponent> Components { get; set; } = [];

    #endregion Properties
}
