using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;
using System.Windows;
using System.Windows.Threading;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a passive labeled value component inside a node.
/// </summary>
public sealed class NodeValueDisplayComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the default display value shown by the component.
    /// </summary>
    public string DefaultValue
    {
        get => ValueDisplayModel.DefaultValue;
        set
        {
            var resolvedValue = value ?? string.Empty;
            ApplyComponentChange(
                "Change display value",
                ValueDisplayModel.DefaultValue,
                resolvedValue,
                defaultValue =>
                {
                    ValueDisplayModel.DefaultValue = defaultValue;
                    OnPropertyChanged();
                });
        }
    }

    /// <summary>
    /// Gets or sets how the display value chooses its text color.
    /// </summary>
    public NodeValueDisplayColorMode ValueColorMode
    {
        get => ValueDisplayModel.ValueColorMode;
        set
        {
            ApplyComponentChange(
                "Change display value color mode",
                ValueDisplayModel.ValueColorMode,
                value,
                valueColorMode =>
                {
                    ValueDisplayModel.ValueColorMode = valueColorMode;
                    OnPropertyChanged();
                    Application.Current?.Dispatcher.BeginInvoke(new Action(RebuildEditTabs), DispatcherPriority.Background);
                });
        }
    }

    /// <summary>
    /// Gets or sets the custom value color.
    /// </summary>
    public NodeColor ValueColor
    {
        get => ValueDisplayModel.ValueColor;
        set
        {
            ApplyComponentChange(
                "Change display value color",
                ValueDisplayModel.ValueColor,
                value,
                valueColor =>
                {
                    ValueDisplayModel.ValueColor = valueColor;
                    OnPropertyChanged();
                });
        }
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new value display component view model.
    /// </summary>
    public NodeValueDisplayComponentVM(NodeValueDisplay? model = null)
        : base(model ?? new NodeValueDisplay
        {
            Label = "Value Display",
            DefaultValue = "Value",
            ValueColorMode = NodeValueDisplayColorMode.Default,
            ValueColor = NodeColor.White,
        }, PaletteItemKind.ValueDisplay, "Value Display", supportsId: true)
    {
        RebuildEditTabs();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    protected override List<DataInputWrapperVM> CreateGeneralInputs()
    {
        var inputs = new List<DataInputWrapperVM>
        {
            new InputStringVM("Label:", Label, label =>
            {
                Label = label ?? string.Empty;
            }),

            new InputStringVM("Default Value:", DefaultValue, value =>
            {
                DefaultValue = value ?? string.Empty;
            }),

            new InputEnumVM("Value Color Mode:", ValueColorMode, mode =>
            {
                if (mode is NodeValueDisplayColorMode valueColorMode)
                    ValueColorMode = valueColorMode;
            }),
        };

        if (ValueColorMode == NodeValueDisplayColorMode.Custom)
        {
            inputs.Add(new InputColorVM("Custom Color:", ValueColor.ToMediaColor(), color =>
            {
                ValueColor = color.ToNodeColor();
            }));
        }

        return inputs;
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Properties

    private NodeValueDisplay ValueDisplayModel => (NodeValueDisplay)Model;

    #endregion Private Properties
}
