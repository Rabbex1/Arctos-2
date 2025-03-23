using System;
using System.Windows;
using System.Windows.Shapes;

namespace Arctos.Wpf.Charts
{
    public class LineGraphic : CartesianGraphic
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private bool _slope;

        #endregion Fields

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
            Children.Clear();
            Line line;
            var nextElement = NextElement as CartesianGraphic;
            if (X == null || Y == null || nextElement == null)
                line = new Line();
            else
            {

                var startX = Host?.XAxis?.VisualRange?.ValueToVisual(X) ?? 0;
                var startY = Host?.YAxis?.VisualRange?.ValueToVisual(Y) ?? 0;
                var endX = Host?.XAxis?.VisualRange?.ValueToVisual(nextElement.X) ?? 0;
                var endY = Host?.YAxis?.VisualRange?.ValueToVisual(nextElement.Y) ?? 0;

                _slope = endY > startY;
                var height = Math.Abs(endY - startY);
                var width = Math.Abs(endX - startX);

                line = new Line
                {
                    Style = LineStyle,
                    X1 = 0.0,
                    Y1 = 0.0,
                    X2 = width,
                    Y2 = _slope ? height : -height
                };
            }
            Children.Add(line);
            line.Measure(availableSize);
            return Children[0].DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (X == null || Y == null)
                return finalSize;
            var y = Host?.YAxis?.VisualRange?.ValueToVisual(Y) ?? 0;
            var point = new Point
              (
                Host?.XAxis?.VisualRange?.ValueToVisual(X) ?? 0,
                _slope ? y : y - DesiredSize.Height
              );
            if (Children.Count > 0)
            {
                Children[0].Arrange(new Rect(point, finalSize));
            }
            return finalSize;
        }
    }
}
