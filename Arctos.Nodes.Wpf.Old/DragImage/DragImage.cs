using Arctos.Wpf.DragDrop;

namespace Arctos.Nodes.Wpf.DragImage;

/// <summary>
/// Represents a platform-neutral drag image artifact rendered off-screen with Skia.
/// </summary>
public sealed record DragImage(
    byte[] PngData,
    int WidthPx,
    int HeightPx,
    int HotspotXPx,
    int HotspotYPx) : IDragImageSource;
