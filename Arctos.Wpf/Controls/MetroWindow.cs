using System.Windows;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace Arctos.Wpf.Controls
{
    [TemplatePart(Name = PartIcon, Type = typeof(UIElement))]
    [TemplatePart(Name = PartTitleBar, Type = typeof(UIElement))]
    [TemplatePart(Name = PartTitleBarBackground, Type = typeof(UIElement))]
    [TemplatePart(Name = PartWindowButtons, Type = typeof(WindowButtons))]

    public class MetroWindow : Window
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        //---------------------------------------------------------------------
        #region Constants

        private const string PartIcon = "PART_Icon";
        private const string PartTitleBar = "PART_TitleBar";
        private const string PartTitleBarBackground = "PART_TitleBarBackground";
        private const string PartWindowButtons = "PART_WindowButtons";

        #endregion Constants

        private FrameworkElement? _icon;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region ShowTitleBar
        public static readonly DependencyProperty ShowTitleBarProperty = DependencyProperty.Register
        (
          "ShowTitleBar",
          typeof(bool),
          typeof(MetroWindow),
          new PropertyMetadata(true, OnShowTitleBarChanged)
        );

        public bool ShowTitleBar
        {
            get => (bool)GetValue(ShowTitleBarProperty);
            set => SetValue(ShowTitleBarProperty, value);
        }

        private static void OnShowTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion ShowTitleBar

        #region ShowIconOnTitleBar
        public static readonly DependencyProperty ShowIconOnTitleBarProperty = DependencyProperty.Register
        (
          "ShowIconOnTitleBar",
          typeof(bool),
          typeof(MetroWindow),
          new PropertyMetadata(default(bool), OnShowIconOnTitleBarChanged)
        );

        public bool ShowIconOnTitleBar
        {
            get => (bool)GetValue(ShowIconOnTitleBarProperty);
            set => SetValue(ShowIconOnTitleBarProperty, value);
        }

        private static void OnShowIconOnTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MetroWindow)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibilityForIcon();
            }
        }
        #endregion ShowIconOnTitleBar

        #region ShowMinButton
        public static readonly DependencyProperty ShowMinButtonProperty = DependencyProperty.Register
        (
          "ShowMinButton",
          typeof(bool),
          typeof(MetroWindow),
          new PropertyMetadata(default(bool), OnSowMinButtonChanged)
        );

        public bool ShowMinButton
        {
            get => (bool)GetValue(ShowMinButtonProperty);
            set => SetValue(ShowMinButtonProperty, value);
        }

        private static void OnSowMinButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion SowMinButton

        #region ShowMaxButton
        public static readonly DependencyProperty ShowMaxButtonProperty = DependencyProperty.Register
        (
          "ShowMaxButton",
          typeof(bool),
          typeof(MetroWindow),
          new PropertyMetadata(default(bool), OnShowMaxButtonChanged)
        );

        public bool ShowMaxButton
        {
            get => (bool)GetValue(ShowMaxButtonProperty);
            set => SetValue(ShowMaxButtonProperty, value);
        }

        private static void OnShowMaxButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion ShowMaxButton

        #region ShowCloseButton
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register
        (
          "ShowCloseButton",
          typeof(bool),
          typeof(MetroWindow),
          new PropertyMetadata(default(bool), OnShowCloseButtonChanged)
        );

        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        private static void OnShowCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion ShowCloseButton

        #region TitleBarHeight
        public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register
        (
          "TitleBarHeight",
          typeof(double),
          typeof(MetroWindow),
          new PropertyMetadata(default(double), OnTitleBarHeightChanged)
        );

        public double TitleBarHeight
        {
            get => (double)GetValue(TitleBarHeightProperty);
            set => SetValue(TitleBarHeightProperty, value);
        }

        private static void OnTitleBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion TitleBarHeight

        #region TitleForground
        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register
        (
          "TitleForeground",
          typeof(Brush),
          typeof(MetroWindow),
          new PropertyMetadata(default(Brush), OnTitleForegroundChanged)
        );

        public Brush TitleForeground
        {
            get => (Brush)GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }

        private static void OnTitleForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion TitleForground

        #region TitleAlignment
        public static readonly DependencyProperty TitleAlignmentProperty = DependencyProperty.Register
        (
          "TitleAlignment",
          typeof(HorizontalAlignment),
          typeof(MetroWindow),
          new PropertyMetadata(default(HorizontalAlignment), OnTitleAlignmentChanged)
        );

        public HorizontalAlignment TitleAlignment
        {
            get => (HorizontalAlignment)GetValue(TitleAlignmentProperty);
            set => SetValue(TitleAlignmentProperty, value);
        }

        private static void OnTitleAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion TitleAlignment

        #region TitleBackgroundBrush

        public static readonly DependencyProperty TitleBackgroundBrushProperty = DependencyProperty.Register
        (
          "TitleBackgroundBrush",
          typeof(Brush),
          typeof(MetroWindow),
          new PropertyMetadata(default(Brush), OnTitleBackgroundBrushChanged)
        );

        public Brush TitleBackgroundBrush
        {
            get => (Brush)GetValue(TitleBackgroundBrushProperty);
            set => SetValue(TitleBackgroundBrushProperty, value);
        }

        private static void OnTitleBackgroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion TitleBackgroundBrush

        #region NonActiveTitleBackgroundBrush
        public static readonly DependencyProperty NonActiveTitleBackgroundBrushProperty = DependencyProperty.Register
        (
          "NonActiveTitleBackgroundBrush",
          typeof(Brush),
          typeof(MetroWindow),
          new PropertyMetadata(Brushes.Gray, OnNonActiveTitleBackgroundBrushChanged)
        );

        public Brush NonActiveTitleBackgroundBrush
        {
            get => (Brush)GetValue(NonActiveTitleBackgroundBrushProperty);
            set => SetValue(NonActiveTitleBackgroundBrushProperty, value);
        }

        private static void OnNonActiveTitleBackgroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion NonActiveTitleBackgroundBrush

        #region NonActiveTitleForegroundBrush
        public static readonly DependencyProperty NonActiveTitleForegroundBrushProperty = DependencyProperty.Register
        (
          "NonActiveTitleForegroundBrush",
          typeof(Brush),
          typeof(MetroWindow),
          new PropertyMetadata(Brushes.Black)
        );

        public Brush NonActiveTitleForegroundBrush
        {
            get => (Brush)GetValue(NonActiveTitleForegroundBrushProperty);
            set => SetValue(NonActiveTitleForegroundBrushProperty, value);
        }
        #endregion

        #region NonActiveBorderBrush
        public static readonly DependencyProperty NonActiveBorderBrushProperty = DependencyProperty.Register
        (
          "NonActiveBorderBrush",
          typeof(Brush),
          typeof(MetroWindow),
          new PropertyMetadata(Brushes.Gray, OnNonActiveBorderBrushChanged)
        );

        public Brush NonActiveBorderBrush
        {
            get => (Brush)GetValue(NonActiveBorderBrushProperty);
            set => SetValue(NonActiveBorderBrushProperty, value);
        }

        private static void OnNonActiveBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion NonActiveBorderBrush

        #region ActiveBorderThickness
        public static readonly DependencyProperty ActiveBorderThicknessProperty = DependencyProperty.Register
        (
          "ActiveBorderThickness",
          typeof(Thickness),
          typeof(MetroWindow),
          new PropertyMetadata(new Thickness(1.0), OnActiveBorderThicknessChanged)
        );

        public Thickness ActiveBorderThickness
        {
            get => (Thickness)GetValue(ActiveBorderThicknessProperty);
            set => SetValue(ActiveBorderThicknessProperty, value);
        }

        private static void OnActiveBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion ActiveBorderThickness

        #region WindowButtons
        public static readonly DependencyProperty WindowButtonsProperty = DependencyProperty.Register
        (
          "WindowButtons",
          typeof(WindowButtons),
          typeof(MetroWindow),
          new PropertyMetadata(null, OnWindowButtonsChanged)
        );

        public WindowButtons WindowButtons
        {
            get => (WindowButtons)GetValue(WindowButtonsProperty);
            set => SetValue(WindowButtonsProperty, value);
        }

        private static void OnWindowButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion WindowButtons

        #region IconTemplate
        public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register
        (
          "IconTemplate",
          typeof(DataTemplate),
          typeof(MetroWindow),
          new PropertyMetadata(default(DataTemplate), OnIconTemplateChanged)
        );

        public DataTemplate IconTemplate
        {
            get => (DataTemplate)GetValue(IconTemplateProperty);
            set => SetValue(IconTemplateProperty, value);
        }

        private static void OnIconTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion IconTemplate

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        static MetroWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroWindow), new FrameworkPropertyMetadata(typeof(MetroWindow)));
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            WindowButtons = new WindowButtons
            {
                ParentWindow = this
            };
            _icon = GetTemplateChild(PartIcon) as FrameworkElement;
            SetVisibilityForIcon();
        }

        private void SetVisibilityForIcon()
        {
            if (_icon == null)
                return;
            var isVisible = (ShowIconOnTitleBar && ShowTitleBar);
            var iconVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            _icon.Visibility = iconVisibility;
        }


        #endregion Public Methods

        #endregion Methods


    }
}
