using System.Windows.Media;

namespace Arctos.Wpf.Inputs;

/// <summary>
/// Represents a named color entry for list-style inputs.
/// </summary>
public sealed class NamedColorValue
{
    /// <summary>
    /// Gets or sets the user-facing name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the associated color.
    /// </summary>
    public Color Color { get; set; } = Colors.White;
}
