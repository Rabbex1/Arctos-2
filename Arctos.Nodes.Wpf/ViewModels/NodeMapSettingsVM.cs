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
    private Action<string, Action, Action>? _recordHistory;

    #endregion Fields

    #region Properties

    public NodeMapSettings Settings => _settings;

    public MapStyle Style => _style;

    public InputFormVM PropertiesForm { get; } = new();

    public Action? OnDocumentChanged { get; set; }

    public Action<string>? OnMapNameChanged { get; set; }

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

    //------------------------------------------------------------------------------------------------------------------------------------------

    #region Methods

    public void SetHistoryRecorder(Action<string, Action, Action>? recordHistory)
    {
        _recordHistory = recordHistory;
    }


    private void MakePropertiesForm()
    {
        PropertiesForm.Inputs.Add(new InputStringVM("Name:", _settings.Name, (wrapper, name) =>
        {
            var value = name ?? string.Empty;

            ApplyMapChange(
                "Change map name",
                _settings.Name,
                value,
                mapName =>
                {
                    _settings.Name = mapName;
                    wrapper.UpdateValue(mapName);
                    OnMapNameChanged?.Invoke(mapName ?? string.Empty);
                });
        }));

        PropertiesForm.Inputs.Add(new InputStringVM("Document Type:", _settings.DocumentType, (wrapper, docType) =>
        {
            var value = docType ?? string.Empty;
            ApplyMapChange(
                "Change document type",
                _settings.DocumentType,
                value,
                documentType =>
                {
                    _settings.DocumentType = documentType;
                    wrapper.UpdateValue(documentType);
                });
        }));

        PropertiesForm.Inputs.Add(new InputEnumVM("Startup Mode:", _settings.StartupMode, (wrapper, mode) =>
        {
            if (mode is not OperationMode startupMode)
                return;

            ApplyMapChange(
                "Change startup mode",
                _settings.StartupMode,
                startupMode,
                resolvedMode =>
                {
                    _settings.StartupMode = resolvedMode;
                    wrapper.UpdateValue(resolvedMode);
                });
        }));

        PropertiesForm.Inputs.Add(new InputColorVM("Canvas Color:", _settings.CanvasColor.ToMediaColor(), (wrapper, color) =>
        {
            ApplyMapChange(
                "Change canvas color",
                _settings.CanvasColor.ToMediaColor(),
                color,
                resolvedColor =>
                {
                    _settings.CanvasColor = resolvedColor.ToNodeColor();
                    wrapper.UpdateValue(resolvedColor);
                });
        }));

        PropertiesForm.Inputs.Add(new InputBoolVM("Runtime Library Edit:", _settings.RuntimeLibraryEdit, (wrapper, rle) =>
        {
            ApplyMapChange(
                "Change runtime library edit",
                _settings.RuntimeLibraryEdit,
                rle,
                runtimeLibraryEdit =>
                {
                    _settings.RuntimeLibraryEdit = runtimeLibraryEdit;
                    wrapper.UpdateValue(runtimeLibraryEdit);
                });
        }));

        PropertiesForm.Inputs.Add(new InputBoolVM("Runtime Graph Edit:", _settings.RuntimeGraphEdit, (wrapper, rge) =>
        {
            ApplyMapChange(
                "Change runtime graph edit",
                _settings.RuntimeGraphEdit,
                rge,
                runtimeGraphEdit =>
                {
                    _settings.RuntimeGraphEdit = runtimeGraphEdit;
                    wrapper.UpdateValue(runtimeGraphEdit);
                });
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
            new InputStringVM("Font Family:", _style.FontFamily, (input, fontFamily) =>
            {
                var value = string.IsNullOrWhiteSpace(fontFamily) ? null : fontFamily;
                ApplyMapChange(
                    "Change font family",
                    _style.FontFamily,
                    value,
                    resolvedFontFamily =>
                    {
                        _style.FontFamily = resolvedFontFamily;
                        input.UpdateValue(resolvedFontFamily);
                    });
            }),

            new InputFloatVM("Font Size:", _style.FontSize, (input, fontSize) =>
            {
                ApplyMapChange(
                    "Change font size",
                    _style.FontSize,
                    fontSize,
                    resolvedFontSize =>
                    {
                        _style.FontSize = resolvedFontSize;
                        input.UpdateValue(resolvedFontSize);
                    });
            }),

            new InputFloatVM("Header Font Size:", _style.HeaderFontSize, (input, headerFontSize) =>
            {
                ApplyMapChange(
                    "Change header font size",
                    _style.HeaderFontSize,
                    headerFontSize,
                    resolvedHeaderFontSize =>
                    {
                        _style.HeaderFontSize = resolvedHeaderFontSize;
                        input.UpdateValue(resolvedHeaderFontSize);
                    });
            }),

            new InputColorVM("Font Color:", _style.FontColor.ToMediaColor(), (input, color) =>
            {
                ApplyMapChange(
                    "Change font color",
                    _style.FontColor.ToMediaColor(),
                    color,
                    resolvedColor =>
                    {
                        _style.FontColor = resolvedColor.ToNodeColor();
                        input.UpdateValue(resolvedColor);
                    });
            }),
        ];
    }

    private List<DataInputWrapperVM> CreateLayoutInputs()
    {
        return
        [
            new InputFloatVM("Padding:", _style.Padding, (input, padding) =>
            {
                ApplyMapChange(
                    "Change padding",
                    _style.Padding,
                    padding,
                    resolvedPadding =>
                    {
                        _style.Padding = resolvedPadding;
                        input.UpdateValue(resolvedPadding);
                    });
            }),

            new InputFloatVM("Row Spacing:", _style.RowSpacing, (input, rowSpacing) =>
            {
                ApplyMapChange(
                    "Change row spacing",
                    _style.RowSpacing,
                    rowSpacing,
                    resolvedRowSpacing =>
                    {
                        _style.RowSpacing = resolvedRowSpacing;
                        input.UpdateValue(resolvedRowSpacing);
                    });
            }),

            new InputFloatVM("Column Spacing:", _style.ColumnSpacing, (input, columnSpacing) =>
            {
                ApplyMapChange(
                    "Change column spacing",
                    _style.ColumnSpacing,
                    columnSpacing,
                    resolvedColumnSpacing =>
                    {
                        _style.ColumnSpacing = resolvedColumnSpacing;
                        input.UpdateValue(resolvedColumnSpacing);
                    });
            }),

            new InputFloatVM("Rounded Corner Radius:", _style.RoundedCornerRadius, (input, radius) =>
            {
                ApplyMapChange(
                    "Change rounded corner radius",
                    _style.RoundedCornerRadius,
                    radius,
                    resolvedRadius =>
                    {
                        _style.RoundedCornerRadius = resolvedRadius;
                        input.UpdateValue(resolvedRadius);
                    });
            }),

            new InputFloatVM("Control Corner Radius:", _style.ControlCornerRadius, (input, radius) =>
            {
                ApplyMapChange(
                    "Change control corner radius",
                    _style.ControlCornerRadius,
                    radius,
                    resolvedRadius =>
                    {
                        _style.ControlCornerRadius = resolvedRadius;
                        input.UpdateValue(resolvedRadius);
                    });
            }),

            new InputFloatVM("Minimum Column Width:", _style.MinimumColumnWidth, (input, width) =>
            {
                ApplyMapChange(
                    "Change minimum column width",
                    _style.MinimumColumnWidth,
                    width,
                    resolvedWidth =>
                    {
                        _style.MinimumColumnWidth = resolvedWidth;
                        input.UpdateValue(resolvedWidth);
                    });
            }),

            new InputFloatVM("Port Size:", _style.PortSize, (input, size) =>
            {
                ApplyMapChange(
                    "Change port size",
                    _style.PortSize,
                    size,
                    resolvedSize =>
                    {
                        _style.PortSize = resolvedSize;
                        input.UpdateValue(resolvedSize);
                    });
            }),

            new InputEnumVM("Port Placement:", _style.PortPlacement, (input, placement) =>
            {
                if (placement is not NodePortPlacement resolvedPlacement)
                    return;

                ApplyMapChange(
                    "Change port placement",
                    _style.PortPlacement,
                    resolvedPlacement,
                    placementValue =>
                    {
                        _style.PortPlacement = placementValue;
                        input.UpdateValue(placementValue);
                    });
            }),
        ];

    }

    private void NotifyDocumentChanged()
    {
        OnDocumentChanged?.Invoke();
    }

    private bool ApplyMapChange<T>(string description, T oldValue, T newValue, Action<T> applyValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(applyValue);

        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return false;

        if (_recordHistory != null)
        {
            _recordHistory(
                description,
                () => applyValue(newValue),
                () => applyValue(oldValue));
            return true;
        }

        applyValue(newValue);
        OnDocumentChanged?.Invoke();
        return true;
    }


    #endregion Methods

}
