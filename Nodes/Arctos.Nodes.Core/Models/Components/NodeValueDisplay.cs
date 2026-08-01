namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a passive labeled value that can be updated by the host application but not edited by the user.
/// </summary>
public sealed class NodeValueDisplay : NodeComponent
{
    /// <summary>
    /// Gets or sets the default display value shown by the component.
    /// </summary>
    public string DefaultValue { get; set; } = "Value";

    /// <summary>
    /// Gets or sets how the display value chooses its text color.
    /// </summary>
    public NodeValueDisplayColorMode ValueColorMode { get; set; } = NodeValueDisplayColorMode.Default;

    /// <summary>
    /// Gets or sets the custom value color used when <see cref="ValueColorMode"/> is <see cref="NodeValueDisplayColorMode.Custom"/>.
    /// </summary>
    public NodeColor ValueColor { get; set; } = NodeColor.White;
}

