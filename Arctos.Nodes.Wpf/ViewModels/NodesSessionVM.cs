using Arctos.Messenger;
using Arctos.Shell.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Coordinates the current nodes document and exposes the current editing state used by WPF tools.
/// </summary>
public partial class NodesSessionVM : ObservableObject, IRecipient<DocumentMessage>
{
    #region Fields

    private readonly IDocumentService _documentService;
    private readonly IMessenger _messenger;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the current nodes document host when the focused shell document participates in nodes editing.
    /// </summary>
    [ObservableProperty]
    private INodesDocumentHost? _currentDocument;

    /// <summary>
    /// Gets the current graph document when the focused shell document participates in nodes editing.
    /// </summary>
    public NodeDocument? CurrentNodeDocument => CurrentDocument?.NodeDocument;

    /// <summary>
    /// Gets the current editor mode.
    /// </summary>
    public OperationMode CurrentMode
    {
        get => CurrentNodeDocument?.Settings.StartupMode ?? OperationMode.GraphEdit;
        set
        {
            if (CurrentNodeDocument == null || CurrentNodeDocument.Settings.StartupMode == value)
                return;

            CurrentNodeDocument.Settings.StartupMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLibraryEditMode));
            OnPropertyChanged(nameof(IsGraphEditMode));
            OnPropertyChanged(nameof(IsRunMode));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the session is currently in library-edit mode.
    /// </summary>
    public bool IsLibraryEditMode => CurrentMode == OperationMode.LibraryEdit;

    /// <summary>
    /// Gets a value indicating whether the session is currently in graph-edit mode.
    /// </summary>
    public bool IsGraphEditMode => CurrentMode == OperationMode.GraphEdit;

    /// <summary>
    /// Gets a value indicating whether the session is currently in run mode.
    /// </summary>
    public bool IsRunMode => CurrentMode == OperationMode.Run;

    /// <summary>
    /// Gets the current node library when one is available.
    /// </summary>
    public NodeLibrary? NodeLibrary => CurrentNodeDocument?.NodeLibrary;

    /// <summary>
    /// Gets the current set of node definitions presented by the session.
    /// </summary>
    public ObservableCollection<NodeDefinitionVM> Definitions { get; } = [];

    /// <summary>
    /// Gets or sets the selected node definition in the active library.
    /// </summary>
    [ObservableProperty]
    private NodeDefinitionVM? _selectedDefinition;

    /// <summary>
    /// Gets the currently selected object for use by a generic properties toolbox.
    /// </summary>
    public object? SelectedObject => Components.SelectedComponent as object ?? SelectedDefinition;

    /// <summary>
    /// Gets the component editor VM for the selected definition.
    /// </summary>
    public NodeComponentsVM Components { get; }

    /// <summary>
    /// Gets the preview canvas VM for the selected definition.
    /// </summary>
    public LibraryCanvasVM Canvas { get; }

    /// <summary>
    /// Gets the active document host when the focused shell document participates in nodes editing.
    /// </summary>
    public INodesDocumentHost? ActiveDocument => CurrentDocument;

    /// <summary>
    /// Gets the active graph document when the focused shell document participates in nodes editing.
    /// </summary>
    public NodeDocument? ActiveNodeDocument => CurrentNodeDocument;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new nodes session.
    /// </summary>
    /// <param name="documentService">The shell document service used to resolve the active document.</param>
    /// <param name="messenger">The application messenger used to receive document activation events.</param>
    public NodesSessionVM(IDocumentService documentService, IMessenger messenger)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

        Components = new NodeComponentsVM(this);
        Components.PropertyChanged += ComponentsPropertyChanged;
        Canvas = new LibraryCanvasVM(this);

