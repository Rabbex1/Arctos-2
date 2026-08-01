using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a separator component inside a node.
/// </summary>
public sealed class NodeSeparatorComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the color source used to render the separator line.
    /// </summary>
    public NodeSeparatorColorMode ColorMode
    {
        get => SeparatorModel.ColorMode;
        set
        {
            ApplyComponentChange(
                "Change separator color mode",
                SeparatorModel.ColorMode,
                value,
                colorMode =>
                {
                    SeparatorModel.ColorMode = colorMode;
                    OnPropertyChanged();
                });
        }
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node separator component view model.
    /// </summary>
    /// <param name="model">The persisted separator model to wrap.</param>
    public NodeSeparatorComponentVM(NodeSeparator? model = null)
        : base(model ?? new NodeSeparator { Label = "Separator" }, PaletteItemKind.Separator, "Separator")
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
            new InputEnumVM("Color:", ColorMode, (wrapper, colorMode) =>
            {
                if (colorMode is not NodeSeparatorColorMode separatorColorMode)
                    return;

                ApplyComponentChange(
                    "Change separator color mode",
                    SeparatorModel.ColorMode,
                    separatorColorMode,
                    resolvedColorMode =>
                    {
                        SeparatorModel.ColorMode = resolvedColorMode;
                        wrapper.UpdateValue(resolvedColorMode);
                        OnPropertyChanged(nameof(ColorMode));
                    });
            }),
        ];
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Properties

    private NodeSeparator SeparatorModel => (NodeSeparator)Model;

    #endregion Private Properties
}
