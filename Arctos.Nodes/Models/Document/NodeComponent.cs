using System.Text.Json.Serialization;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a persisted detailed-node component.
/// </summary>
[JsonDerivedType(typeof(NodeText), typeDiscriminator: "Text")]
[JsonDerivedType(typeof(NodeInput), typeDiscriminator: "Input")]
[JsonDerivedType(typeof(NodeButton), typeDiscriminator: "Button")]
[JsonDerivedType(typeof(NodeSeparator), typeDiscriminator: "Separator")]
[JsonDerivedType(typeof(NodeSpacer), typeDiscriminator: "Spacer")]
[JsonDerivedType(typeof(NodePort), typeDiscriminator: "Port")]
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

    #endregion Properties
}
