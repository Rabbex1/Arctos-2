namespace Arctos.Nodes.Wpf.ViewModels;

public interface INodeTreeItemVM
{
    bool IsSelected { get; set; }

    bool IsExpanded { get; set; }

    bool IsSelectable { get; }
}
