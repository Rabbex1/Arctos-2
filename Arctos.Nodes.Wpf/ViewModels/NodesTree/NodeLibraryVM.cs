using Arctos.Nodes.Models;
using Arctos.Wpf.Inputs;
using Arctos.Wpf.Utilities;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Tree-based view model over a <see cref="NodeLibrary"/>.
/// </summary>
public sealed class NodeLibraryVM : NodeTreeParentVM<NodeDefinitionVM, INodeTreeItemVM>
{
    private readonly NodeLibrary _library;
    private readonly MapStyle _defaultStyle = new();
    private NodeDefinitionVM? _selectedDefinition;
    private ICommand? _addDefinitionCommand;
    private Action? _onDocumentChanged;
    private Action<string, Action, Action>? _recordHistory;

    public NodeLibraryVM(NodeLibrary library, INodeTreeItemVM? parent = null, bool isExternal = false)
        : base(parent)
    {
        _library = library ?? throw new ArgumentNullException(nameof(library));
        IsExternal = isExternal;
        DisplayName = string.IsNullOrWhiteSpace(_library.Name) ? "Local Library" : _library.Name;
        IsExpanded = true;
        PropertiesForm.OnInputChanged = NotifyDocumentChanged;

        foreach (var definition in _library.Definitions)
        {
            var definitionVm = new NodeDefinitionVM(definition, this, isExternal);
            definitionVm.SetDocumentChangedCallback(_onDocumentChanged);
            definitionVm.SetHistoryRecorder(_recordHistory);
            AddChild(definitionVm);
        }

        MakePropertiesForm();
    }

    public NodeLibrary Library => _library;

    public bool IsExternal { get; }

    public string LibraryName => _library.Name ?? string.Empty;

    public string LibraryVersion => _library.Version ?? string.Empty;

    public string Name => LibraryName;

    public string Version => LibraryVersion;

    public int DefinitionCount => _library.Count;

    public IEnumerable<NodeDefinitionVM> Definitions => Children;

    public InputFormVM PropertiesForm { get; } = new();

    public ICommand AddDefinitionCommand => _addDefinitionCommand ??= new RelayCommand(_ => AddDefinition());

    public NodeDefinitionVM? SelectedDefinition
    {
        get => _selectedDefinition;
        set => SetProperty(ref _selectedDefinition, value);
    }

    public IModelHost? SelectedHost => _selectedDefinition;

    public override bool IsSelectable => false;

    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        _onDocumentChanged = onDocumentChanged;

