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
            ApplyComponentChange(
                "Change indicator color",
                IndicatorModel.Color,
                value,
                color =>
                {
                    IndicatorModel.Color = color;
                    OnPropertyChanged();
                });
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
            ApplyComponentChange(
                "Change indicator shape",
                IndicatorModel.Shape,
                value,
                shape =>
                {
                    IndicatorModel.Shape = shape;
                    OnPropertyChanged();
                });
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

            new InputEnumVM("Shape:", Shape, (wrapper, shape) =>
            {
                if (shape == null)
                    return;

                var value = (NodeIndicatorShape)shape;
                ApplyComponentChange(
                    "Change indicator shape",
                    IndicatorModel.Shape,
                    value,
                    resolvedShape =>
                    {
                        IndicatorModel.Shape = resolvedShape;
                        wrapper.UpdateValue(resolvedShape);
                        OnPropertyChanged(nameof(Shape));
                    });
            }),

            new InputColorVM("Default Color:", Color.ToMediaColor(), (wrapper, color) =>
            {
                var value = color.ToNodeColor();
                ApplyComponentChange(
                    "Change indicator color",
                    IndicatorModel.Color,
                    value,
                    resolvedColor =>
                    {
                        IndicatorModel.Color = resolvedColor;
                        if (wrapper is InputColorVM colorWrapper)
                            colorWrapper.UpdateColor(resolvedColor.ToMediaColor());
                        else
                            wrapper.UpdateValue(resolvedColor.ToMediaColor());
                        OnPropertyChanged(nameof(Color));
                    });
            }),
        ];
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Properties

    private NodeIndicator IndicatorModel => (NodeIndicator)Model;

    #endregion Private Properties
}
