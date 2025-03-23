using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Arctos.Wpf.Charts
{
    public abstract class ChartCanvas : Grid, ICanvas
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ICanvasHost? CanvasHost { get; set; }

        #endregion Properties

        #region MonitorCursor

        public static readonly DependencyProperty MonitorCursorProperty = DependencyProperty.Register(
            "MonitorCursor", typeof(bool), 
            typeof(ChartCanvas),
            new FrameworkPropertyMetadata
            (
                false,
                FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnMonitorCursorChanged
            )
        );

        public bool MonitorCursor
        {
            get => (bool) GetValue(MonitorCursorProperty);
            set => SetValue(MonitorCursorProperty, value);
        }

        private static void OnMonitorCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ChartCanvas canvas)
                return;
            if (e.NewValue is not bool monitor)
                return;
            if (monitor)
                canvas.MouseMove += OnMouseMove;
            else
                canvas.MouseMove -= OnMouseMove;
        }

        #endregion

        #region CursurPosition

        public static readonly DependencyProperty CursorPositionProperty = DependencyProperty.Register(
            "CursorPositionProperty", typeof(Point), 
            typeof(ChartCanvas), 
            new PropertyMetadata(default(Point)));

        public Point CursorPosition
        {
            get => (Point) GetValue(CursorPositionProperty);
            set => SetValue(CursorPositionProperty, value);
        }
        #endregion

        #region SnapCursorX
        public static readonly DependencyProperty SnapCursorXProperty = DependencyProperty.Register(
            "SnapCursorX", typeof(object), typeof(ChartCanvas), new PropertyMetadata(default(object?)));

        public object? SnapCursorX
        {
            get => (object?) GetValue(SnapCursorXProperty);
            set => SetValue(SnapCursorXProperty, value);
        }
        #endregion

        #region SnapCursorY
        public static readonly DependencyProperty SnapCursorYProperty = DependencyProperty.Register(
            "SnapCursorY", typeof(object), typeof(ChartCanvas), new PropertyMetadata(default(object?)));

        public object? SnapCursorY
        {
            get => (object?)GetValue(SnapCursorYProperty);
            set => SetValue(SnapCursorYProperty, value);
        }
        #endregion

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not ChartCanvas canvas)
                return;
            canvas.CursorPosition = e.GetPosition(canvas);
        }

    }
}