using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Arctos.Nodes.Wpf.Views.ToolWindows;

/// <summary>
/// Hosts the node edit toolbox tabs.
/// </summary>
public partial class NodeEditToolsView : UserControl
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node edit tools view.
    /// </summary>
    public NodeEditToolsView()
    {
        InitializeComponent();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void OnEditTabsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CommitFocusedTextBox();
    }

    private static void CommitFocusedTextBox()
    {
        var dependencyObject = Keyboard.FocusedElement as DependencyObject;
        var textBox = FindAncestorOrSelf<TextBox>(dependencyObject);
        var bindingExpression = textBox?.GetBindingExpression(TextBox.TextProperty);

        bindingExpression?.UpdateSource();
    }

    private static T? FindAncestorOrSelf<T>(DependencyObject? dependencyObject)
        where T : DependencyObject
    {
        while (dependencyObject != null)
        {
            if (dependencyObject is T typedDependencyObject)
                return typedDependencyObject;

            dependencyObject = dependencyObject switch
            {
                Visual visual => VisualTreeHelper.GetParent(visual),
                Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                _ => null,
            };
        }

        return null;
    }

    #endregion Private Methods
}
