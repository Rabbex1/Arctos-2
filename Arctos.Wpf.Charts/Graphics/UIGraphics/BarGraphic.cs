using System.Windows;
using System.Windows.Shapes;

namespace Arctos.Wpf.Charts
{
  public class BarGraphic : CartesianGraphic
  {
    #region BarStyle
    public static readonly DependencyProperty BarStyleProperty = DependencyProperty.Register
    (
      "BarStyle",
      typeof(Style),
      typeof(BarGraphic),
      new PropertyMetadata(default(Style), OnBarStyleChanged)
    );

    public Style BarStyle
    {
      get => (Style)GetValue(BarStyleProperty);
      set => SetValue(BarStyleProperty, value);
    }

    private static void OnBarStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region BarWidth
    public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register
    (
      "BarWidth",
      typeof(double),
      typeof(BarGraphic),
      new PropertyMetadata(default(double), OnBarWidthChanged)
    );

    public double BarWidth
    {
      get => (double)GetValue(BarWidthProperty);
      set => SetValue(BarWidthProperty, value);
    }

    private static void OnBarWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion BarWidth

    protected override Size MeasureOverride(Size availableSize)
    {
      Children.Clear();
      Rectangle rectangle;
      var nextElement = NextElement as CartesianGraphic;
      if (X == null || Y == null || nextElement == null && Math.Abs(BarWidth) < 10e-6)
        rectangle = new Rectangle();
      else
      {

        var startX = Host?.XAxis?.VisualRange?.ValueToVisual(X) ?? 0;
        var startY = Host?.YAxis?.VisualRange?.ValueToVisual(Y) ?? 0;
        double endX;
        if (nextElement == null)
          endX = startX + Host?.XAxis?.VisualRange?.ValueToVisualSpan(BarWidth) ?? 0;
        else
          endX = Host?.XAxis?.VisualRange?.ValueToVisual(nextElement.X) ?? 0;
        //var endY = Host.YAxis.VisualRange.ValueToVisual(nextElement.Y);

        var height = availableSize.Height - Host?.YAxis?.VisualRange?.ValueToVisual(Y) ?? 0;
        var width = Math.Abs(endX - startX);

        rectangle = new Rectangle
        {
          Style = BarStyle,
          Width = width,
          Height = height
        };
        rectangle.Width -= (rectangle.Margin.Left + rectangle.Margin.Right);
      }
      Children.Add(rectangle);
      rectangle.Measure(availableSize);
      return Children[0].DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      if (Host == null)
        return finalSize;
      var point = new Point
        (
          Host?.XAxis?.VisualRange?.ValueToVisual(X) ?? 0 - DesiredSize.Width / 2,
          Host?.YAxis?.VisualRange?.ValueToVisual(Y) ?? 0
        );
      Children[0].Arrange(new Rect(point, finalSize));

      return finalSize;
    }

  }
}
