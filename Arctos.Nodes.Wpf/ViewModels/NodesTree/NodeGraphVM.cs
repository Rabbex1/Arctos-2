using Arctos.Nodes.Models;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Tree-based view model over <see cref="NodeGraph"/>.
/// </summary>
public sealed class NodeGraphVM : NodeTreeParentVM<NodeInstanceVM, INodeTreeItemVM>, IModelHost<NodeGraph>
{
    private readonly NodeGraph _graph;
    private NodeInstanceVM? _selectedNode;

    public NodeGraphVM(NodeGraph graph, INodeTreeItemVM? parent = null)
        : base(parent)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        DisplayName = "Graph";
        IsExpanded = true;

        foreach (var node in _graph.Nodes)
            AddChild(NodeInstanceVM.Create(node, this));
    }

    public NodeGraph Model => _graph;

    object IModelHost.Model => Model;

    string IModelHost.DisplayName => DisplayName ?? string.Empty;

    bool IModelHost.HasDetails => true;

    public NodeGraph Graph => _graph;

    public int RootNodeCount => _graph.Nodes.Count;

    public int TotalNodeCount => _graph.Nodes.Sum(CountNodeRecursive);

    public NodeInstanceVM? SelectedNode
    {
        get => _selectedNode;
        set => SetProperty(ref _selectedNode, value);
    }

    public IModelHost? SelectedHost => _selectedNode as IModelHost ?? this;

    public override bool IsSelectable => true;

    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        foreach (var child in Children)
            child.SetDocumentChangedCallback(onDocumentChanged);
    }

    public void ClearSelection()
    {
        ClearSelection(true);
    }

    public void ClearSelection(bool clearTreeSelection)
    {
        if (clearTreeSelection && _selectedNode != null)
            _selectedNode.IsSelected = false;

        SelectedNode = null;
        OnPropertyChanged(nameof(SelectedHost));
    }

    public void SelectNode(NodeInstanceVM? node)
    {
        if (_selectedNode != null && !ReferenceEquals(_selectedNode, node))
            _selectedNode.IsSelected = false;

        SelectedNode = node;
        OnPropertyChanged(nameof(SelectedHost));
    }

    private static int CountNodeRecursive(LeafNode node)
    {
        if (node is not CompositeNode composite)
            return 1;

        return 1 + composite.Children.Sum(CountNodeRecursive);
    }
}
