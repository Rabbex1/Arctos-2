using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
    public class UiGraphic : Panel
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Host
        private ICanvasHost? _host;
        protected ICanvasHost? Host => _host ??= (Parent as ChartCanvas)?.CanvasHost;

        #endregion

        #endregion Properties

        #region BoundingElements
        public static readonly DependencyProperty BoundingElementsProperty = DependencyProperty.Register
        (
          "BoundingElements",
          typeof(BoundingElements),
          typeof(UiGraphic),
          new PropertyMetadata(BoundingElements.None, OnBoundingElementsChanged)
        );

        public BoundingElements BoundingElements
        {
            get => (BoundingElements)GetValue(BoundingElementsProperty);
            set => SetValue(BoundingElementsProperty, value);
        }

        private static void OnBoundingElementsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region NextElement
        public static readonly DependencyProperty NextElementProperty = DependencyProperty.Register
        (
          "NextElement",
          typeof(UiGraphic),
          typeof(UiGraphic),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnNextElementChanged)
        );

        public UiGraphic? NextElement
        {
            get => (UiGraphic?)GetValue(NextElementProperty);
            set => SetValue(NextElementProperty, value);
        }

        private static void OnNextElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region PreviousElement
        public static readonly DependencyProperty PreviousElementProperty = DependencyProperty.Register
        (
          "PreviousElement",
          typeof(UiGraphic),
          typeof(UiGraphic),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPreviousElementChanged)
        );

        public UiGraphic? PreviousElement
        {
            get => (UiGraphic?)GetValue(PreviousElementProperty);
            set => SetValue(PreviousElementProperty, value);
        }

        private static void OnPreviousElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

    }
}
