namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Represents a single addable entry in the node palette.
/// </summary>
public sealed class NodePaletteItemVM
{
    #region Properties

    /// <summary>
    /// Gets the palette item kind.
    /// </summary>
    public required NodePaletteItemKind Kind { get; init; }

    /// <summary>
    /// Gets the palette item display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the palette item type definition when the item represents a component type.
    /// </summary>
    public InputTypeDefinition? TypeDefinition { get; init; }

    /// <summary>
    /// Gets the node definition when the item represents a graph node definition.
    /// </summary>
    public NodeDefinitionVM? NodeDefinition { get; init; }

    #endregion Properties
}
