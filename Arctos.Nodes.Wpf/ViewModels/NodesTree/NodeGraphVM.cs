using Arctos.Nodes.Models;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Tree-based view model over a <see cref="NodeGraph"/>.
/// </summary>
public sealed class NodeGraphVM : NodeTreeParentVM<NodeInstanceVM, INodeTreeItemVM>, IModelHost<NodeGraph>
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly NodeGraph _graph;
    private NodeInstanceVM? _selectedNode;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the hosted graph model.
    /// </summary>
    public NodeGraph Model => _graph;

    object IModelHost.Model => Model;

    string IModelHost.DisplayName => DisplayName ?? string.Empty;

    bool IModelHost.HasOverview => true;

    /// <summary>
    /// Gets the hosted graph model.
    /// </summary>
    public NodeGraph Graph => _graph;

    /// <summary>
    /// Gets the number of root nodes in the graph.
    /// </summary>
    public int RootNodeCount => _graph.Nodes.Count;

    /// <summary>
    /// Gets the total number of nodes in the graph, including nested composite children.
    /// </summary>
    public int TotalNodeCount => _graph.Nodes.Sum(CountNodeRecursive);

    /// <summary>
    /// Gets or sets the currently selected node within the graph tree.
    /// </summary>
    public NodeInstanceVM? SelectedNode
    {
        get => _selectedNode;
        set => SetProperty(ref _selectedNode, value);
    }

    /// <summary>
    /// Gets the currently selected host for properties and navigation. When no node is selected, the graph is selected.
    /// </summary>
    public IModelHost? SelectedHost => _selectedNode as IModelHost ?? this;

    /// <inheritdoc />
    public override bool IsSelectable => true;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new graph view model.
    /// </summary>
    /// <param name="graph">The graph model to wrap.</param>
    /// <param name="parent">The parent tree item.</param>
    public NodeGraphVM(NodeGraph graph, INodeTreeItemVM? parent = null)
        : base(parent)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        DisplayName = "Graph";
        IsExpanded = true;

        foreach (var node in _graph.Nodes)
            AddChild(NodeInstanceVM.Create(node, this));
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Applies the document changed callback to each child node in the graph.
    /// </summary>
    /// <param name="onDocumentChanged">The callback invoked when a child mutates the document.</param>
    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        foreach (var child in Children)
            child.SetDocumentChangedCallback(onDocumentChanged);
    }

    /// <summary>
    /// Clears the current graph selection and the corresponding tree selection.
    /// </summary>
    public void ClearSelection()
    {
        ClearSelection(true);
    }

    /// <summary>
    /// Clears the current graph selection.
    /// </summary>
    /// <param name="clearTreeSelection"><see langword="true"/> to also clear the selected tree item; otherwise, <see langword="false"/>.</param>
    public void ClearSelection(bool clearTreeSelection)
    {
        if (clearTreeSelection && _selectedNode != null)
            _selectedNode.IsSelected = false;

        SelectedNode = null;
        OnPropertyChanged(nameof(SelectedHost));
    }

    /// <summary>
    /// Selects the provided node inside the graph.
    /// </summary>
    /// <param name="node">The node to select.</param>
    public void SelectNode(NodeInstanceVM? node)
    {
        if (_selectedNode != null && !ReferenceEquals(_selectedNode, node))
            _selectedNode.IsSelected = false;

        SelectedNode = node;
        OnPropertyChanged(nameof(SelectedHost));
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private static int CountNodeRecursive(LeafNode node)
    {
        if (node is not CompositeNode composite)
            return 1;

        return 1 + composite.Children.Sum(CountNodeRecursive);
    }

    #endregion Private Methods
}
