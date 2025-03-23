using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class CartesianGraphic : UiGraphic
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency properties

        #region X
        public static readonly DependencyProperty XProperty = DependencyProperty.Register
        (
            "X",
            typeof(object),
            typeof(CartesianGraphic),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnXChanged)
        );

        public object? X
        {
            get => GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        private static void OnXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Y
        public static readonly DependencyProperty YProperty = DependencyProperty.Register
        (
            "Y",
            typeof(object),
            typeof(CartesianGraphic),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnYChanged)
        );

        public object? Y
        {
            get => GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        private static void OnYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #endregion Dependency properties
    }
}