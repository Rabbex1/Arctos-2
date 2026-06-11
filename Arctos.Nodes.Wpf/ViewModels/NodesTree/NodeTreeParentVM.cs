using Arctos;

namespace Arctos.Nodes.Wpf.ViewModels;

public abstract class NodeTreeParentVM<TChild, TParent> : ParentVM<TChild, TParent>, INodeTreeItemVM
    where TParent : class
    where TChild : class
{
    private bool _isSelected;
    private bool _isExpanded;

    protected NodeTreeParentVM(TParent? parent)
        : base(parent)
    {
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public virtual bool IsSelectable => true;
}
