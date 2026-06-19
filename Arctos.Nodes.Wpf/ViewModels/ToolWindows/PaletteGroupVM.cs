using Arctos;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Represents a named group of system-defined palette items.
/// </summary>
public sealed class PaletteGroupVM : BaseVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the group header shown above the palette items.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets the palette items belonging to this group.
    /// </summary>
    public IReadOnlyList<PaletteItemVM> Items { get; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new palette group view model.
    /// </summary>
    /// <param name="header">The displayed group header.</param>
    /// <param name="items">The palette items belonging to the group.</param>
    public PaletteGroupVM(string header, IReadOnlyList<PaletteItemVM> items)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Items = items ?? throw new ArgumentNullException(nameof(items));
        DisplayName = header;
    }

    #endregion Constructors
}
