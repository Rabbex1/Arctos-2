using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a simple text label component inside a node.
/// </summary>
public sealed class NodeLabelComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node label component view model.
    /// </summary>
    /// <param name="model">The persisted text model to wrap.</param>
    public NodeLabelComponentVM(NodeText? model = null)
        : base(model ?? new NodeText { Label = "Text" }, PaletteItemKind.Text, "Text")
    {
        RebuildEditTabs();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    protected override List<DataInputWrapperVM> CreateGeneralInputs()
    {
        return
        [
            new InputStringVM("Text:", Label, (wrapper, label) =>
            {
                var value = label ?? string.Empty;
                ApplyComponentChange(
                    "Change component label",
                    Model.Label,
                    value,
                    resolvedLabel =>
                    {
                        Model.Label = resolvedLabel;
                        wrapper.UpdateValue(resolvedLabel);
                        DisplayName = GetDisplayName();
                        OnPropertyChanged(nameof(Label));
                    });
            }),
        ];
    }

    #endregion Protected Methods
}
