using System;

namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Describes a component hit region on the hidden hit-test layer.
/// </summary>
[Flags]
internal enum ComponentHitFlags
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Top = 1 << 2,
    Bottom = 1 << 3,
    Body = 1 << 4,
    Border = 1 << 5,
    Header = 1 << 6,
    Glyph = 1 << 7,
}
