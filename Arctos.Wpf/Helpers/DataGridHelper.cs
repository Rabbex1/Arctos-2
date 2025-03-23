using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Arctos.Wpf.Helpers
{
  public class DataGridHelper : DependencyObject
  {
    private static readonly DependencyProperty ColumnStyleProperty = DependencyProperty.RegisterAttached
      (
        "ColumnStyle",
        typeof(Style),
        typeof(DataGridHelper),
        new PropertyMetadata(OnColumnStyleChanged)
      );

    public static void SetColumnStyle(DependencyObject element, Style value)
    {
      element.SetValue(ColumnStyleProperty, value);
    }

    public static Style GetColumnStyle(DependencyObject element)
    {
      return (Style)element.GetValue(ColumnStyleProperty);
    }

    private static void OnColumnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (DesignerProperties.GetIsInDesignMode(d))
        return;

      var column = (DataGridColumn)d;
      if (!(e.NewValue is Style columnStyle))
        return;

      foreach (var setterBase in columnStyle.Setters)
      {
        if (setterBase is not Setter setter)
          continue;
        if (setter.Value is BindingBase)
        {
          //Not done yet..
        }
        else
        {
          var type = column.GetType();
          var propertyInfo = type.GetProperty(setter.Property.Name);
          propertyInfo?.SetValue(column, setter.Value, null);
        }
      }
    }
  }
}
