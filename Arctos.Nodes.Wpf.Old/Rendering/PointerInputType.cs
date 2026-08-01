namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Defines the supported pointer input event kinds for the Skia host.
/// </summary>
public enum PointerInputType
{
    Move = 0,
    Leave = 1,
    LeftButtonDown = 2,
    LeftButtonUp = 3,
    RightButtonDown = 4,
    RightButtonUp = 5,
    MiddleButtonDown = 6,
    MiddleButtonUp = 7,
    Wheel = 8,
}
