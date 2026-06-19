namespace Arctos.Nodes.Models;

/// <summary>
/// Defines the color source used when rendering a separator component.
/// </summary>
public enum NodeSeparatorColorMode
{
    /// <summary>
    /// Uses the node shape border color.
    /// </summary>
    Border = 0,

    /// <summary>
    /// Uses the effective document text color.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Uses a neutral contrast color derived from the node background.
    /// </summary>
    Neutral = 2,
}
