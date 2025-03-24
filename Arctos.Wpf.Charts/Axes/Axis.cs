using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Arctos.Wpf.Charts
{
    public class Axis : Grid, IAxis, IDataPresenterHelper
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private bool _beginScroll;
        private Point _scrollOrigin;
        private InputDevice? _capturedDevice;
        private Point _lastScrollPosition;

        private RenderListener? _renderListener;
        private bool _isAnimating;
        private double _currentLocation;
        private double _targetLocation;
        private double _currentVelocity;
        private const double MinLocationDelta = 0.1;
        private const double MinVelocityDelta = 0.1;

        protected double MaxTickSetWidth;
        protected double MaxTickSetHeight;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public virtual IDataPresenter? DataPresenter { get; set; }
        public IVisualRange? VisualRange { get; set; }
        public IRange? DataRange { get; set; }

        #region Orientation
        private PartOrientation _orientation;
        public virtual PartOrientation Orientation
        {
            get
            {
                if (_orientation == PartOrientation.None)
                {
                    var vPos = ChartPanel.GetVerticalPosition(this);
                    var hPos = ChartPanel.GetHorizontalPosition(this);
                    if ((hPos == HorizontalPosition.Left || hPos == HorizontalPosition.Right) && vPos == VerticalPosition.Center)
                        _orientation = PartOrientation.Vertical;
                    else if ((vPos == VerticalPosition.Top || vPos == VerticalPosition.Bottom) && hPos == HorizontalPosition.Center)
                        _orientation = PartOrientation.Horizontal;
                    SetOrientation(_orientation);
                }
                return _orientation;
            }
        }
        #endregion

        public bool IsShared { get; set; }

        #region Scrollable
        private bool _scrollable;
        public bool Scrollable
        {
            get => _scrollable;
            set
            {
                if (_scrollable == value) return;
                _scrollable = value;
                if (_scrollable)
                {
                    MouseLeave += OnMouseLeave;
                    AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnLeftButtonDown), true);
                    AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnLeftButtonUp), true);
                    MouseMove += OnMouseMove;
                }
                else
                {
                    MouseLeave -= OnMouseLeave;
                    RemoveHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnLeftButtonDown));
                    RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnLeftButtonUp));
                    MouseMove -= OnMouseMove;
                }
            }
        }
        #endregion

        #region Zoomable
        private bool _zoomable;

        public bool Zoomable
        {
            get => _zoomable;
            set
            {
                if (_zoomable == value) return;
                _zoomable = value;
                if (_zoomable)
                    AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheel), true);
                else
                    RemoveHandler(MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheel));
            }
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        static Axis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Axis), new FrameworkPropertyMetadata(typeof(Axis)));
        }

        public Axis()
        {
            TickSets = new TickSetCollection();
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region Title
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
          "Title",
          typeof(string),
          typeof(Axis),
          new PropertyMetadata(null)
        );

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        #endregion

        #region TickSet
        public static readonly DependencyProperty TickSetsProperty = DependencyProperty.Register
        (
          "TickSets",
          typeof(ObservableCollection<TickSet>),
          typeof(Axis),
          new PropertyMetadata(null, OnTickSetChanged)
        );

        public ObservableCollection<TickSet> TickSets
        {
            get => (ObservableCollection<TickSet>)GetValue(TickSetsProperty);
            set => SetValue(TickSetsProperty, value);
        }

        private static void OnTickSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Source
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register
        (
          nameof(Source),
          typeof(string),
          typeof(Axis),
          new PropertyMetadata(null, OnSourceChanged)
        );

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Mode
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register
        (
          "Mode",
          typeof(AxisMode),
          typeof(Axis),
          new PropertyMetadata(AxisMode.Auxiliary, OnModeChanged)
        );

        public AxisMode Mode
        {
            get => (AxisMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;
            axis.SetMode((AxisMode)e.NewValue);
        }
        #endregion Mode

        #region IsScrollable
        public static readonly DependencyProperty IsScrollableProperty = DependencyProperty.Register
        (
          "IsScrollable",
          typeof(bool),
          typeof(Axis),
          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsScrollableChanged)
        );

        public bool IsScrollable
        {
            get => (bool)GetValue(IsScrollableProperty);
            set => SetValue(IsScrollableProperty, value);
        }

        private static void OnIsScrollableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;
            if (axis.Scaling != AxisScaling.None)
            {
                Debug.WriteLine("IsScrollable cannot be used in conjunction with AxisScaling. IsScrollable will be ignored.");
                return;
            }
            axis.Scrollable = (bool)e.NewValue;
        }
        #endregion

        #region IsZoomable
        public static readonly DependencyProperty IsZoomableProperty = DependencyProperty.Register
        (
          "IsZoomable",
          typeof(bool),
          typeof(Axis),
          new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsZoomableChanged)
        );

        public bool IsZoomable
        {
            get => (bool)GetValue(IsZoomableProperty);
            set => SetValue(IsZoomableProperty, value);
        }

        private static void OnIsZoomableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;
            if (axis.Scaling != AxisScaling.None)
            {
                Debug.WriteLine("IsZoomable cannot be used in conjunction with AxisScaling. IsZoomable will be ignored.");
                return;
            }
            axis.Zoomable = (bool)e.NewValue;
        }
        #endregion

        #region Scaling
        public static readonly DependencyProperty ScalingProperty = DependencyProperty.Register
        (
          "Scaling",
          typeof(AxisScaling),
          typeof(Axis),
          new FrameworkPropertyMetadata(AxisScaling.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnScalingChanged, CoerceScaling)
        );

        public AxisScaling Scaling
        {
            get => (AxisScaling)GetValue(ScalingProperty);
            set => SetValue(ScalingProperty, value);
        }

        private static void OnScalingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;
            if (axis.Zoomable)
            {
                Debug.WriteLine("IsZoomable cannot be used in conjunction with AxisScaling. IsZoomable will be ignored.");
                axis.IsZoomable = false;
            }
            if (axis.Scrollable)
            {
                Debug.WriteLine("IsScrollable cannot be used in conjunction with AxisScaling. IsScrollable will be ignored.");
                axis.IsScrollable = false;
            }
        }

        private static object CoerceScaling(DependencyObject d, object value)
        {
            if (d is not Axis axis)
                return value;
            if (axis.Mode == AxisMode.Primary && (AxisScaling)value != AxisScaling.None)
            {
                Debug.WriteLine("Scaling cannot be applied to the Primary Axis. Scaling will be set to None.");
                return AxisScaling.None;
            }
            return value;
        }

        #endregion Scaling  

        #region ScalingMargin
        public static readonly DependencyProperty ScalingMarginProperty = DependencyProperty.Register
        (
          "ScalingMargin",
          typeof(double),
          typeof(Axis),
          new PropertyMetadata(0.0, OnScalingMarginChanged, CoerceScalingMargin)
        );

        public double ScalingMargin
        {
            get => (double)GetValue(ScalingMarginProperty);
            set => SetValue(ScalingMarginProperty, value);
        }

        private static void OnScalingMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static object CoerceScalingMargin(DependencyObject d, object value)
        {
            var current = (double)value;
            if (current < 0) current = 0;
            if (current > 100) current = 100;
            return current;
        }
        #endregion ScalingMargin

        #region Minimum
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register
        (
          "Minimum",
          typeof(object),
          typeof(Axis),
          new FrameworkPropertyMetadata
          (
            null,
            FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnMinimumChanged
          )
        );

        public object? Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;
            if (e.NewValue != null)
            {
                if (e.NewValue.GetType() != axis.DataRange?.GetType().GetGenericArguments()[0])
                    return;
                if (axis.VisualRange != null)
                {
                    axis.VisualRange.Minimum = e.NewValue;
                    axis.CurrentValue = axis.VisualRange.FractionalToValue(axis.CurrentValueOffset);
                }
            }
            ////      axis.InvalidateAxis();
            //axis.Parent.InvalidateMeasure();
        }
        #endregion

        #region Maximum
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register
        (
          "Maximum",
          typeof(object),
          typeof(Axis),
          new FrameworkPropertyMetadata
          (
            null,
            FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnMaximumChanged
          )
        );

        public object? Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;
            if (e.NewValue != null)
            {
                if (e.NewValue.GetType() != axis.DataRange?.GetType().GetGenericArguments()[0])
                    return;
                if (axis.VisualRange != null)
                {
                    axis.VisualRange.Maximum = e.NewValue;
                    axis.CurrentValue = axis.VisualRange.FractionalToValue(axis.CurrentValueOffset);
                }
            }
            //axis.InvalidateAxis();
            //axis.InvalidateMeasure();
        }
        #endregion

        #region CurrentValue

        public static readonly DependencyProperty CurrentValueProperty = DependencyProperty.Register
        (
          "CurrentValue",
          typeof(object),
          typeof(Axis),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentValueChanged)
        );

        public object? CurrentValue
        {
            get => GetValue(CurrentValueProperty);
            set => SetValue(CurrentValueProperty, value);
        }

        private static void OnCurrentValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion CurrentValue

        #region CurrentValueOffset

        public static readonly DependencyProperty CurrentValueOffsetProperty = DependencyProperty.Register
        (
          "CurrentValueOffset",
          typeof(double),
          typeof(Axis),
          new PropertyMetadata(0.5, OnCurrentValueOffsetChanged)
        );

        public double CurrentValueOffset
        {
            get => (double)GetValue(CurrentValueOffsetProperty);
            set => SetValue(CurrentValueOffsetProperty, value);
        }

        private static void OnCurrentValueOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;

            if (!axis.IsScrollable || axis.Orientation == PartOrientation.None)
                return;

            if (axis.VisualRange != null)
            {
                var from = axis.VisualRange.FractionalToVisual((double)e.OldValue);
                var to = axis.VisualRange.FractionalToVisual((double)e.NewValue);
                if (axis.Orientation == PartOrientation.Horizontal)
                    axis.VisualRange.Move(from - to);
                else
                    axis.VisualRange.Move(to - from);
            }

            axis.InvalidateAxis();
        }

        #endregion CurrentValueOffset

        #region DataAlignment
        public static readonly DependencyProperty DataAlignmentProperty = DependencyProperty.Register
        (
          "DataAlignment",
          typeof(DataAlignment),
          typeof(Axis),
          new FrameworkPropertyMetadata(DataAlignment.None, OnDataAlignmentChanged, CoerceDataAlignment)
        );

        public DataAlignment DataAlignment
        {
            get => (DataAlignment)GetValue(DataAlignmentProperty);
            set => SetValue(DataAlignmentProperty, value);
        }

        private static void OnDataAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Axis axis)
                return;

            if ((DataAlignment)e.NewValue == DataAlignment.Animate)
            {
                axis._renderListener = new RenderListener();
                axis._renderListener.Rendering += axis.OnRender;
            }
            else if ((DataAlignment)e.OldValue == DataAlignment.Animate)
            {
                axis._renderListener?.Dispose();
                axis._renderListener = null;
            }
        }

        private static object CoerceDataAlignment(DependencyObject d, object value)
        {
            if (d is not Axis axis)
                return value;

            if (axis.Mode == AxisMode.Auxiliary && (DataAlignment)value != DataAlignment.None)
            {
                Debug.WriteLine("Data alignment can only be applied to the Primary Axis. DataAlignment will be set to None.");
                return DataAlignment.None;
            }
            return value;
        }
        #endregion DataAlignment

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void InvalidateAxis()
        {
            CurrentValue = VisualRange?.FractionalToValue(CurrentValueOffset) ?? 0;
            foreach (UIElement child in Children)
                child.InvalidateMeasure();
            InvalidateMeasure();
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected virtual void SetMode(AxisMode mode)
        {
            if (mode == AxisMode.Primary && Scaling != AxisScaling.None)
            {
                Debug.WriteLine("Scaling cannot be applied to the Primary Axis. Scaling will be set to None.");
                Scaling = AxisScaling.None;
            }
            if (mode == AxisMode.Auxiliary && DataAlignment != DataAlignment.None)
            {
                Debug.WriteLine("Data alignment can only be applied to the Primary Axis. DataAlignment will be set to None.");
                DataAlignment = DataAlignment.None;
            }
        }

        protected virtual void SetOrientation(PartOrientation orientation)
        {
            if (VisualRange != null) VisualRange.Orientation = orientation;
        }

        protected void ScaleRange(Size availableSize)
        {
            double length;
            if (Orientation == PartOrientation.Horizontal)
                length = availableSize.Width;
            else if (Orientation == PartOrientation.Vertical)
                length = availableSize.Height;
            else
                throw new InvalidOperationException("Axis orientation cannot be 'None'");
            VisualRange?.SetVisualScale(length);
        }

        protected virtual void AutoScaleTickSet(double length, TickSet hatching)
        {
            throw new NotImplementedException();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //if (double.IsPositiveInfinity(availableSize.Width) || double.IsPositiveInfinity(availableSize.Height))
            //  throw new ArgumentException("Chart cannot be contained in a control that provides PositiveInfinity as available space. eg. Scroller, StackPane, Grid cell with Width or Height set to Auto.");
            var maxWidth = 0.0;
            var maxHeight = 0.0;
            ScaleRange(availableSize);
            var labels = new List<TickLabel>();
            var icons = new List<TickIcon>();

            Children.Clear();
            foreach (var tickSet in TickSets)
            {
                if (tickSet.IsAuto)
                    AutoScaleTickSet(Orientation == PartOrientation.Horizontal ? availableSize.Width : availableSize.Height, tickSet);
                Children.Add(tickSet);
                tickSet.Measure(availableSize);
                if (tickSet.IsLabeled)
                    SetTickLabels(tickSet, labels);

                if (tickSet.HasIcons)
                {
                    tickSet.Ticks.ForEach(h =>
                    {
                        if (icons.Any(i => i.Center == h.Center))
                            return;
                        var icon = (TickIcon?)tickSet.IconConverter.Convert(h.Value, typeof(TickIcon), null, null);
                        if (icon != null)
                        {
                            icon.Center = h.Center;
                            icons.Add(icon);
                        }
                    });
                }

                if (tickSet.DesiredSize.Width > maxWidth) maxWidth = tickSet.DesiredSize.Width;
                if (tickSet.DesiredSize.Height > maxHeight) maxHeight = tickSet.DesiredSize.Height;
            }
            MaxTickSetWidth = maxWidth;
            MaxTickSetHeight = maxHeight;

            if (labels.Count > 0)
            {
                var tickSetLabels = new TickDecorators<TickLabel>(labels, this);
                tickSetLabels.Measure(availableSize);
                Children.Add(tickSetLabels);
                if (Orientation == PartOrientation.Vertical)
                    maxWidth += tickSetLabels.DesiredSize.Width;
                else
                    maxHeight += tickSetLabels.DesiredSize.Height;
            }

            if (icons.Count > 0)
            {
                var tickSetIcons = new TickDecorators<TickIcon>(icons, this);
                tickSetIcons.Measure(availableSize);
                Children.Add(tickSetIcons);
                if (Orientation == PartOrientation.Vertical)
                    maxWidth += tickSetIcons.DesiredSize.Width;
                else
                    maxHeight += tickSetIcons.DesiredSize.Height;
            }

            if (Title != null)
            {
                var title = new TextBlock { Text = Title, TextAlignment = TextAlignment.Center };
                if (Orientation == PartOrientation.Vertical)
                    title.VerticalAlignment = VerticalAlignment.Stretch;
                else
                    title.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            RaiseAxisChanged();

            return new Size(maxWidth, maxHeight);
        }

        protected override Size ArrangeOverride(Size totalSize)
        {
            foreach (UIElement child in Children)
            {
                var point = new Point(0, 0);
                if (child is TickSet)
                {
                    if (ChartPanel.GetHorizontalPosition(this) == HorizontalPosition.Left)
                        point.X = DesiredSize.Width - child.DesiredSize.Width;
                    else if (ChartPanel.GetVerticalPosition(this) == VerticalPosition.Top)
                        point.Y = DesiredSize.Height - child.DesiredSize.Height;
                }
                else if (child is TickDecorators<TickLabel>)
                {
                    if (ChartPanel.GetHorizontalPosition(this) == HorizontalPosition.Left)
                        point.X = DesiredSize.Width - child.DesiredSize.Width - MaxTickSetWidth;
                    else if (ChartPanel.GetVerticalPosition(this) == VerticalPosition.Bottom)
                        point.Y = DesiredSize.Height - child.DesiredSize.Height;
                }
                child.Arrange(new Rect(point, totalSize));
            }

            return totalSize;
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            if (e.Device == _capturedDevice)
                _capturedDevice = null;
        }

        protected virtual void SetTickLabels(TickSet tickSet, List<TickLabel> labels)
        {
            tickSet.Ticks.ForEach(tick =>
            {
                if (labels.Any(l => l.Center == tick.Center))
                    return;
                labels.Add(new TickLabel
                {
                    Text = VisualRange?.ParseValue(tick.Value, tickSet.CurrentLabelFormat),
                    Center = tick.Center,
                    Foreground = tickSet.LabelForeground,
                    FontSize = tickSet.LabelFontSize,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(4, 1, 4, 2),
                });
            });

        }

        #endregion Protected Methods

        //---------------------------------------------------------------------
        #region Private Methods


        private void BeginScrollOperation(InputDevice deviceToCapture, Point initialScrollPosition)
        {
            if (_capturedDevice != null)
                return;
            if (deviceToCapture is not MouseDevice mouseDevice)
                return;

            _lastScrollPosition = initialScrollPosition;
            mouseDevice.Captured?.ReleaseMouseCapture();   //<---WTF
            if (mouseDevice.Captured != null || mouseDevice.LeftButton != MouseButtonState.Pressed)
                return;
            _capturedDevice = mouseDevice;
            mouseDevice.Capture(this);
        }

        private void UpdateScrollPosition(Point currentScrollPosition)
        {
            var distance = Orientation == PartOrientation.Horizontal
              ? _lastScrollPosition.X - currentScrollPosition.X
              : _lastScrollPosition.Y - currentScrollPosition.Y;
            _lastScrollPosition = currentScrollPosition;
            if (VisualRange != null)
            {
                VisualRange.Move(distance);
                Minimum = VisualRange.Minimum;
                Maximum = VisualRange.Maximum;
            }

            InvalidateAxis();
        }

        private void Zoom(double delta)
        {
            if (_isAnimating)
                StopAnimation();
            if (VisualRange != null)
            {
                VisualRange.Zoom(delta / 12.0);
                ScaleRange(DesiredSize);
                Minimum = VisualRange.Minimum;
                Maximum = VisualRange.Maximum;
            }

            InvalidateAxis();
            AlignWithNearestData(CurrentValue); //Todo: bug here
                                                //RaiseAxisChanged();
        }

        private void AlignWithNearestData(object? targetValue)
        {
            if (targetValue == null || DataAlignment == DataAlignment.None || DataPresenter == null || VisualRange == null)
                return;

            var before = DataPresenter.DataProvider?.LastAtOrBefore(targetValue);
            var after = DataPresenter.DataProvider?.FirstAtOrAfter(targetValue);

            object? closest;
            if (before == null && after == null)
                return;
            if (before == null)
                closest = after;
            else if (after == null)
                closest = before;
            else
                closest = VisualRange.Closest(targetValue, before, after) <= 0 ? before : after;
            if(closest is not null)
                DataPresenter.SetFocalData(DataPresenter.DataProvider?.GetDataObject(Source, closest));

            var from = VisualRange.ValueToVisual(CurrentValue);
            var to = VisualRange.ValueToVisual(closest);
            if (DataAlignment == DataAlignment.Snap)
            {
                VisualRange.Move(to - from);
                InvalidateAxis();
            }
            else
            {
                _currentLocation = from;
                _targetLocation = to;
                _renderListener?.StartListening();
            }
        }

        private bool UpdateAnimation(out double newLocation)
        {
            const double dampening = 0.1;
            const double attractionFactor = 0.2;
            const double terminalVelocity = 1000;

            var diff = _targetLocation - _currentLocation;

            if (Math.Abs(diff) > MinLocationDelta || Math.Abs(_currentVelocity) > MinVelocityDelta)
            {
                var newVelocity = _currentVelocity * (1 - dampening);
                newVelocity += diff * attractionFactor;
                if (Math.Abs(_currentVelocity) > terminalVelocity)
                {
                    newVelocity *= terminalVelocity / Math.Abs(_currentVelocity);
                }
                newLocation = _currentLocation + newVelocity;
                return true;
            }

            newLocation = _targetLocation;
            _currentVelocity = 0;
            return false;
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private static void OnLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Axis axis)
                return;

            if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(axis);
                object? target = null;
                switch (axis.Orientation)
                {
                    case PartOrientation.None:
                        return;
                    case PartOrientation.Vertical:
                        target = axis.VisualRange?.VisualToValue(mousePosition.Y);
                        break;
                    case PartOrientation.Horizontal:
                        target = axis.VisualRange?.VisualToValue(mousePosition.X);
                        break;
                }
                if(target != null)
                    axis.AlignWithNearestData(target);
                e.Handled = true;
            }
            else
            {
                if (axis._isAnimating)
                    axis.StopAnimation();
                axis._beginScroll = true;
                axis._scrollOrigin = e.GetPosition(axis);
                axis.CaptureMouse();
            }

        }

        private static void OnLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Axis axis)
                return;
            axis._beginScroll = false;
            if (Mouse.Captured != null && Mouse.Captured.Equals(axis))
            {
                axis.ReleaseMouseCapture();
                axis._capturedDevice = null;
            }
            axis.AlignWithNearestData(axis.CurrentValue);
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is not Axis axis)
                return;
            axis._beginScroll = false;
            if (Mouse.Captured != null && Mouse.Captured.Equals(axis))
                axis.ReleaseMouseCapture();
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not Axis axis)
                return;
            if (axis._beginScroll)
            {
                axis._beginScroll = false;
                e.Handled = true;
                if (Mouse.Captured != null && Mouse.Captured.Equals(axis))
                    axis.ReleaseMouseCapture();
                axis.BeginScrollOperation(e.Device, axis._scrollOrigin);
                axis.UpdateScrollPosition(e.GetPosition(axis));
            }
            else if (e.Device == axis._capturedDevice)
            {
                if (axis._capturedDevice is MouseDevice mouseDevice)
                    axis.UpdateScrollPosition(mouseDevice.GetPosition(axis));
                e.Handled = true;
            }
        }

        private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var axis = sender as Axis;
            axis?.Zoom(e.Delta);
        }

        private void OnRender(object? sender, EventArgs e)
        {
            _isAnimating = UpdateAnimation(out var newLocation);
            {
                VisualRange?.Move(newLocation - _currentLocation);
                _currentLocation = newLocation;
                InvalidateAxis();
            }
            if (!_isAnimating)
                _renderListener?.StopListening();
        }

        private void StopAnimation()
        {
            _renderListener?.StopListening();
            _isAnimating = false;
        }


        #endregion Event Handlers

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        public event EventHandler<EventArgs>? AxisChanged;
        public void RaiseAxisChanged()
        {
            AxisChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Events
    }
}
