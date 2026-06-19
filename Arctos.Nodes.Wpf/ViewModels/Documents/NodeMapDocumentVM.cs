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
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly NodesManager _nodesManager;
    private NodeMapVM? _nodeMap;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the owning nodes manager.
    /// </summary>
    public NodesManager NodesManager => _nodesManager;

    /// <summary>
    /// Gets or sets the wrapped node map view model.
    /// </summary>
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
            OnPropertyChanged(nameof(EditTabs));
            RefreshCanvasBackgroundColor();
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowOverviewToggle));
        }
    }

    /// <summary>
    /// Gets the local library view model when one exists for the map.
    /// </summary>
    public NodeLibraryVM? LocalLibrary => _nodeMap?.LocalLibrary;

    /// <inheritdoc />
    public override Guid? DocumentId => _nodeMap?.Id;

    /// <inheritdoc />
    public override string DocumentName => _nodeMap?.MapName ?? string.Empty;

    /// <inheritdoc />
    public override int DefinitionCount => _nodeMap?.DefinitionCount ?? 0;

    /// <inheritdoc />
    public override IModelHost? SelectedHost => _nodeMap?.SelectedHost;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node map document wrapper.
    /// </summary>
    /// <param name="nodesManager">The owning nodes manager.</param>
    /// <param name="session">The owning node session.</param>
    public NodeMapDocumentVM(NodesManager nodesManager, NodeSession session)
        : base(session)
    {
        _nodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
        DisplayName = "Untitled.nmap";
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
        if (_nodeMap != null)
            _nodesManager.RemoveMap(_nodeMap.Map);
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    /// <inheritdoc />
    protected override bool DoLoad()
    {
        NodeMap = new NodeMapVM(_nodesManager.LoadMapAndAdd(FilePath));
        return true;
    }

    /// <inheritdoc />
    protected override bool DoSave()
    {
        _nodeMap ??= new NodeMapVM(CreateBlankMap());
        _nodesManager.SaveMap(_nodeMap.Map, FilePath);
        IsChanged = false;
        return true;
    }

    /// <inheritdoc />
    protected override void FilePathChanged(string value)
    {
        base.FilePathChanged(value);

        if (_nodeMap == null)
            return;

        _nodeMap.MapName = Path.GetFileNameWithoutExtension(value);
        OnPropertyChanged(nameof(DocumentName));
    }

    /// <inheritdoc />
    protected override NodeColor ResolveCanvasBackgroundColor()
    {
        return _nodeMap?.Settings.Settings.CanvasColor ?? NodeColor.White;
    }

    /// <inheritdoc />
    protected override string? ResolveCanvasFontFamily()
    {
        return _nodeMap?.Map.Style.FontFamily;
    }

    /// <inheritdoc />
    protected override NodeColor ResolveCanvasFontColor()
    {
        return _nodeMap?.Map.Style.FontColor ?? NodeColor.Black;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasFontSize()
    {
        return _nodeMap?.Map.Style.FontSize ?? 12.0f;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasHeaderFontSize()
    {
        return _nodeMap?.Map.Style.HeaderFontSize ?? 14.0f;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasPadding()
    {
        return _nodeMap?.Map.Style.Padding ?? 1.0f;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasRowSpacing()
    {
        return _nodeMap?.Map.Style.RowSpacing ?? 0.0f;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasColumnSpacing()
    {
        return _nodeMap?.Map.Style.ColumnSpacing ?? 1.0f;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasRoundedCornerRadius()
    {
        return _nodeMap?.Map.Style.RoundedCornerRadius ?? 2.0f;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasMinimumColumnWidth()
    {
        return _nodeMap?.Map.Style.MinimumColumnWidth ?? 24.0f;
    }

    /// <inheritdoc />
    protected override float ResolveCanvasPortSize()
    {
        return _nodeMap?.Map.Style.PortSize ?? 1.5f;
    }

    /// <inheritdoc />
    protected override NodePortPlacement ResolveCanvasPortPlacement()
    {
        return _nodeMap?.Map.Style.PortPlacement ?? NodePortPlacement.Over;
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

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
            OnPropertyChanged(nameof(EditTabs));
            UpdateNavLinksFromSelection(SelectedHost);
            OnPropertyChanged(nameof(ShowOverviewToggle));
        }

        if (e.PropertyName == nameof(NodeMapVM.MapName))
            OnPropertyChanged(nameof(DocumentName));
    }

    #endregion Private Methods
}
