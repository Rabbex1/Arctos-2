namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents a fixed-width icon button component inside a detailed node.
/// </summary>
public sealed class NodeIconButton : NodeComponent
{
    /// <summary>
    /// Gets the default fixed width of an icon button in node units.
    /// </summary>
    public const float DefaultWidthNu = 4.0f;

    /// <summary>
    /// Gets the default SVG used when a new icon button is created.
    /// </summary>
    public const string DefaultSvg = "<svg viewBox=\"0 0 24 24\"><path d=\"M10 4H14V10H20V14H14V20H10V14H4V10H10Z\" /></svg>";

    /// <summary>
    /// Gets or sets the inline SVG payload rendered inside the button body.
    /// </summary>
    public string? Svg { get; set; } = DefaultSvg;

    /// <summary>
    /// Gets or sets how the icon SVG should be colored.
    /// </summary>
    public NodeIconButtonColorMode ColorMode { get; set; } = NodeIconButtonColorMode.Svg;

    /// <summary>
    /// Gets or sets the fixed icon-button width in node units.
    /// </summary>
    public override float FixedWidth { get; set; } = DefaultWidthNu;
}

