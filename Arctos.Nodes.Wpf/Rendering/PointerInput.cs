using SkiaSharp;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Represents a pointer input event in node-space, canvas-local pixel, and desktop screen coordinates.
/// </summary>
public readonly record struct PointerInput(
    PointerInputType InputType,
    SKPoint Position,
    SKPoint LocalPosition,
    SKPoint ScreenPosition,
    int ClickCount = 0,
    int WheelDelta = 0);
