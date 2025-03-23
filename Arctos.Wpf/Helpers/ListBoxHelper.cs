using System.Collections.Specialized;
using System.Windows;
using ListBox = System.Windows.Controls.ListBox;

namespace Arctos.Wpf.Helpers
{
  public class ListBoxHelper
  {
    public static readonly DependencyProperty IsAutoScrollProperty = DependencyProperty.RegisterAttached
      ("IsAutoScroll",
      typeof(bool),
      typeof(ListBoxHelper),
      new UIPropertyMetadata(default(bool), OnIsAutoScrollChanged)
      );
    public static bool GetIsAutoScroll(DependencyObject obj)
    {
      return (bool)obj.GetValue(IsAutoScrollProperty);
    }
    public static void SetIsAutoScroll(DependencyObject obj, bool value)
    {
      obj.SetValue(IsAutoScrollProperty, value);
    }

    public static void OnIsAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is not ListBox lb)
        return;
      var val = (bool)e.NewValue;
      var ic = lb.Items;
      if (!(ic.SourceCollection is INotifyCollectionChanged data))
        return;
      var autoScroller = new NotifyCollectionChangedEventHandler((s1, e1) =>
        {
          var selectedItem = default(object);
          switch (e1.Action)
          {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Move:
                if (e1.NewItems != null) selectedItem = e1.NewItems?[^1];
                break;
            case NotifyCollectionChangedAction.Remove:
              if (ic.Count < e1.OldStartingIndex)
                selectedItem = ic[e1.OldStartingIndex - 1];
              else if (ic.Count > 0)
                selectedItem = ic[0];
              break;
            case NotifyCollectionChangedAction.Reset:
              if (ic.Count > 0)
                selectedItem = ic[0];
              break;
            case NotifyCollectionChangedAction.Replace:
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }

          if (selectedItem == default(object))
            return;
          ic.MoveCurrentTo(selectedItem);
          lb.ScrollIntoView(selectedItem);
        });

      if (val)
        data.CollectionChanged += autoScroller;
      else
        data.CollectionChanged -= autoScroller;
    }
  }
}
