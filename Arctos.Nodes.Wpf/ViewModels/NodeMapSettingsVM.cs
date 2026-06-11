using System.ComponentModel;
using Arctos.Nodes.Models;
using Arctos.Wpf.Inputs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Minimal working view model over <see cref="NodeMapSettings"/>.
/// </summary>
public sealed class NodeMapSettingsVM : ObservableObject
{
    #region Fields

    private readonly NodeMapSettings _settings;

    #endregion Fields

    #region Properties

    public NodeMapSettings Settings => _settings;

    public InputFormVM PropertiesForm { get; } = new();

    public Action? OnDocumentChanged { get; set; }

    #endregion Properties

    #region Constructors

    public NodeMapSettingsVM(NodeMapSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
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

    }

    private void NotifyDocumentChanged()
    {
        OnDocumentChanged?.Invoke();
    }

}
