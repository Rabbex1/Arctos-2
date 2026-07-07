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

            new InputStringVM("Default Value:", DefaultValue, (wrapper, value) =>
            {
                var resolvedValue = value ?? string.Empty;
                ApplyComponentChange(
                    "Change display value",
                    ValueDisplayModel.DefaultValue,
                    resolvedValue,
                    defaultValue =>
                    {
                        ValueDisplayModel.DefaultValue = defaultValue;
                        wrapper.UpdateValue(defaultValue);
                        OnPropertyChanged(nameof(DefaultValue));
                    });
            }),

            new InputEnumVM("Value Color Mode:", ValueColorMode, (wrapper, mode) =>
            {
                if (mode is not NodeValueDisplayColorMode valueColorMode)
                    return;

                ApplyComponentChange(
                    "Change display value color mode",
                    ValueDisplayModel.ValueColorMode,
                    valueColorMode,
                    resolvedValueColorMode =>
                    {
                        ValueDisplayModel.ValueColorMode = resolvedValueColorMode;
                        wrapper.UpdateValue(resolvedValueColorMode);
                        OnPropertyChanged(nameof(ValueColorMode));
                        Application.Current?.Dispatcher.BeginInvoke(new Action(RebuildEditTabs), DispatcherPriority.Background);
                    });
            }),
        };

        if (ValueColorMode == NodeValueDisplayColorMode.Custom)
        {
            inputs.Add(new InputColorVM("Custom Color:", ValueColor.ToMediaColor(), (wrapper, color) =>
            {
                var value = color.ToNodeColor();
                ApplyComponentChange(
                    "Change display value color",
                    ValueDisplayModel.ValueColor,
                    value,
                    resolvedValueColor =>
                    {
                        ValueDisplayModel.ValueColor = resolvedValueColor;
                        if (wrapper is InputColorVM colorWrapper)
                            colorWrapper.UpdateColor(resolvedValueColor.ToMediaColor());
                        else
                            wrapper.UpdateValue(resolvedValueColor.ToMediaColor());
                        OnPropertyChanged(nameof(ValueColor));
                    });
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
