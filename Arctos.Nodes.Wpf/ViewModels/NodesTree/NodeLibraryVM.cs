using Arctos.Nodes.Models;
using Arctos.Wpf.Inputs;
using Arctos.Wpf.Utilities;
using System.Windows.Input;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Tree-based view model over a <see cref="NodeLibrary"/>.
/// </summary>
public sealed class NodeLibraryVM : NodeTreeParentVM<NodeDefinitionVM, INodeTreeItemVM>
{
    private readonly NodeLibrary _library;
    private NodeDefinitionVM? _selectedDefinition;
    private ICommand? _addDefinitionCommand;
    private Action? _onDocumentChanged;

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
            AddChild(definitionVm);
        }

        MakePropertiesForm();
    }

    public NodeLibrary Library => _library;

    public bool IsExternal { get; }

    public string LibraryName
    {
        get => _library.Name ?? string.Empty;
        set
        {
            if (string.Equals(_library.Name, value, StringComparison.Ordinal))
                return;

            _library.Name = value;
            DisplayName = string.IsNullOrWhiteSpace(value) ? "Local Library" : value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Name));
        }
    }

    public string LibraryVersion
    {
        get => _library.Version ?? string.Empty;
        set
        {
            if (string.Equals(_library.Version, value, StringComparison.Ordinal))
                return;

            _library.Version = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Version));
        }
    }

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
        PropertiesForm.Inputs.Add(new InputStringVM("Name:", _library.Name, name =>
        {
            LibraryName = name ?? string.Empty;
        }));

        PropertiesForm.Inputs.Add(new InputStringVM("Version:", _library.Version, version =>
        {
            LibraryVersion = version ?? string.Empty;
        }));
    }

    private void NotifyDocumentChanged()
    {
        _onDocumentChanged?.Invoke();
    }
}
