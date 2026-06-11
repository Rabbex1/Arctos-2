using SkiaSharp;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Represents a pointer input event in centered node-space coordinates.
/// </summary>
public readonly record struct PointerInput(
    PointerInputType InputType,
    SKPoint Position,
    SKPoint ScreenPosition,
    int ClickCount = 0,
    int WheelDelta = 0);
