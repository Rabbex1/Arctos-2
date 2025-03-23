using System.Windows;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace Arctos.Wpf.Charts
{
    public class GridGraphic : UiGraphic
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency properties

        #region LineStyle

        public static readonly DependencyProperty LineStyleProperty = DependencyProperty.Register
        (
          "LineStyle",
          typeof(Style),
          typeof(LineGraphic),
          new PropertyMetadata(default(Style), OnLineStyleChanged)
        );

        public Style LineStyle
        {
            get => (Style)GetValue(LineStyleProperty);
            set => SetValue(LineStyleProperty, value);
        }

        private static void OnLineStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #endregion Dependency properties

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Math.Abs(ActualHeight) < 10e-3 || Math.Abs(ActualWidth) < 10e-3)
                return availableSize;
            Children.Clear();
            if (Host?.YAxis != null)
            {
                foreach (var hatching in Host.YAxis.TickSets)
                {
                    if (hatching.IsGridLine)
                    {
                        foreach (Tick hatch in hatching.Children)
                        {
                            if (!double.IsNaN(hatch.Center.Y))
                            {
                                var line = new Line
                                {
                                    Style = LineStyle,
                                    X1 = 0.0,
                                    Y1 = hatch.Center.Y,
                                    X2 = ActualWidth,
                                    Y2 = hatch.Center.Y
                                };
                                line.Measure(availableSize);
                                Children.Add(line);
                            }
                        }
                    }
                }
            }
            if (Host?.XAxis != null)
            {
                foreach (var hatching in Host.XAxis.TickSets)
                {
                    if (hatching.IsGridLine)
                    {
                        foreach (Tick hatch in hatching.Children)
                        {
                            if (!double.IsNaN(hatch.Center.X))
                            {
                                var line = new Line
                                {
                                    Style = LineStyle,
                                    X1 = hatch.Center.X,
                                    Y1 = 0,
                                    X2 = hatch.Center.X,
                                    Y2 = ActualHeight
                                };
                                line.Measure(availableSize);
                                Children.Add(line);
                            }
                        }
                    }
                }
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size totalSize)
        {
            foreach (UIElement child in Children)
            {
                var point = new Point(0, 0);
                if (child is Line)
                {
                    child.Arrange(new Rect(point, totalSize));

                }
            }

            return totalSize;
        }

    }
}
