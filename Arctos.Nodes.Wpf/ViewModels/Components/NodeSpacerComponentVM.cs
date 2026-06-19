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
            if (SpacerModel.Height == height)
                return;

            SpacerModel.Height = height;
            OnPropertyChanged();
            NotifyComponentChanged();
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
        var sizeInput = new IntRangeInput
        {
            Min = NodeSpacer.SizeRange.Minimum,
            Max = NodeSpacer.SizeRange.Maximum,
            Value = Height,
        };

        return
        [
            new InputIntRangeVM("Size:", sizeInput, input =>
            {
                if (input != null)
                    Height = input.Value;
            }),
        ];
    }

    protected override string GetDisplayName()
    {
        return $"Spacer ({Height:0.##} nu)";
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Properties

    private NodeSpacer SpacerModel => (NodeSpacer)Model;

    #endregion Private Properties
}
