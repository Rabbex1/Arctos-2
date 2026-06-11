namespace Arctos.Nodes.Models;

/// <summary>
/// Describes the visual styling used to paint a node body and its main text.
/// </summary>
public sealed class NodeShapeStyle
{
    #region Properties

    /// <summary>
    /// Gets or sets the node background color.
    /// </summary>
    public NodeColor BackgroundColor { get; set; } = NodeColor.Blues.Dark;

    /// <summary>
    /// Gets or sets the node border color.
    /// </summary>
    public NodeColor BorderColor { get; set; } = NodeColor.Blues.Base;

    /// <summary>
    /// Gets or sets the node border thickness.
    /// </summary>
    public float BorderThickness { get; set; } = 2.0f;

    /// <summary>
    /// Gets or sets the main node font size.
    /// </summary>
    public float FontSize { get; set; } = 12.0f;

    /// <summary>
    /// Gets or sets the main node font color.
    /// </summary>
    public NodeColor FontColor { get; set; } = NodeColor.Black;

    /// <summary>
    /// Gets or sets the preferred font family. When <see langword="null"/>, the host default font is used.
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Gets or sets the preferred font weight.
    /// </summary>
    public NodeFontWeight FontWeight { get; set; } = NodeFontWeight.Normal;

    #endregion Properties
}
