using Arctos.Nodes.Models;
using System.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Tree-based view model over a <see cref="NodeMap"/>.
/// </summary>
public sealed class NodeMapVM : NodeTreeParentVM<INodeTreeItemVM, object>
{
    private readonly NodeMap _map;
    private readonly NodeMapSettingsVM _settings;
    private readonly NodeGraphVM _graph;
    private readonly NodeLibraryVM? _localLibrary;
    private readonly NodeReferencesVM _references;

    public NodeMapVM(NodeMap map)
        : base(null)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        DisplayName = _map.Settings.Name ?? "Node Map";
        _settings = new NodeMapSettingsVM(_map.Settings, _map.Style);
        _graph = new NodeGraphVM(_map.Graph, this);
        _references = new NodeReferencesVM(this);

        _graph.PropertyChanged += OnGraphPropertyChanged;

        var localLibrary = _map.Libraries.FirstOrDefault(static library =>
            string.IsNullOrWhiteSpace(library.Name) && string.IsNullOrWhiteSpace(library.Version));

        if (localLibrary != null)
        {
            _localLibrary = new NodeLibraryVM(localLibrary, this);
            _localLibrary.PropertyChanged += OnLocalLibraryPropertyChanged;
            AddChild(_localLibrary);
        }

        AddChild(_graph);
        AddChild(_references);

        foreach (var library in _map.Libraries.Where(library => !ReferenceEquals(library, localLibrary)))
        {
            var libraryVm = new NodeLibraryVM(library, _references, true);
            libraryVm.PropertyChanged += OnReferenceLibraryPropertyChanged;
            _references.AddChild(libraryVm);
        }
    }

    public NodeMap Map => _map;

    public NodeMapSettingsVM Settings => _settings;

    public NodeGraphVM Graph => _graph;

    public Guid Id => _map.Id;

    public string MapName
    {
        get => _map.Settings.Name ?? string.Empty;
        set
        {
            if (string.Equals(_map.Settings.Name, value, StringComparison.Ordinal))
                return;

            _map.Settings.Name = value;
            DisplayName = value;
            OnPropertyChanged();
        }
    }

    public NodeLibraryVM? LocalLibrary => _localLibrary;

    public NodeReferencesVM References => _references;

    public IReadOnlyList<NodeLibrary> Libraries => _map.Libraries;

    public IEnumerable<NodeLibraryVM> ReferencedLibraries => _references.Children;

    public int DefinitionCount => _map.Libraries.Sum(library => library.Count);

    public IModelHost? SelectedHost =>
        _localLibrary?.SelectedHost
        ?? _graph.SelectedHost
        ?? _references.SelectedHost;

    public void SelectTreeItem(INodeTreeItemVM? item)
    {
        if (item is { IsSelectable: false })
            return;

        _localLibrary?.ClearSelection(false);
        _graph.ClearSelection(false);

        foreach (var library in _references.Children)
            library.ClearSelection(false);

        switch (item)
        {
            case NodeDefinitionVM definition when definition.Parent is NodeLibraryVM library:
                library.SelectDefinition(definition);
                break;

            case NodeGraphVM:
                break;

            case NodeInstanceVM node:
                _graph.SelectNode(node);
                break;
        }

        OnPropertyChanged(nameof(SelectedHost));
    }

    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        _settings.OnDocumentChanged = onDocumentChanged;
        _localLibrary?.SetDocumentChangedCallback(onDocumentChanged);
        _graph.SetDocumentChangedCallback(onDocumentChanged);

        foreach (var library in _references.Children)
            library.SetDocumentChangedCallback(onDocumentChanged);
    }

    public void ClearSelection()
    {
        _localLibrary?.ClearSelection();
        _graph.ClearSelection();

        foreach (var library in _references.Children)
            library.ClearSelection();

        OnPropertyChanged(nameof(SelectedHost));
    }

    private void OnLocalLibraryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeLibraryVM.SelectedHost))
            OnPropertyChanged(nameof(SelectedHost));
    }

    private void OnGraphPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeGraphVM.SelectedHost))
            OnPropertyChanged(nameof(SelectedHost));
    }

    private void OnReferenceLibraryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeLibraryVM.SelectedHost))
            OnPropertyChanged(nameof(SelectedHost));
    }
}
