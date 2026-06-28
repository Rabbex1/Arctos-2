using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a fixed-width icon button component inside a node.
/// </summary>
public sealed class NodeIconButtonComponentVM : NodeComponentVM
{
    /// <summary>
    /// Gets or sets the inline SVG payload rendered inside the icon button.
    /// </summary>
    public string Svg
    {
        get => IconButtonModel.Svg ?? string.Empty;
        set
        {
            var svg = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (string.Equals(IconButtonModel.Svg, svg, StringComparison.Ordinal))
                return;

            IconButtonModel.Svg = svg;
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    /// <summary>
    /// Gets or sets how the icon SVG should be colored.
    /// </summary>
    public NodeIconButtonColorMode ColorMode
    {
        get => IconButtonModel.ColorMode;
        set
        {
            if (IconButtonModel.ColorMode == value)
                return;

            IconButtonModel.ColorMode = value;
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    /// <summary>
    /// Creates a new icon button component view model.
    /// </summary>
    public NodeIconButtonComponentVM(NodeIconButton? model = null)
        : base(model ?? new NodeIconButton { Label = "Icon Button" }, PaletteItemKind.IconButton, "Icon Button", supportsId: true)
    {
        RebuildEditTabs();
    }

    protected override List<DataInputWrapperVM> CreateGeneralInputs()
    {
        return
        [
            new InputStringVM("Name:", Label, label =>
            {
                Label = label ?? string.Empty;
            }),

            new InputEnumVM("Color:", ColorMode, colorMode =>
            {
                if (colorMode is NodeIconButtonColorMode iconColorMode)
                    ColorMode = iconColorMode;
            }),

            new InputTextAreaVM("SVG:", Svg, svg =>
            {
                Svg = svg ?? string.Empty;
            }),
        ];
    }

    private NodeIconButton IconButtonModel => (NodeIconButton)Model;
}
