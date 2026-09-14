namespace Arctos.Forms;

/// <summary>
/// Represents a portable RGBA color value used by form inputs.
/// </summary>
public readonly record struct InputColor(byte Red, byte Green, byte Blue, byte Alpha = 255);
