using Arctos;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Wpf.DragDrop;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Represents a single system-defined palette item that may be dragged into a node.
/// </summary>
public sealed class PaletteItemVM : BaseVM, IDragable
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the core palette item kind represented by this entry.
    /// </summary>
    public PaletteItemKind Kind { get; }

    /// <summary>
    /// Gets the user-facing label shown for this palette item.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the factory used to create a component instance from this palette item.
    /// </summary>
    public Func<NodeComponentVM> CreateComponent { get; }

    /// <summary>
    /// Gets the drag/drop payload type exposed by this palette item.
    /// </summary>
    public Type DataType => typeof(PaletteItemVM);

    /// <summary>
    /// Gets the drag/drop payload object exposed by this palette item.
    /// </summary>
    public object DataObject => this;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new palette item view model.
    /// </summary>
    /// <param name="kind">The represented palette item kind.</param>
    /// <param name="label">The label shown for the palette item.</param>
    /// <param name="createComponent">The callback used to create a component from this palette item.</param>
    public PaletteItemVM(PaletteItemKind kind, string label, Func<NodeComponentVM> createComponent)
    {
        Kind = kind;
        Label = label ?? throw new ArgumentNullException(nameof(label));
        CreateComponent = createComponent ?? throw new ArgumentNullException(nameof(createComponent));
        DisplayName = label;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Removes the supplied dragged object from its owner. Palette items are immutable drag sources, so this is a no-op.
    /// </summary>
    /// <param name="i">The dragged object.</param>
    public void Remove(object i)
    {
        _ = i;
    }

    #endregion Public Methods
}
