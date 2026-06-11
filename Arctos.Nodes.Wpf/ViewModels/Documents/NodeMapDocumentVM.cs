using System.ComponentModel;
using System.IO;
using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Shell.Documents;

namespace Arctos.Nodes.Wpf.ViewModels.Documents;

/// <summary>
/// File-backed shell document wrapper for <c>.nmap</c> files.
/// </summary>
public sealed class NodeMapDocumentVM : NodeDocumentVM, INodeMapDocumentVM
{
    private readonly NodesManager _nodesManager;
    private NodeMapVM? _nodeMap;

    public NodesManager NodesManager => _nodesManager;

    public NodeMapVM NodeMap
    {
        get => _nodeMap ?? throw new InvalidOperationException("The node map has not been created or loaded yet.");
        private set
        {
            if (_nodeMap == value)
                return;

            if (_nodeMap != null)
                _nodeMap.PropertyChanged -= OnNodeMapPropertyChanged;

            _nodeMap = value;
            _nodeMap.PropertyChanged += OnNodeMapPropertyChanged;
            _nodeMap.SetDocumentChangedCallback(MarkChanged);

            OnPropertyChanged();
            OnPropertyChanged(nameof(LocalLibrary));
            OnPropertyChanged(nameof(DocumentId));
            OnPropertyChanged(nameof(DocumentName));
            OnPropertyChanged(nameof(DefinitionCount));
            OnPropertyChanged(nameof(SelectedHost));
            OnPropertyChanged(nameof(PropertiesForm));
            RefreshCanvasBackgroundColor();
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowDetailedViewToggle));
        }
    }

    public NodeLibraryVM? LocalLibrary => _nodeMap?.LocalLibrary;

    public override Guid? DocumentId => _nodeMap?.Id;

    public override string DocumentName => _nodeMap?.MapName ?? string.Empty;

    public override int DefinitionCount => _nodeMap?.DefinitionCount ?? 0;

    public override IModelHost? SelectedHost => _nodeMap?.SelectedHost;

    public NodeMapDocumentVM(NodesManager nodesManager, NodeSession session)
        : base(session)
    {
        _nodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
        DisplayName = "Untitled.nmap";
        FilePath = string.Empty;
        RefreshCanvasBackgroundColor();
        UpdateNavLinksFromSelection(null);
        OnPropertyChanged(nameof(ShowDetailedViewToggle));
    }

    public override void Closing()
    {
        if (_nodeMap != null)
            _nodesManager.RemoveMap(_nodeMap.Map);
    }

    protected override bool DoLoad()
    {
        NodeMap = new NodeMapVM(_nodesManager.LoadMapAndAdd(FilePath));
        return true;
    }

    protected override bool DoSave()
    {
        _nodeMap ??= new NodeMapVM(CreateBlankMap());
        _nodesManager.SaveMap(_nodeMap.Map, FilePath);
        IsChanged = false;
        return true;
    }

    protected override void FilePathChanged(string value)
    {
        base.FilePathChanged(value);

        if (_nodeMap == null)
            return;

        _nodeMap.MapName = Path.GetFileNameWithoutExtension(value);
        OnPropertyChanged(nameof(DocumentName));
    }

    private NodeMap CreateBlankMap()
    {
        var settings = new NodeMapSettings
        {
            Name = string.IsNullOrWhiteSpace(FilePath)
                ? Path.GetFileNameWithoutExtension(DisplayName) ?? "Untitled"
                : Path.GetFileNameWithoutExtension(FilePath),
        };

        var map = new NodeMap(settings, new NodeGraph(), [new NodeLibrary()]);
        _nodesManager.AddMap(map);
        return map;
    }

    private void OnNodeMapPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeMapVM.SelectedHost))
        {
            OnPropertyChanged(nameof(SelectedHost));
            OnPropertyChanged(nameof(PropertiesForm));
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowDetailedViewToggle));
        }

        if (e.PropertyName == nameof(NodeMapVM.MapName))
            OnPropertyChanged(nameof(DocumentName));
    }

    protected override NodeColor ResolveCanvasBackgroundColor()
    {
        return _nodeMap?.Settings.Settings.CanvasColor ?? NodeColor.White;
    }
}
