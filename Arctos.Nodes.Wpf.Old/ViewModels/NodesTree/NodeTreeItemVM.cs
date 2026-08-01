using Arctos;

namespace Arctos.Nodes.Wpf.ViewModels;

public abstract class NodeTreeItemVM<TParent> : ChildVM<TParent>, INodeTreeItemVM
    where TParent : class
{
    private bool _isSelected;
    private bool _isExpanded;

    protected NodeTreeItemVM(TParent? parent)
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
