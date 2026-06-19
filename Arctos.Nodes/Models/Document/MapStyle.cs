namespace Arctos.Nodes.Models;

/// <summary>
/// Describes the document-level visual language and layout rules used by a node map.
/// </summary>
public sealed class MapStyle
{
    #region Properties

    /// <summary>
    /// Gets or sets the shared font family used for layout and text rendering. When <see langword="null"/>, the host default font is used.
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Gets or sets the shared node font size used for layout and text rendering.
    /// </summary>
    public float FontSize { get; set; } = 12.0f;

    /// <summary>
    /// Gets or sets the shared node header font size used for header layout and text rendering.
    /// </summary>
    public float HeaderFontSize { get; set; } = 14.0f;

    /// <summary>
    /// Gets or sets the shared text color used for node headers, text components, and component labels.
    /// </summary>
    public NodeColor FontColor { get; set; } = NodeColor.Black;

    /// <summary>
    /// Gets or sets the shared inner padding used by detailed node layout, expressed in minor node units.
    /// </summary>
    public float Padding { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the shared extra spacing inserted between component rows, expressed in minor node units.
    /// </summary>
    public float RowSpacing { get; set; }

    /// <summary>
    /// Gets or sets the shared spacing inserted between component columns, expressed in minor node units.
    /// </summary>
    public float ColumnSpacing { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the corner radius used by rounded rectangle node rendering, expressed in minor node units.
    /// </summary>
    public float RoundedCornerRadius { get; set; } = 2.0f;

    /// <summary>
    /// Gets or sets the minimum width used by a single detailed-layout content column, expressed in minor node units.
    /// </summary>
    public float MinimumColumnWidth { get; set; } = 24.0f;

    /// <summary>
    /// Gets or sets the shared port glyph size, expressed in minor node units.
    /// </summary>
    public float PortSize { get; set; } = 1.5f;

    /// <summary>
    /// Gets or sets the shared port placement relative to the node border.
    /// </summary>
    public NodePortPlacement PortPlacement { get; set; } = NodePortPlacement.Over;

    #endregion Properties
}
