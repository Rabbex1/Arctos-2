using Arctos.Nodes.Models;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Carries the effective document typography and layout settings used by node rendering and layout.
/// </summary>
public sealed record NodeCanvasStyleContext(
    string? FontFamily,
    float FontSize,
    float HeaderFontSize,
    NodeColor FontColor,
    float Padding,
    float RowSpacing,
    float ColumnSpacing,
    float RoundedCornerRadius,
    float MinimumColumnWidth,
    float PortSize,
    NodePortPlacement PortPlacement);
