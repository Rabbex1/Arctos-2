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
            ApplyComponentChange(
                "Change icon button svg",
                IconButtonModel.Svg,
                svg,
                resolvedSvg =>
                {
                    IconButtonModel.Svg = resolvedSvg;
                    OnPropertyChanged();
                });
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
            ApplyComponentChange(
                "Change icon button color mode",
                IconButtonModel.ColorMode,
                value,
                colorMode =>
                {
                    IconButtonModel.ColorMode = colorMode;
                    OnPropertyChanged();
                });
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
            new InputStringVM("Name:", Label, (wrapper, label) =>
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

            new InputEnumVM("Color:", ColorMode, (wrapper, colorMode) =>
            {
                if (colorMode is not NodeIconButtonColorMode iconColorMode)
                    return;

                ApplyComponentChange(
                    "Change icon button color mode",
                    IconButtonModel.ColorMode,
                    iconColorMode,
                    resolvedColorMode =>
                    {
                        IconButtonModel.ColorMode = resolvedColorMode;
                        wrapper.UpdateValue(resolvedColorMode);
                        OnPropertyChanged(nameof(ColorMode));
                    });
            }),

            new InputTextAreaVM("SVG:", Svg, (wrapper, svg) =>
            {
                var value = string.IsNullOrWhiteSpace(svg) ? null : svg.Trim();
                ApplyComponentChange(
                    "Change icon button svg",
                    IconButtonModel.Svg,
                    value,
                    resolvedSvg =>
                    {
                        IconButtonModel.Svg = resolvedSvg;
                        wrapper.UpdateValue(resolvedSvg ?? string.Empty);
                        OnPropertyChanged(nameof(Svg));
                    });
            }),
        ];
    }

    private NodeIconButton IconButtonModel => (NodeIconButton)Model;
}
