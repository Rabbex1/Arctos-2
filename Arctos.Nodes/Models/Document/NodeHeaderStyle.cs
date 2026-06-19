namespace Arctos.Nodes.Models;

/// <summary>
/// Describes the visual styling used to paint a node header and its text.
/// </summary>
public sealed class NodeHeaderStyle
{
    #region Properties

    /// <summary>
    /// Gets or sets the default header background color.
    /// </summary>
    public NodeColor BackgroundColor { get; set; } = NodeColor.Transparent;

    /// <summary>
    /// Gets or sets the default header font color.
    /// </summary>
    public NodeColor FontColor { get; set; } = NodeColor.Black;

    /// <summary>
    /// Gets or sets the preferred header font weight.
    /// </summary>
    public NodeFontWeight FontWeight { get; set; } = NodeFontWeight.Normal;

    /// <summary>
    /// Gets or sets the horizontal alignment used for the header text.
    /// </summary>
    public NodeTextAlignment Alignment { get; set; } = NodeTextAlignment.Center;

    #endregion Properties
}
