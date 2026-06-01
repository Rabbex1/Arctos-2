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
    /// Gets or sets the default header font size.
    /// </summary>
    public float FontSize { get; set; } = 12.0f;

    /// <summary>
    /// Gets or sets the preferred header font family. When <see langword="null"/>, the host default font is used.
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Gets or sets the preferred header font weight.
    /// </summary>
    public NodeFontWeight FontWeight { get; set; } = NodeFontWeight.Normal;

    #endregion Properties
}
