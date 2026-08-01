using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.DragImage;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Tree-based view model over a placed leaf node instance.
/// </summary>
public class NodeInstanceVM : NodeTreeParentVM<NodeInstanceVM, INodeTreeItemVM>, IEditTabs, IModelHost<LeafNode>, IDragImageHost
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly LeafNode _node;
    private readonly InputFormVM _generalForm = new();
    private Action? _onDocumentChanged;
    private DragImage.DragImage? _dragImage;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the hosted node model.
    /// </summary>
    public LeafNode Model => _node;

    object IModelHost.Model => Model;

    string IModelHost.DisplayName => DisplayName ?? string.Empty;

    bool IModelHost.HasOverview => false;

    /// <summary>
    /// Gets the stable node identifier.
    /// </summary>
    public string NodeId => _node.Id;

    /// <summary>
    /// Gets or sets the definition name resolved by the node instance.
    /// </summary>
    public string NodeDefinitionName
    {
        get => _node.NodeDefinitionName;
        set
        {
            if (string.Equals(_node.NodeDefinitionName, value, StringComparison.Ordinal))
                return;

            _node.NodeDefinitionName = value;
            DisplayName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the node position in node-space.
    /// </summary>
    public NodePoint Position
    {
        get => _node.Position;
        set
        {
            if (_node.Position.Equals(value))
                return;

            _node.Position = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the edit tabs shown for the node instance.
    /// </summary>
    public IReadOnlyList<EditTabVM> EditTabs { get; }

    /// <summary>
    /// Gets or sets the cached drag image owned by this node-instance host.
    /// </summary>
    public DragImage.DragImage? DragImage
    {
        get => _dragImage;
        set => SetProperty(ref _dragImage, value);
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node instance view model.
    /// </summary>
    /// <param name="node">The node model to wrap.</param>
    /// <param name="parent">The parent tree item.</param>
    protected NodeInstanceVM(LeafNode node, INodeTreeItemVM? parent)
        : base(parent)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        DisplayName = _node.NodeDefinitionName;
        _generalForm.OnInputChanged = NotifyDocumentChanged;
        EditTabs =
        [
            new EditTabVM("General", _generalForm),
        ];
        MakeEditTabs();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Sets the callback invoked when the node instance mutates the document.
    /// </summary>
    /// <param name="onDocumentChanged">The callback to invoke when the node changes.</param>
    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        _onDocumentChanged = onDocumentChanged;

        foreach (var child in Children)
            child.SetDocumentChangedCallback(onDocumentChanged);
    }

    /// <summary>
    /// Creates the correct node instance view model for the provided node.
    /// </summary>
    /// <param name="node">The node model to wrap.</param>
    /// <param name="parent">The parent tree item.</param>
    /// <returns>The created node instance view model.</returns>
    public static NodeInstanceVM Create(LeafNode node, INodeTreeItemVM? parent)
    {
        return node is CompositeNode composite
            ? new CompositeNodeVM(composite, parent)
            : new NodeInstanceVM(node, parent);
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void MakeEditTabs()
    {
        _generalForm.Inputs.Add(new InputStringVM("Node Id:", _node.Id, _ => { }));
        _generalForm.Inputs.Add(new InputStringVM("Definition:", _node.NodeDefinitionName, definition =>
        {
            NodeDefinitionName = definition ?? string.Empty;
        }));
    }

    private void NotifyDocumentChanged()
    {
        _onDocumentChanged?.Invoke();
    }

    #endregion Private Methods
}

/// <summary>
/// Strongly typed node instance view model over a specific leaf node subtype.
/// </summary>
/// <typeparam name="TNode">The concrete node model type.</typeparam>
public class NodeInstanceVM<TNode> : NodeInstanceVM, IModelHost<TNode>
    where TNode : LeafNode
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the hosted node model as its concrete type.
    /// </summary>
    public new TNode Model => (TNode)base.Model;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new strongly typed node instance view model.
    /// </summary>
    /// <param name="node">The node model to wrap.</param>
    /// <param name="parent">The parent tree item.</param>
    protected NodeInstanceVM(TNode node, INodeTreeItemVM? parent)
        : base(node, parent)
    {
    }

    #endregion Constructors
}
