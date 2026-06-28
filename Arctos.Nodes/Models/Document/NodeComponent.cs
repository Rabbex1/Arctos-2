using System.Text.Json.Serialization;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a persisted detailed-node component.
/// </summary>
[JsonDerivedType(typeof(NodeText), typeDiscriminator: "Text")]
[JsonDerivedType(typeof(NodeInput), typeDiscriminator: "Input")]
[JsonDerivedType(typeof(NodeValueDisplay), typeDiscriminator: "ValueDisplay")]
[JsonDerivedType(typeof(NodeButton), typeDiscriminator: "Button")]
[JsonDerivedType(typeof(NodeSeparator), typeDiscriminator: "Separator")]
[JsonDerivedType(typeof(NodeSpacer), typeDiscriminator: "Spacer")]
[JsonDerivedType(typeof(NodePort), typeDiscriminator: "Port")]
[JsonDerivedType(typeof(NodeGridInput), typeDiscriminator: "GridInput")]
[JsonDerivedType(typeof(NodeIndicator), typeDiscriminator: "Indicator")]
[JsonDerivedType(typeof(NodeIconButton), typeDiscriminator: "IconButton")]
public abstract class NodeComponent
{
    #region Properties

    /// <summary>
    /// Gets or sets the optional stable component identifier used by host applications and runtime updates.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the user-facing component label or text.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired component position in minor node-space units.
    /// </summary>
    public NodePoint Position { get; set; }

    /// <summary>
    /// Gets or sets the zero-based layout column used by the detailed node editor.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the optional parent container identifier used by hierarchical layouts.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the persisted vertical ordering slot used within the current parent container.
    /// </summary>
    public int YOrder { get; set; }

    /// <summary>
    /// Gets or sets the persisted horizontal ordering slot used within a shared row.
    /// </summary>
    public int XOrder { get; set; }

    /// <summary>
    /// Gets or sets the fixed width of the component in node units. A value of <c>0</c> means the component can expand.
    /// </summary>
    public virtual float FixedWidth { get; set; }

    #endregion Properties
}
