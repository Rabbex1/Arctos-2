using Arctos.Nodes.Models;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels;

public class NodeInstanceVM : NodeTreeParentVM<NodeInstanceVM, INodeTreeItemVM>, IEditableProperties, IModelHost<LeafNode>
{
    private readonly LeafNode _node;
    private Action? _onDocumentChanged;

    protected NodeInstanceVM(LeafNode node, INodeTreeItemVM? parent)
        : base(parent)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        DisplayName = _node.NodeDefinitionName;
        PropertiesForm.OnInputChanged = NotifyDocumentChanged;
        MakePropertiesForm();
    }

    public LeafNode Model => _node;

    object IModelHost.Model => Model;

    string IModelHost.DisplayName => DisplayName ?? string.Empty;

    bool IModelHost.HasDetails => false;

    public string NodeId => _node.Id;

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

    public InputFormVM PropertiesForm { get; } = new();

    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        _onDocumentChanged = onDocumentChanged;

        foreach (var child in Children)
            child.SetDocumentChangedCallback(onDocumentChanged);
    }

    public static NodeInstanceVM Create(LeafNode node, INodeTreeItemVM? parent)
    {
        return node is CompositeNode composite
            ? new CompositeNodeVM(composite, parent)
            : new NodeInstanceVM(node, parent);
    }

    private void MakePropertiesForm()
    {
        PropertiesForm.Inputs.Add(new InputStringVM("Node Id:", _node.Id, _ => { }));
        PropertiesForm.Inputs.Add(new InputStringVM("Definition:", _node.NodeDefinitionName, definition =>
        {
            NodeDefinitionName = definition ?? string.Empty;
        }));
    }

    private void NotifyDocumentChanged()
    {
        _onDocumentChanged?.Invoke();
    }
}

public class NodeInstanceVM<TNode> : NodeInstanceVM, IModelHost<TNode>
    where TNode : LeafNode
{
    protected NodeInstanceVM(TNode node, INodeTreeItemVM? parent)
        : base(node, parent)
    {
    }

    public new TNode Model => (TNode)base.Model;
}
