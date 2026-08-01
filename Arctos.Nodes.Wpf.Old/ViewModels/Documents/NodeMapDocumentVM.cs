using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Arctos.History;
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
    private readonly record struct CanvasViewState(NodePoint Pan, float Zoom);
    private readonly record struct DefinitionSurfaceKey(bool IsLocalLibrary, string LibraryName, string LibraryVersion, string TypeName);

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly NodesManager _nodesManager;
    private readonly bool _isTransient;
    private NodeMapVM? _nodeMap;
    private NodeMapDocumentVM? _testDocument;
    private readonly Dictionary<DefinitionSurfaceKey, CanvasViewState> _testViewStates = [];

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the owning nodes manager.
    /// </summary>
    public NodesManager NodesManager => _nodesManager;

    /// <inheritdoc />
    public override INodeDocumentVM PresentationDocument => _testDocument ?? this;

    /// <inheritdoc />
    public override bool IsDesignMode => !_isTransient && _testDocument == null;

    /// <inheritdoc />
    public override bool CanToggleSurfaceMode => !_isTransient && (!IsDesignMode || SelectedHost is NodeDefinitionVM);

    /// <summary>
    /// Gets a value indicating whether test mode should restore the previous test canvas view state.
    /// This stays internal for now and can become a user option later.
    /// </summary>
    private bool ShouldRestoreTestViewState => false;

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
            _nodeMap.SetHistoryRecorder(CreateHistoryRecorder());

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
    public NodeMapDocumentVM(NodesManager nodesManager, NodeSession session, IHistoryService historyService, IHistoryContext history, bool isTransient = false)
        : base(session, historyService, history)
    {
        _nodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
        _isTransient = isTransient;
        DisplayName = "Untitled.nmap";
        FilePath = string.Empty;
        RefreshCanvasSurfaceMode();
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
        ExitTestMode(releaseHistoryOnly: true);
        base.Closing();

        if (_nodeMap != null)
            _nodesManager.RemoveMap(_nodeMap.Map);
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    /// <inheritdoc />
    protected override bool DoLoad()
    {
        if (!ValidateAndAutoFixComponentUids(
                FilePath,
                json => JsonSerializer.Deserialize<NodeMapFile>(json),
                file => _nodesManager.SaveMapFile(file, FilePath)))
        {
            return false;
        }

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

        _nodeMap.Settings.Settings.Name = Path.GetFileNameWithoutExtension(value);
        _nodeMap.DisplayName = _nodeMap.Settings.Settings.Name;
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
    protected override float ResolveCanvasControlCornerRadius()
    {
        return _nodeMap?.Map.Style.ControlCornerRadius ?? 1.0f;
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

    /// <inheritdoc />
    protected override void ToggleSurfaceMode()
    {
        if (!CanToggleSurfaceMode)
            return;

        if (IsDesignMode)
            EnterTestMode();
        else
            ExitTestMode();
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
            OnPropertyChanged(nameof(CanToggleSurfaceMode));
        }

        if (e.PropertyName == nameof(NodeMapVM.MapName))
            OnPropertyChanged(nameof(DocumentName));
    }

    private void EnterTestMode()
    {
        if (_nodeMap == null || SelectedHost is not NodeDefinitionVM selectedDefinition)
            return;

        var selectedDefinitionKey = CreateDefinitionSurfaceKey(selectedDefinition);
        var clonedMap = CloneMapForTest(_nodeMap.Map);
        var testHistoryId = $"{(DocumentId?.ToString() ?? FilePath ?? Guid.NewGuid().ToString())}::test";
        var testHistory = HistoryService.GetOrCreateContext(testHistoryId);
        var displayName = DisplayName;
        var filePath = FilePath;
        var testDocument = new NodeMapDocumentVM(_nodesManager, Session, HistoryService, testHistory, isTransient: true)
        {
            DisplayName = displayName,
            FilePath = filePath ?? string.Empty,
        };

        testDocument.NodeMap = new NodeMapVM(clonedMap);
        if (ShouldRestoreTestViewState)
            ApplyCachedTestViewStates(testDocument.NodeMap);

        _testDocument = testDocument;
        NotifySurfaceModeChanged();
        BeginSelectTestDefinition(testDocument, selectedDefinitionKey);
    }

    private void ExitTestMode(bool releaseHistoryOnly = false)
    {
        if (_testDocument == null)
            return;

        if (ShouldRestoreTestViewState)
            CaptureCachedTestViewStates(_testDocument.NodeMap);
        _testDocument.ReleaseHistoryContext();
        _testDocument = null;

        if (!releaseHistoryOnly)
            NotifySurfaceModeChanged();
    }

    private void NotifySurfaceModeChanged()
    {
        RefreshCanvasSurfaceMode();
        _testDocument?.RefreshCanvasSurfaceMode();
        OnPropertyChanged(nameof(PresentationDocument));
        OnPropertyChanged(nameof(IsDesignMode));
        OnPropertyChanged(nameof(IsTestMode));
        OnPropertyChanged(nameof(SurfaceMode));
        OnPropertyChanged(nameof(ToggleSurfaceModeText));
        OnPropertyChanged(nameof(CanToggleSurfaceMode));
    }

    private void BeginSelectTestDefinition(NodeMapDocumentVM testDocument, DefinitionSurfaceKey selectedDefinitionKey)
    {
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        _ = dispatcher.BeginInvoke(
            DispatcherPriority.ContextIdle,
            new Action(() =>
            {
                if (!ReferenceEquals(_testDocument, testDocument))
                    return;

                if (!TryResolveDefinition(testDocument.NodeMap, selectedDefinitionKey, out var clonedDefinition)
                    || clonedDefinition == null)
                {
                    return;
                }

                if (!ShouldRestoreTestViewState)
                {
                    clonedDefinition.CanvasPan = default;
                    clonedDefinition.CanvasZoom = 0.0f;
                }

                testDocument.NodeMap.SelectTreeItem(clonedDefinition);
            }));
    }

    private void ApplyCachedTestViewStates(NodeMapVM map)
    {
        foreach (var definition in EnumerateDefinitions(map))
        {
            var key = CreateDefinitionSurfaceKey(definition);
            if (!_testViewStates.TryGetValue(key, out var state))
                continue;

            definition.CanvasPan = state.Pan;
            definition.CanvasZoom = state.Zoom;
        }
    }

    private void CaptureCachedTestViewStates(NodeMapVM map)
    {
        foreach (var definition in EnumerateDefinitions(map))
        {
            if (definition.CanvasZoom <= 0.0f)
                continue;

            _testViewStates[CreateDefinitionSurfaceKey(definition)] = new CanvasViewState(definition.CanvasPan, definition.CanvasZoom);
        }
    }

    private NodeMap CloneMapForTest(NodeMap sourceMap)
    {
        ArgumentNullException.ThrowIfNull(sourceMap);

        var file = NodeMapFile.FromMap(sourceMap);
        var json = JsonSerializer.Serialize(file);
        var clonedFile = JsonSerializer.Deserialize<NodeMapFile>(json)
            ?? throw new InvalidOperationException("The node map test clone could not be deserialized.");

        var referencedLibraries = sourceMap.Libraries
            .Where(static library => !string.IsNullOrWhiteSpace(library.Name) && !string.IsNullOrWhiteSpace(library.Version))
            .ToDictionary(
                static library => $"{library.Name}::{library.Version}",
                CloneLibraryForTest,
                StringComparer.Ordinal);

        return clonedFile.ToMap(reference =>
        {
            var key = $"{reference.Name}::{reference.Version}";
            if (referencedLibraries.TryGetValue(key, out var library))
                return library;

            throw new InvalidOperationException(
                $"The referenced library '{reference.Name}' version '{reference.Version}' could not be resolved for node test mode.");
        });
    }

    private static NodeLibrary CloneLibraryForTest(NodeLibrary sourceLibrary)
    {
        var json = JsonSerializer.Serialize(sourceLibrary);
        return JsonSerializer.Deserialize<NodeLibrary>(json)
               ?? throw new InvalidOperationException("The node library test clone could not be deserialized.");
    }

    private static IEnumerable<NodeDefinitionVM> EnumerateDefinitions(NodeMapVM map)
    {
        if (map.LocalLibrary != null)
        {
            foreach (var definition in map.LocalLibrary.Definitions)
                yield return definition;
        }

        foreach (var library in map.ReferencedLibraries)
        {
            foreach (var definition in library.Definitions)
                yield return definition;
        }
    }

    private static DefinitionSurfaceKey CreateDefinitionSurfaceKey(NodeDefinitionVM definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.Parent is not NodeLibraryVM library)
            throw new InvalidOperationException("Node test mode currently supports node definitions hosted by a library.");

        var isLocalLibrary = string.IsNullOrWhiteSpace(library.LibraryName) && string.IsNullOrWhiteSpace(library.LibraryVersion);
        return new DefinitionSurfaceKey(
            isLocalLibrary,
            library.LibraryName,
            library.LibraryVersion,
            definition.TypeName);
    }

    private static bool TryResolveDefinition(NodeMapVM map, DefinitionSurfaceKey key, out NodeDefinitionVM? definition)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (key.IsLocalLibrary)
        {
            definition = map.LocalLibrary?.Definitions.FirstOrDefault(candidate => candidate.TypeName == key.TypeName);
            return definition != null;
        }

        var library = map.ReferencedLibraries.FirstOrDefault(candidate =>
            string.Equals(candidate.LibraryName, key.LibraryName, StringComparison.Ordinal)
            && string.Equals(candidate.LibraryVersion, key.LibraryVersion, StringComparison.Ordinal));

        definition = library?.Definitions.FirstOrDefault(candidate => candidate.TypeName == key.TypeName);
        return definition != null;
    }

    #endregion Private Methods
}
