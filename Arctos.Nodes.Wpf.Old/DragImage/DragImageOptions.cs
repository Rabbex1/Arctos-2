namespace Arctos.Nodes.Wpf.DragImage;

/// <summary>
/// Controls drag image rendering size, normalization, and fade behavior.
/// </summary>
public sealed class DragImageOptions
{
    /// <summary>
    /// Gets or sets the maximum output width in pixels.
    /// </summary>
    public int MaxWidthPx { get; set; } = 128;

    /// <summary>
    /// Gets or sets the maximum output height in pixels.
    /// </summary>
    public int MaxHeightPx { get; set; } = 128;

    /// <summary>
    /// Gets or sets the target text height in pixels used to normalize font-family differences.
    /// </summary>
    public float TargetTextHeightPx { get; set; } = 22.0f;

    /// <summary>
    /// Gets or sets the fade radius in pixels measured from the hotspot corner.
    /// </summary>
    public float FadeRadiusPx { get; set; } = 96.0f;
}
