using System.ComponentModel;
using System.IO;
using Arctos.History;
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
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly NodesManager _nodesManager;
    private NodeLibraryVM? _nodeLibrary;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the owning nodes manager.
    /// </summary>
    public NodesManager NodesManager => _nodesManager;

    /// <summary>
    /// Gets or sets the wrapped node library view model.
    /// </summary>
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
            _nodeLibrary.SetHistoryRecorder(CreateHistoryRecorder());

            OnPropertyChanged();
            OnPropertyChanged(nameof(DocumentName));
            OnPropertyChanged(nameof(DefinitionCount));
            OnPropertyChanged(nameof(SelectedHost));
            OnPropertyChanged(nameof(EditTabs));
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowOverviewToggle));
        }
    }

    /// <inheritdoc />
    public override Guid? DocumentId => null;

    /// <inheritdoc />
    public override string DocumentName => _nodeLibrary?.DisplayName ?? string.Empty;

    /// <inheritdoc />
    public override int DefinitionCount => _nodeLibrary?.DefinitionCount ?? 0;

    /// <inheritdoc />
    public override IModelHost? SelectedHost => _nodeLibrary?.SelectedHost;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node library document wrapper.
    /// </summary>
    /// <param name="nodesManager">The owning nodes manager.</param>
    /// <param name="session">The owning node session.</param>
    public NodeLibraryDocumentVM(NodesManager nodesManager, NodeSession session, IHistoryService historyService, IHistoryContext history)
        : base(session, historyService, history)
    {
        _nodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
        DisplayName = "Untitled.nlib";
        FilePath = string.Empty;
        RefreshCanvasBackgroundColor();
        UpdateNavLinksFromSelection(null);
        OnPropertyChanged(nameof(ShowOverviewToggle));
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <inheritdoc />
    public override void Closing()
    {
        base.Closing();

        if (_nodeLibrary != null)
            _nodesManager.RemoveLibrary(_nodeLibrary.Library);
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    /// <inheritdoc />
    protected override bool DoLoad()
    {
        NodeLibrary = new NodeLibraryVM(_nodesManager.LoadLibraryAndAdd(FilePath));
        return true;
    }

    /// <inheritdoc />
    protected override bool DoSave()
    {
        _nodeLibrary ??= new NodeLibraryVM(CreateBlankLibrary());
        _nodesManager.SaveLibrary(_nodeLibrary.Library, FilePath);
        IsChanged = false;
        return true;
    }

    /// <inheritdoc />
    protected override void FilePathChanged(string value)
    {
        base.FilePathChanged(value);

        if (_nodeLibrary == null)
            return;

        if (string.IsNullOrWhiteSpace(_nodeLibrary.LibraryName))
            _nodeLibrary.LibraryName = Path.GetFileNameWithoutExtension(value);

        OnPropertyChanged(nameof(DocumentName));
    }

    /// <inheritdoc />
    protected override NodeColor ResolveCanvasBackgroundColor()
    {
        return NodeColor.Neutral.Surface;
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

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
            OnPropertyChanged(nameof(EditTabs));
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowOverviewToggle));
        }

        if (e.PropertyName == nameof(NodeLibraryVM.LibraryName) || e.PropertyName == nameof(NodeLibraryVM.DisplayName))
            OnPropertyChanged(nameof(DocumentName));
    }

    #endregion Private Methods
}
