using System.Windows;
using System.Windows.Controls;
using Arctos.Nodes.Wpf.ViewModels;

namespace Arctos.Nodes.Wpf.Views;

public partial class NodeLibraryView : UserControl
{
    public NodeLibraryView()
    {
        InitializeComponent();
    }

    private void OnTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is not NodeLibraryVM nodeLibrary || e.NewValue is not INodeTreeItemVM item)
            return;

        if (!item.IsSelectable)
        {
            item.IsSelected = false;
            return;
        }

        nodeLibrary.SelectDefinition(item as NodeDefinitionVM);
    }
}
