using Arctos.Nodes.Wpf.ViewModels;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Exposes edit tabs for the shared edit toolbox.
/// </summary>
public interface IEditTabs
{
    /// <summary>
    /// Gets the edit tabs shown for the current editable host.
    /// </summary>
    IReadOnlyList<EditTabVM> EditTabs { get; }
}
