using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class PointGraphic : CartesianGraphic
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region Template
        public static readonly DependencyProperty TemplateProperty = DependencyProperty.Register
        (
            "Template",
            typeof(DataTemplate),
            typeof(PointGraphic),
            new PropertyMetadata(default(DataTemplate), OnTemplateChanged)
        );

        public DataTemplate Template
        {
            get => (DataTemplate)GetValue(TemplateProperty);
            set => SetValue(TemplateProperty, value);
        }

        private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PointGraphic point)
                return;
            if (e.NewValue is DataTemplate dataTemplate)
            {
                if (dataTemplate.LoadContent() is UIElement content)
                {
                    point.Children.Clear();
                    point.Children.Add(content);
                }
            }
        }
        #endregion

        #endregion Dependency Properties


        protected override Size MeasureOverride(Size availableSize)
        {
            Children[0].Measure(availableSize);
            return Children[0].DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Host == null)
                return finalSize;
            var point = new Point
            (
                Host?.XAxis?.VisualRange?.ValueToVisual(X) ?? 0 - DesiredSize.Width / 2,
                Host?.YAxis?.VisualRange?.ValueToVisual(Y) ?? 0 - DesiredSize.Height / 2
            );
            Children[0].Arrange(new Rect(point, finalSize));

            return finalSize;
        }

    }
}