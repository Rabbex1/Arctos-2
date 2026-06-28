using System.ComponentModel;
using Arctos.Nodes.Models;
using Arctos.Wpf.Inputs;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Minimal working view model over <see cref="NodeMapSettings"/>.
/// </summary>
public sealed class NodeMapSettingsVM : ObservableObject
{
    #region Fields

    private readonly NodeMapSettings _settings;
    private readonly MapStyle _style;

    #endregion Fields

    #region Properties

    public NodeMapSettings Settings => _settings;

    public MapStyle Style => _style;

    public InputFormVM PropertiesForm { get; } = new();

    public Action? OnDocumentChanged { get; set; }

    #endregion Properties

    #region Constructors

    public NodeMapSettingsVM(NodeMapSettings settings, MapStyle style)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _style = style ?? throw new ArgumentNullException(nameof(style));
        PropertiesForm.OnInputChanged = NotifyDocumentChanged;
        MakePropertiesForm();
    }

    #endregion Constructors

    private void MakePropertiesForm()
    {
        PropertiesForm.Inputs.Add(new InputStringVM("Name:", _settings.Name, (name) =>
        {
            _settings.Name = name ?? string.Empty;
        }));

        PropertiesForm.Inputs.Add(new InputStringVM("Document Type:", _settings.DocumentType, (docType) =>
        {
            _settings.DocumentType = docType ?? string.Empty;
        }));

        PropertiesForm.Inputs.Add(new InputEnumVM("Startup Mode:", _settings.StartupMode, (mode) =>
        {
            if (mode == null)
                return;
            _settings.StartupMode = (OperationMode) mode;
        }));

        PropertiesForm.Inputs.Add(new InputColorVM("Canvas Color:", _settings.CanvasColor.ToMediaColor(), (color) =>
        {
            _settings.CanvasColor = color.ToNodeColor();
        }));

        PropertiesForm.Inputs.Add(new InputBoolVM("Runtime Library Edit:", _settings.RuntimeLibraryEdit, (rle) =>
        {
            _settings.RuntimeLibraryEdit = rle;
        }));

        PropertiesForm.Inputs.Add(new InputBoolVM("Runtime Graph Edit:", _settings.RuntimeGraphEdit, (rge) =>
        {
            _settings.RuntimeGraphEdit = rge;
        }));

        PropertiesForm.Inputs.Add(new InputGroupVM(
            string.Empty,
            ("Typography", CreateTypographyInputs()),
            collapsable: true));

        PropertiesForm.Inputs.Add(new InputGroupVM(
            string.Empty,
            ("Layout", CreateLayoutInputs()),
            collapsable: true));
    }

    private List<DataInputWrapperVM> CreateTypographyInputs()
    {
        return
        [
            new InputStringVM("Font Family:", _style.FontFamily, fontFamily =>
            {
                _style.FontFamily = string.IsNullOrWhiteSpace(fontFamily) ? null : fontFamily;
            }),

            new InputFloatVM("Font Size:", _style.FontSize, fontSize =>
            {
                _style.FontSize = fontSize;
            }),

            new InputFloatVM("Header Font Size:", _style.HeaderFontSize, headerFontSize =>
            {
                _style.HeaderFontSize = headerFontSize;
            }),

            new InputColorVM("Font Color:", _style.FontColor.ToMediaColor(), color =>
            {
                _style.FontColor = color.ToNodeColor();
            }),
        ];
    }

    private List<DataInputWrapperVM> CreateLayoutInputs()
    {
        return
        [
            new InputFloatVM("Padding:", _style.Padding, padding =>
            {
                _style.Padding = padding;
            }),

            new InputFloatVM("Row Spacing:", _style.RowSpacing, rowSpacing =>
            {
                _style.RowSpacing = rowSpacing;
            }),

            new InputFloatVM("Column Spacing:", _style.ColumnSpacing, columnSpacing =>
            {
                _style.ColumnSpacing = columnSpacing;
            }),

            new InputFloatVM("Rounded Corner Radius:", _style.RoundedCornerRadius, radius =>
            {
                _style.RoundedCornerRadius = radius;
            }),

            new InputFloatVM("Control Corner Radius:", _style.ControlCornerRadius, radius =>
            {
                _style.ControlCornerRadius = radius;
            }),

            new InputFloatVM("Minimum Column Width:", _style.MinimumColumnWidth, width =>
            {
                _style.MinimumColumnWidth = width;
            }),

            new InputFloatVM("Port Size:", _style.PortSize, size =>
            {
                _style.PortSize = size;
            }),

            new InputEnumVM("Port Placement:", _style.PortPlacement, placement =>
            {
                if (placement == null)
                    return;

                _style.PortPlacement = (NodePortPlacement)placement;
            }),
        ];

    }

    private void NotifyDocumentChanged()
    {
        OnDocumentChanged?.Invoke();
    }

}
