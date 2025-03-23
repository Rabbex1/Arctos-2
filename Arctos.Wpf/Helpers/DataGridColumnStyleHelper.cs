using System.Windows;

namespace Arctos.Wpf.Helpers
{
  public class DataGridColumnStyleHelper : FrameworkElement
  {
    public DataGridColumnStyleHelper() { }

    public static readonly DependencyProperty ElementStyleProperty = DependencyProperty.Register
      (
        "ElementStyle",
        typeof(Style),
        typeof(DataGridColumnStyleHelper)
      );

    public Style ElementStyle
    {
      get => (Style)GetValue(ElementStyleProperty);
      set => SetValue(ElementStyleProperty, value);
    }

    public static readonly DependencyProperty EditingElementStyleProperty = DependencyProperty.Register
    (
      "EditingElementStyle",
      typeof(Style),
      typeof(DataGridColumnStyleHelper)
    );

    public Style EditingElementStyle
    {
      get => (Style)GetValue(ElementStyleProperty);
      set => SetValue(ElementStyleProperty, value);
    }

  }
}
