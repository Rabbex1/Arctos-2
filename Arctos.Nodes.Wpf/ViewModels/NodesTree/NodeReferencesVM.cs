namespace Arctos.Nodes.Wpf.ViewModels;

public sealed class NodeReferencesVM : NodeTreeParentVM<NodeLibraryVM, INodeTreeItemVM>
{
    public NodeReferencesVM(INodeTreeItemVM? parent)
        : base(parent)
    {
        DisplayName = "References";
        IsExpanded = true;
    }

    public IModelHost? SelectedHost => Children.FirstOrDefault(library => library.SelectedHost != null)?.SelectedHost;

    public override bool IsSelectable => false;
}
