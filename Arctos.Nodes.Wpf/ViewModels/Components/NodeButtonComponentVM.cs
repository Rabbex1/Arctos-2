using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a button component inside a node.
/// </summary>
public sealed class NodeButtonComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node button component view model.
    /// </summary>
    /// <param name="model">The persisted button model to wrap.</param>
    public NodeButtonComponentVM(NodeButton? model = null)
        : base(model ?? new NodeButton { Label = "Button" }, PaletteItemKind.Button, "Button", supportsId: true)
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
            new InputStringVM("Label:", Label, (wrapper, label) =>
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
