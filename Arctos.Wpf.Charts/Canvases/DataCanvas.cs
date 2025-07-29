using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class DataCanvas : UiCanvas
    {

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public DataCanvas()
        {
            GraphicPool = new ObjectPool<UiGraphic?>(CreateGraphic);
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region GraphicTemplate
        public static readonly DependencyProperty GraphicTemplateProperty = DependencyProperty.Register
        (
          "GraphicTemplate",
          typeof(DataTemplate),
          typeof(DataCanvas),
          new PropertyMetadata(default(DataTemplate), OnGraphicTemplateChanged)
        );

        public DataTemplate GraphicTemplate
        {
            get => (DataTemplate)GetValue(GraphicTemplateProperty);
            set => SetValue(GraphicTemplateProperty, value);
        }

        private static void OnGraphicTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            if (CanvasHost?.PrimaryAxis == null)
                return availableSize;
            Children.Clear();
            GraphicPool?.Reset();

            //Create children
            CanvasHost.DataProvider?.ForAxisRange(CanvasHost.PrimaryAxis, obj =>
            {
                var graphic = GraphicPool?.Next();
                if (graphic != null)
                {
                    graphic.DataContext = obj;
                    Children.Add(graphic);
                }
            });

            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i] is not UiGraphic graphic)
                    continue;
                if (graphic.BoundingElements.HasFlag(BoundingElements.Next))
                    graphic.NextElement = i < Children.Count - 1 ? Children[i + 1] as UiGraphic : null;
                if (graphic.BoundingElements.HasFlag(BoundingElements.Previous))
                    graphic.NextElement = i > 0 ? Children[i - 1] as UiGraphic : null;
                graphic.Measure(availableSize);
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UiGraphic child in Children)
            {
                child.Arrange(new Rect(new Point(0, 0), child.DesiredSize));
            }

            return finalSize;
        }

        #endregion Protected Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private UiGraphic? CreateGraphic()
        {
            return GraphicTemplate.LoadContent() as UiGraphic;
        }

        #endregion Private Methods

        #endregion Methods

    }
}
