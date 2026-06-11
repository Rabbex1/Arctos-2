using System.Windows;
using System.Windows.Controls;
using Arctos.Nodes.Wpf.ViewModels;

namespace Arctos.Nodes.Wpf.Views;

public partial class NodeMapView : UserControl
{
    public NodeMapView()
    {
        InitializeComponent();
    }

    private void OnTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not NodeMapVM nodeMap || e.NewValue is not INodeTreeItemVM item)
            return;

        if (!item.IsSelectable)
        {
            item.IsSelected = false;
            return;
        }

        nodeMap.SelectTreeItem(item);
    }
}