        foreach (var definition in Children)
            definition.SetDocumentChangedCallback(onDocumentChanged);
    }

    public void SetHistoryRecorder(Action<string, Action, Action>? recordHistory)
    {
        _recordHistory = recordHistory;

        foreach (var definition in Children)
            definition.SetHistoryRecorder(recordHistory);
    }

    public void RefreshLibraryDisplay()
    {
        DisplayName = string.IsNullOrWhiteSpace(_library.Name) ? "Local Library" : _library.Name;
        OnPropertyChanged(nameof(LibraryName));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Version));
    }

    public void ClearSelection()
    {
        ClearSelection(true);
    }

    public void ClearSelection(bool clearTreeSelection)
    {
        if (clearTreeSelection && _selectedDefinition != null)
            _selectedDefinition.IsSelected = false;

        SelectedDefinition = null;
        OnPropertyChanged(nameof(SelectedHost));
    }

    public void SelectDefinition(NodeDefinitionVM? definition)
    {
        if (_selectedDefinition != null && !ReferenceEquals(_selectedDefinition, definition))
            _selectedDefinition.IsSelected = false;

        SelectedDefinition = definition;
        OnPropertyChanged(nameof(SelectedHost));
    }

    public void RemoveDefinition(NodeDefinitionVM definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!ReferenceEquals(definition.Parent, this))
            return;

        if (!_library.Remove(definition.TypeName))
            return;

        if (ReferenceEquals(SelectedDefinition, definition))
            SelectedDefinition = null;

        RemoveChild(definition);
        OnPropertyChanged(nameof(DefinitionCount));
        OnPropertyChanged(nameof(SelectedHost));
        NotifyDocumentChanged();
    }

    private void AddDefinition()
    {
        var typeName = GetNextTypeName();
        var definition = new NodeDefinition
        {
            TypeName = typeName,
            DisplayName = $"New Definition {DefinitionCount + 1}",
            DomainType = "Custom",
            NodeShape = NodeShape.RoundedRectangle,
        };

        _library.Add(definition);

        var definitionVm = new NodeDefinitionVM(definition, this, IsExternal);
        definitionVm.SetDocumentChangedCallback(_onDocumentChanged);
        definitionVm.SetHistoryRecorder(_recordHistory);
        AddChild(definitionVm);
        IsExpanded = true;

        SelectDefinition(definitionVm);
        definitionVm.IsSelected = true;

        OnPropertyChanged(nameof(DefinitionCount));
        OnPropertyChanged(nameof(SelectedHost));
        NotifyDocumentChanged();
    }

    private string GetNextTypeName()
    {
        var index = DefinitionCount + 1;

        while (true)
        {
            var candidate = $"NodeDefinition{index}";

            if (!_library.Contains(candidate))
                return candidate;

            index++;
        }
    }

    private void MakePropertiesForm()
    {
        PropertiesForm.Inputs.Add(new InputStringVM("Name:", _library.Name, (wrapper, name) =>
        {
            var value = name ?? string.Empty;
            ApplyLibraryChange(
                "Change library name",
                _library.Name,
                value,
                libraryName =>
                {
                    _library.Name = libraryName;
                    wrapper.UpdateValue(libraryName);
                    DisplayName = string.IsNullOrWhiteSpace(libraryName) ? "Local Library" : libraryName;
                    OnPropertyChanged(nameof(LibraryName));
                    OnPropertyChanged(nameof(Name));
                });
        }));

        PropertiesForm.Inputs.Add(new InputStringVM("Version:", _library.Version, (wrapper, version) =>
        {
            var value = version ?? string.Empty;
            ApplyLibraryChange(
                "Change library version",
                _library.Version,
                value,
                libraryVersion =>
                {
                    _library.Version = libraryVersion;
                    wrapper.UpdateValue(libraryVersion);
                    OnPropertyChanged(nameof(LibraryVersion));
                    OnPropertyChanged(nameof(Version));
                });
        }));

        PropertiesForm.Inputs.Add(new InputGroupVM(
            string.Empty,
            ("Typography", CreateReadOnlyTypographyInputs()),
            collapsable: true));

        PropertiesForm.Inputs.Add(new InputGroupVM(
            string.Empty,
            ("Layout", CreateReadOnlyLayoutInputs()),
            collapsable: true));
    }

    private List<DataInputWrapperVM> CreateReadOnlyTypographyInputs()
    {
        return
        [
            new InputStringVM("Font Family:", _defaultStyle.FontFamily, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Font Size:", _defaultStyle.FontSize, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Header Font Size:", _defaultStyle.HeaderFontSize, _ => { }) { IsReadOnly = true },
            new InputColorVM("Font Color:", _defaultStyle.FontColor.ToMediaColor(), _ => { }) { IsReadOnly = true },
        ];
    }

    private List<DataInputWrapperVM> CreateReadOnlyLayoutInputs()
    {
        return
        [
            new InputFloatVM("Padding:", _defaultStyle.Padding, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Row Spacing:", _defaultStyle.RowSpacing, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Column Spacing:", _defaultStyle.ColumnSpacing, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Rounded Corner Radius:", _defaultStyle.RoundedCornerRadius, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Control Corner Radius:", _defaultStyle.ControlCornerRadius, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Minimum Column Width:", _defaultStyle.MinimumColumnWidth, _ => { }) { IsReadOnly = true },
            new InputFloatVM("Port Size:", _defaultStyle.PortSize, _ => { }) { IsReadOnly = true },
            new InputEnumVM("Port Placement:", _defaultStyle.PortPlacement, _ => { }) { IsReadOnly = true },
        ];
    }

    private void NotifyDocumentChanged()
    {
        _onDocumentChanged?.Invoke();
    }

    private bool ApplyLibraryChange<T>(string description, T oldValue, T newValue, Action<T> applyValue)
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
        NotifyDocumentChanged();
        return true;
    }
}
