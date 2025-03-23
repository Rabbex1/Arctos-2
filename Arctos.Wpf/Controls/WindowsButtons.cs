using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;

namespace Arctos.Wpf.Controls
{
    [TemplatePart(Name = "PART_Min", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Max", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Close", Type = typeof(Button))]
    public class WindowButtons : ContentControl
    {

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private Button? _minButton;
        private Button? _maxRestoreButton;
        private Button? _closeButton;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public MetroWindow? ParentWindow { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region MinButtonStyle

        public static readonly DependencyProperty MinButtonStyleProperty = DependencyProperty.Register
        (
          "MinButtonStyle",
          typeof(Style),
          typeof(WindowButtons),
          new PropertyMetadata(default(Style), OnMinButtonStyleChanged)
        );

        public Style MinButtonStyle
        {
            get => (Style)GetValue(MinButtonStyleProperty);
            set => SetValue(MinButtonStyleProperty, value);
        }

        private static void OnMinButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion MinButtonStyle

        #region MaxButtonStyle

        public static readonly DependencyProperty MaxButtonStyleProperty = DependencyProperty.Register
        (
          "MaxButtonStyle",
          typeof(Style),
          typeof(WindowButtons),
          new PropertyMetadata(default(Style), OnMaxButtonStyleChanged)
        );

        public Style MaxButtonStyle
        {
            get => (Style)GetValue(MaxButtonStyleProperty);
            set => SetValue(MaxButtonStyleProperty, value);
        }

        private static void OnMaxButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion MaxButtonStyle

        #region RestoreButtonStyle

        public static readonly DependencyProperty RestoreButtonStyleProperty = DependencyProperty.Register
        (
          "RestoreButtonStyle",
          typeof(Style),
          typeof(WindowButtons),
          new PropertyMetadata(default(Style), OnRestoreButtonStyleChanged)
        );

        public Style RestoreButtonStyle
        {
            get => (Style)GetValue(RestoreButtonStyleProperty);
            set => SetValue(RestoreButtonStyleProperty, value);
        }

        private static void OnRestoreButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion RestoreButtonStyle

        #region CloseButtonStyle

        public static readonly DependencyProperty CloseButtonStyleProperty = DependencyProperty.Register
        (
          "CloseButtonStyle",
          typeof(Style),
          typeof(WindowButtons),
          new PropertyMetadata(default(Style), OnCloseButtonStyleChanged)
        );

        public Style CloseButtonStyle
        {
            get => (Style)GetValue(CloseButtonStyleProperty);
            set => SetValue(CloseButtonStyleProperty, value);
        }

        private static void OnCloseButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion CloseButtonStyle

        #region MinButtonToolTip

        public static readonly DependencyProperty MinButtonToolTipProperty = DependencyProperty.Register
        (
          "MinButtonToolTip",
          typeof(string),
          typeof(WindowButtons),
          new PropertyMetadata("Minimize", OnMinButtonToolTipChanged)
        );

        public string MinButtonToolTip
        {
            get => (string)GetValue(MinButtonToolTipProperty);
            set => SetValue(MinButtonToolTipProperty, value);
        }

        private static void OnMinButtonToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion MinButtonToolTip

        #region MaxButtonToolTip

        public static readonly DependencyProperty MaxButtonToolTipProperty = DependencyProperty.Register
        (
          "MaxButtonToolTip",
          typeof(string),
          typeof(WindowButtons),
          new PropertyMetadata("Maximize", OnMaxButtonToolTipChanged)
        );

        public string MaxButtonToolTip
        {
            get => (string)GetValue(MaxButtonToolTipProperty);
            set => SetValue(MaxButtonToolTipProperty, value);
        }

        private static void OnMaxButtonToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion MaxButtonToolTip

        #region RestoreButtonToolTip

        public static readonly DependencyProperty RestoreButtonToolTipProperty = DependencyProperty.Register
        (
          "RestoreButtonToolTip",
          typeof(string),
          typeof(WindowButtons),
          new PropertyMetadata("Restore", OnRestoreButtonToolTipChanged)
        );

        public string RestoreButtonToolTip
        {
            get => (string)GetValue(RestoreButtonToolTipProperty);
            set => SetValue(RestoreButtonToolTipProperty, value);
        }

        private static void OnRestoreButtonToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion RestoreButtonToolTip

        #region CloseButtonToolTip

        public static readonly DependencyProperty CloseButtonToolTipProperty = DependencyProperty.Register
        (
          "CloseButtonToolTip",
          typeof(string),
          typeof(WindowButtons),
          new PropertyMetadata("Close", OnCloseButtonToolTipChanged)
        );

        public string CloseButtonToolTip
        {
            get => (string)GetValue(CloseButtonToolTipProperty);
            set => SetValue(CloseButtonToolTipProperty, value);
        }

        private static void OnCloseButtonToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion CloseButtonToolTip

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        static WindowButtons()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowButtons), new FrameworkPropertyMetadata(typeof(WindowButtons)));
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _minButton = Template.FindName("PART_Min", this) as Button;
            if (_minButton != null)
            {
                _minButton.Click += (sender, e) =>
                {
                    if (ParentWindow != null) ParentWindow.WindowState = WindowState.Minimized;
                };
            }

            _maxRestoreButton = Template.FindName("PART_Max", this) as Button;
            if (_maxRestoreButton != null)
            {
                _maxRestoreButton.Click += (sender, e) =>
                {
                    if (ParentWindow is {WindowState: WindowState.Normal})
                    {
                        _maxRestoreButton.Style = RestoreButtonStyle;
                        ParentWindow.WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        _maxRestoreButton.Style = MaxButtonStyle;
                        if (ParentWindow != null) ParentWindow.WindowState = WindowState.Normal;
                    }
                };
            }

            _closeButton = Template.FindName("PART_Close", this) as Button;
            if (_closeButton != null)
            {
                _closeButton.Click += (sender, e) =>
                {
                    ParentWindow?.Close();
                };
            }
        }

        #endregion Methods
    }
}