        CurrentDocument = _documentService.ActiveDocument as INodesDocumentHost;
        _messenger.RegisterRecipient<DocumentMessage>(this);
        RefreshDefinitions();
    }

    #endregion Constructors

    #region Partial Methods

    partial void OnCurrentDocumentChanged(INodesDocumentHost? value)
    {
        OnPropertyChanged(nameof(ActiveDocument));
        OnPropertyChanged(nameof(CurrentNodeDocument));
        OnPropertyChanged(nameof(ActiveNodeDocument));
        OnPropertyChanged(nameof(NodeLibrary));
        OnPropertyChanged(nameof(CurrentMode));
        OnPropertyChanged(nameof(IsLibraryEditMode));
        OnPropertyChanged(nameof(IsGraphEditMode));
        OnPropertyChanged(nameof(IsRunMode));
        RefreshDefinitions();
    }

    partial void OnSelectedDefinitionChanged(NodeDefinitionVM? value)
    {
        if (Components.SelectedComponent != null)
            Components.SelectedComponent = null;

        OnPropertyChanged(nameof(SelectedObject));
    }

    #endregion Partial Methods

    #region Public Methods

    /// <summary>
    /// Adds a new node definition to the current node library.
    /// </summary>
    public void AddDefinition()
    {
        if (NodeLibrary == null)
            return;

        var typeName = CreateUniqueTypeName();
        var definition = new NodeDefinition
        {
            TypeName = typeName,
            DisplayName = typeName,
            DomainType = typeName,
            NodeShape = NodeShape.RoundedRectangle,
        };

        NodeLibrary.Add(definition);
        RefreshDefinitions();
        SelectedDefinition = Definitions.FirstOrDefault(item => ReferenceEquals(item.Model, definition));
    }

    /// <summary>
    /// Removes the selected node definition from the current node library.
    /// </summary>
    public void RemoveSelectedDefinition()
    {
        if (NodeLibrary == null || SelectedDefinition == null)
            return;

        var selectedTypeName = SelectedDefinition.TypeName;
        if (!NodeLibrary.Remove(selectedTypeName))
            return;

        RefreshDefinitions();
        SelectedDefinition = Definitions.FirstOrDefault();
    }

    /// <summary>
    /// Renames the selected node definition.
    /// </summary>
    /// <param name="newTypeName">The new stable type name.</param>
    public void RenameSelectedDefinition(string? newTypeName)
    {
        if (NodeLibrary == null || SelectedDefinition == null || string.IsNullOrWhiteSpace(newTypeName))
            return;

        var normalizedName = newTypeName.Trim();
        if (string.Equals(SelectedDefinition.TypeName, normalizedName, StringComparison.Ordinal))
            return;

        if (NodeLibrary.Contains(normalizedName))
            throw new InvalidOperationException($"A node definition named '{normalizedName}' already exists.");

        var model = SelectedDefinition.Model;
        var originalTypeName = model.TypeName;

        NodeLibrary.Remove(originalTypeName);
        model.TypeName = normalizedName;
        NodeLibrary.Add(model);

        RefreshDefinitions();
        SelectedDefinition = Definitions.FirstOrDefault(item => ReferenceEquals(item.Model, model));
    }

    /// <summary>
    /// Reloads the node definitions from the current document library.
    /// </summary>
    public void RefreshDefinitions()
    {
        Definitions.Clear();

        if (NodeLibrary == null)
        {
            SelectedDefinition = null;
            OnPropertyChanged(nameof(NodeLibrary));
            OnPropertyChanged(nameof(SelectedObject));
            return;
        }

        foreach (var definition in NodeLibrary.Definitions.OrderBy(item => item.TypeName, StringComparer.Ordinal))
            Definitions.Add(new NodeDefinitionVM(definition));

        OnPropertyChanged(nameof(NodeLibrary));

        if (SelectedDefinition != null)
        {
            var selectedModel = SelectedDefinition.Model;
            SelectedDefinition = Definitions.FirstOrDefault(item => ReferenceEquals(item.Model, selectedModel));
        }

        SelectedDefinition ??= Definitions.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedObject));
    }

    #endregion Public Methods

    #region Message Handlers

    /// <inheritdoc />
    public void Receive(DocumentMessage message)
    {
        if (message.Action == DocumentAction.ActiveDocumentChanged)
        {
            CurrentDocument = message.Params?[0] as INodesDocumentHost;
            return;
        }

        if (message.Action != DocumentAction.DocumentRemoved)
            return;

        if (message.Params?[0] is not INodesDocumentHost removedDocument)
            return;

        if (ReferenceEquals(CurrentDocument, removedDocument))
            CurrentDocument = _documentService.ActiveDocument as INodesDocumentHost;
    }

    #endregion Message Handlers

    #region Private Methods

    private string CreateUniqueTypeName()
    {
        const string baseName = "NewNode";
        if (NodeLibrary == null)
            return baseName;

        if (!NodeLibrary.Contains(baseName))
            return baseName;

        var index = 1;
        while (NodeLibrary.Contains($"{baseName}{index}"))
            index++;

        return $"{baseName}{index}";
    }

    private void ComponentsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(NodeComponentsVM.SelectedComponent))
            return;

        OnPropertyChanged(nameof(SelectedObject));
    }

    #endregion Private Methods
}
