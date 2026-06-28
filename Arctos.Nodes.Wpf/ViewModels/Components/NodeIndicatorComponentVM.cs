using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a passive indicator component inside a node.
/// </summary>
public sealed class NodeIndicatorComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the default indicator color.
    /// </summary>
    public NodeColor Color
    {
        get => IndicatorModel.Color;
        set
        {
            if (IndicatorModel.Color == value)
                return;

            IndicatorModel.Color = value;
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    /// <summary>
    /// Gets or sets the indicator shape.
    /// </summary>
    public NodeIndicatorShape Shape
    {
        get => IndicatorModel.Shape;
        set
        {
            if (IndicatorModel.Shape == value)
                return;

            IndicatorModel.Shape = value;
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new indicator component view model.
    /// </summary>
    public NodeIndicatorComponentVM(NodeIndicator? model = null)
        : base(model ?? new NodeIndicator
        {
            Label = "Indicator",
            Color = NodeColor.White,
            Shape = NodeIndicatorShape.Round,
        }, PaletteItemKind.Indicator, "Indicator", supportsId: true)
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
            new InputStringVM("Label:", Label, label =>
            {
                Label = label ?? string.Empty;
            }),

            new InputEnumVM("Shape:", Shape, shape =>
            {
                if (shape == null)
                    return;

                Shape = (NodeIndicatorShape)shape;
            }),

            new InputColorVM("Default Color:", Color.ToMediaColor(), color =>
            {
                Color = color.ToNodeColor();
            }),
        ];
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Properties

    private NodeIndicator IndicatorModel => (NodeIndicator)Model;

    #endregion Private Properties
}
