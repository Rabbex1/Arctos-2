using Arctos.Nodes.Models;

namespace Arctos.Nodes.Wpf.ViewModels;

public sealed class CompositeNodeVM : NodeInstanceVM<CompositeNode>
{
    public CompositeNodeVM(CompositeNode node, INodeTreeItemVM? parent)
        : base(node, parent)
    {
        foreach (var child in node.Children)
            AddChild(NodeInstanceVM.Create(child, this));
    }

    public CompositeNode CompositeNode => Model;
}
