namespace Arctos.Wpf.DragDrop
{
  /// <summary>
  /// Platform-neutral drag image payload that can be rendered by a UI-specific adapter.
  /// </summary>
  public interface IDragImageSource
  {
    byte[] PngData { get; }
    int WidthPx { get; }
    int HeightPx { get; }
    int HotspotXPx { get; }
    int HotspotYPx { get; }
  }
}
