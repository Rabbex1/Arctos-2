using System.Windows;
using System.Windows.Media;

namespace Arctos.Wpf.Helpers
{
  //http://www.hardcodet.net/2008/02/find-wpf-parent

  public static class TreeClimberHelper
  {
    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the
    /// queried item.</param>
    /// <returns>The first parent item that matches the submitted
    /// type parameter. If not matching item can be found, a null
    /// reference is being returned.</returns>
    public static T? TryFindAncestor<T>(this DependencyObject child) where T : DependencyObject
    {
      while (true)
      {
        var parentObject = GetParent(child);

        switch (parentObject)
        {
          case null:
            return null;
          case T parent:
            return parent;
          default:
            child = parentObject;
            continue;
        }
      }
    }

    /// <summary>
    /// This method is an alternative to WPF's
    /// <see cref="VisualTreeHelper.GetParent"/> method, which also
    /// supports content elements. Keep in mind that for content element,
    /// this method falls back to the logical tree of the element!
    /// </summary>
    /// <param name="child">The item to be processed.</param>
    /// <returns>The submitted item's parent, if available. Otherwise
    /// null.</returns>
    public static DependencyObject? GetParent(this DependencyObject child)
    {
      DependencyObject parent;
      switch (child)
      {
        case null:
          return null;
        case ContentElement contentElement:
          parent = ContentOperations.GetParent(contentElement);
          if (parent != null)
            return parent;
          var fce = contentElement as FrameworkContentElement;
          return fce?.Parent;
      }

      //also try searching for parent in framework elements (such as DockPanel, etc)
      if (!(child is FrameworkElement frameworkElement))
        return VisualTreeHelper.GetParent(child);
      parent = frameworkElement.Parent;
      return parent ?? VisualTreeHelper.GetParent(child);
    }
  }
}
