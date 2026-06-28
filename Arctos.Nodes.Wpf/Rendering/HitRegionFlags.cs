namespace Arctos.Nodes.Wpf.Rendering;

/// <summary>
/// Describes the semantic hit region that was resolved from the hidden hit-test layer.
/// </summary>
[Flags]
public enum HitRegionFlags
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Top = 1 << 2,
    Bottom = 1 << 3,
    Middle = 1 << 4,
    Body = 1 << 5,
    Border = 1 << 6,
    Header = 1 << 7,
    Glyph = 1 << 8,
}
