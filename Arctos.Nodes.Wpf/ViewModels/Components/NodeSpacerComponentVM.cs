using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents an empty spacing component inside a node.
/// </summary>
public sealed class NodeSpacerComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the spacer height in minor node units.
    /// </summary>
    public int Height
    {
        get => SpacerModel.Height;
        set
        {
            var height = NodeSpacer.SizeRange.Clamp(value);
            ApplyComponentChange(
                "Change spacer height",
                SpacerModel.Height,
                height,
                resolvedHeight =>
                {
                    SpacerModel.Height = resolvedHeight;
                    DisplayName = GetDisplayName();
                    OnPropertyChanged();
                });
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the spacer expands to consume leftover column height.
    /// </summary>
    public bool Expand
    {
        get => SpacerModel.Expand;
        set
        {
            ApplyComponentChange(
                "Change spacer expand mode",
                SpacerModel.Expand,
                value,
                expand =>
                {
                    SpacerModel.Expand = expand;
                    DisplayName = GetDisplayName();
                    OnPropertyChanged();
                });
        }
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node spacer component view model.
    /// </summary>
    /// <param name="model">The persisted spacer model to wrap.</param>
    public NodeSpacerComponentVM(NodeSpacer? model = null)
        : base(model ?? new NodeSpacer { Label = "Spacer", Height = 4 }, PaletteItemKind.Spacer, "Spacer")
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
            new InputIntRangeExtraVM("Size:", Expand ? "e" : Height.ToString(), NodeSpacer.SizeRange.Minimum, NodeSpacer.SizeRange.Maximum, value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                if (string.Equals(value, "e", StringComparison.OrdinalIgnoreCase))
                {
                    Expand = true;
                    return;
                }

                if (!int.TryParse(value, out var parsedHeight))
                    return;

                Expand = false;
                Height = parsedHeight;
            }, ('e', "Expand")),
        ];
    }

    protected override string GetDisplayName()
    {
        return Expand
            ? "Spacer (Expand)"
            : $"Spacer ({Height:0.##} nu)";
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Properties

    private NodeSpacer SpacerModel => (NodeSpacer)Model;

    #endregion Private Properties
}
