using System.ComponentModel;
using System.IO;
using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Shell.Documents;

namespace Arctos.Nodes.Wpf.ViewModels.Documents;

/// <summary>
/// File-backed shell document wrapper for <c>.nlib</c> files.
/// </summary>
public sealed class NodeLibraryDocumentVM : NodeDocumentVM, INodeLibraryDocumentVM
{
    private readonly NodesManager _nodesManager;
    private NodeLibraryVM? _nodeLibrary;

    public NodesManager NodesManager => _nodesManager;

    public NodeLibraryVM NodeLibrary
    {
        get => _nodeLibrary ?? throw new InvalidOperationException("The node library has not been created or loaded yet.");
        private set
        {
            if (_nodeLibrary == value)
                return;

            if (_nodeLibrary != null)
                _nodeLibrary.PropertyChanged -= OnNodeLibraryPropertyChanged;

            _nodeLibrary = value;
            _nodeLibrary.PropertyChanged += OnNodeLibraryPropertyChanged;
            _nodeLibrary.SetDocumentChangedCallback(MarkChanged);

            OnPropertyChanged();
            OnPropertyChanged(nameof(DocumentName));
            OnPropertyChanged(nameof(DefinitionCount));
            OnPropertyChanged(nameof(SelectedHost));
            OnPropertyChanged(nameof(PropertiesForm));
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowDetailedViewToggle));
        }
    }

    public override Guid? DocumentId => null;

    public override string DocumentName => _nodeLibrary?.DisplayName ?? string.Empty;

    public override int DefinitionCount => _nodeLibrary?.DefinitionCount ?? 0;

    public override IModelHost? SelectedHost => _nodeLibrary?.SelectedHost;

    public NodeLibraryDocumentVM(NodesManager nodesManager, NodeSession session)
        : base(session)
    {
        _nodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
        DisplayName = "Untitled.nlib";
        FilePath = string.Empty;
        RefreshCanvasBackgroundColor();
        UpdateNavLinksFromSelection(null);
        OnPropertyChanged(nameof(ShowDetailedViewToggle));
    }

    public override void Closing()
    {
        if (_nodeLibrary != null)
            _nodesManager.RemoveLibrary(_nodeLibrary.Library);
    }

    protected override bool DoLoad()
    {
        NodeLibrary = new NodeLibraryVM(_nodesManager.LoadLibraryAndAdd(FilePath));
        return true;
    }

    protected override bool DoSave()
    {
        _nodeLibrary ??= new NodeLibraryVM(CreateBlankLibrary());
        _nodesManager.SaveLibrary(_nodeLibrary.Library, FilePath);
        IsChanged = false;
        return true;
    }

    protected override void FilePathChanged(string value)
    {
        base.FilePathChanged(value);

        if (_nodeLibrary == null)
            return;

        if (string.IsNullOrWhiteSpace(_nodeLibrary.LibraryName))
            _nodeLibrary.LibraryName = Path.GetFileNameWithoutExtension(value);

        OnPropertyChanged(nameof(DocumentName));
    }

    private NodeLibrary CreateBlankLibrary()
    {
        var fileName = string.IsNullOrWhiteSpace(FilePath)
            ? Path.GetFileNameWithoutExtension(DisplayName) ?? "Untitled"
            : Path.GetFileNameWithoutExtension(FilePath);

        var library = new NodeLibrary
        {
            Name = fileName,
            Version = "1.0.0",
        };

        _nodesManager.AddLibrary(library);
        return library;
    }

    private void OnNodeLibraryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeLibraryVM.SelectedHost))
        {
            OnPropertyChanged(nameof(SelectedHost));
            OnPropertyChanged(nameof(PropertiesForm));
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowDetailedViewToggle));
        }

        if (e.PropertyName == nameof(NodeLibraryVM.LibraryName) || e.PropertyName == nameof(NodeLibraryVM.DisplayName))
            OnPropertyChanged(nameof(DocumentName));
    }

    protected override NodeColor ResolveCanvasBackgroundColor()
    {
        return NodeColor.Neutral.Surface;
    }
}
