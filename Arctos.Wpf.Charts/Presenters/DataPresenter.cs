using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class DataPresenter : Presenter, IDataPresenter, ICanvasHost
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Axis? XAxis { get; set; }
        public Axis? YAxis { get; set; }
        public Axis? PrimaryAxis { get; set; }
        public List<Axis> Axes => PresenterHost?.Axes.ToList() ?? new List<Axis>();

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public DataPresenter()
        {
            Canvases = new ObservableCollection<ChartCanvas>();
        }

        #endregion Constructor

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependancy Properties

        #region Title
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        (
          "Title",
          typeof(object),
          typeof(DataPresenter),
          new FrameworkPropertyMetadata(null, OnTitleChanged)
        );

        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion

        #region DataItemsSource
        public static readonly DependencyProperty DataItemsSourceProperty = DependencyProperty.Register
          (
            "DataItemsSource",
            typeof(IEnumerable),
            typeof(DataPresenter),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange, OnDataItemsSourceChanged)
          );

        public IEnumerable DataItemsSource
        {
            set => SetValue(DataItemsSourceProperty, value);
            get => (IEnumerable)GetValue(DataItemsSourceProperty);
        }

        private static void OnDataItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataPresenter presenter || e.NewValue == null)
                return;
            if (presenter.DataItemsProvider != null)
                throw new InvalidOperationException("Cannot use 'DataItemsProvider' and 'DataItemsSource' together.");
            if (e.NewValue is not INotifyCollectionChanged)
                throw new InvalidOperationException("Collection must implement 'INotifyCollectionChanged'");

            var newCollection = e.NewValue as IEnumerable;
            var oldCollection = e.OldValue as IEnumerable;
            presenter.SetDataItemsSource(newCollection, oldCollection);
        }
        #endregion

        #region DataItemsProvider
        public static readonly DependencyProperty DataItemsProviderProperty = DependencyProperty.Register
        (
          "DataItemsProvider",
          typeof(IDataProvider),
          typeof(DataPresenter),
          new PropertyMetadata(null, OnDataItemsProviderChanged)
        );

        public IDataProvider DataItemsProvider
        {
            get => (IDataProvider)GetValue(DataItemsProviderProperty);
            set => SetValue(DataItemsProviderProperty, value);
        }

        private static void OnDataItemsProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataPresenter presenter)
                return;
            //if (e.NewValue == null)
            //  return;
            if (presenter.DataItemsSource != null)
                throw new InvalidOperationException("Cannot use 'DataItemsProvider' and 'DataItemsSource' together.");
            if(e.NewValue is IDataProvider dataProvider)
                presenter.SetDataProvider(dataProvider);
        }
        #endregion

        #region Canvases
        public static readonly DependencyProperty CanvasesProperty = DependencyProperty.Register
        (
          "Canvases",
          typeof(ObservableCollection<ChartCanvas>),
          typeof(DataPresenter),
          new PropertyMetadata(default(ObservableCollection<ChartCanvas>), OnCanvasesChanged)
        );

        public ObservableCollection<ChartCanvas> Canvases
        {
            get => (ObservableCollection<ChartCanvas>)GetValue(CanvasesProperty);
            set => SetValue(CanvasesProperty, value);
        }

        private static void OnCanvasesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is not DataPresenter dp)
                return;
            var newCanvases = (ObservableCollection<ChartCanvas>)e.NewValue;
            var oldCanvases = (ObservableCollection<ChartCanvas>)e.OldValue;
            dp.CanvasesChanged(newCanvases, oldCanvases);
        }
        #endregion

        #region XAxisName
        public static readonly DependencyProperty XAxisNameProperty = DependencyProperty.Register
        (
          nameof(XAxisName),
          typeof(string),
          typeof(DataPresenter),
          new PropertyMetadata(null, OnXAxisNameChanged)
        );

        public string XAxisName
        {
            get => (string)GetValue(XAxisNameProperty);
            set => SetValue(XAxisNameProperty, value);
        }

        private static void OnXAxisNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataPresenter presenter)
                return;
            presenter.XAxis ??= presenter.PresenterHost?.Axes.FirstOrDefault(a => a.Name == presenter.XAxisName);
            //if (presenter.XAxis?.GetType() == typeof(DiscreteAxis))
            //{
            //    if(presenter.DataItemsProvider.GetType() == typeof(DiscreteDataProvider<>))
            //}
        }
        #endregion

        #region YAxisName
        public static readonly DependencyProperty YAxisNameProperty = DependencyProperty.Register
        (
          nameof(YAxisName),
          typeof(string),
          typeof(DataPresenter),
          new PropertyMetadata(null, OnYAxisNameChanged)
        );

        public string YAxisName
        {
            get => (string)GetValue(YAxisNameProperty);
            set => SetValue(YAxisNameProperty, value);
        }

        private static void OnYAxisNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataPresenter presenter)
                return;
            presenter.YAxis ??= presenter.PresenterHost?.Axes.FirstOrDefault(a => a.Name == presenter.YAxisName);
        }
        #endregion

        #endregion Dependancy Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public override void SetFocalData(object? data)
        {
            if(data is null)
                return;
            RaiseFocalDataChanged(new FocalDataEventArgs(data));
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected virtual void SetAxes()
        {
            if (Axes.Count == 0)
                return;

            //Ensure there is 1 and only 1 PrimaryAxis
            var primaries = Axes.Where(a => a.Mode == AxisMode.Primary).ToList();
            if (primaries.Count > 1)
                throw new ArgumentException("A chart can only have one Primary Axis");
            if (primaries.Count == 0)
                throw new ArgumentException("A chart requires a Primary Axis");
            PrimaryAxis = primaries[0];

            //Set the PrimaryAxis first as it may be need by other axes
            PrimaryAxis.AxisChanged += OnAxisMoved;
            var min = PrimaryAxis.Minimum;
            var max = PrimaryAxis.Maximum;

            if (PrimaryAxis is NumericAxis {ShowZero: true} numericAxis)
            {
                if (numericAxis.DataRange != null) numericAxis.DataRange.Minimum = min as double? <= 0.0 ? min : 0.0;
            }
            else if (PrimaryAxis.DataRange != null) PrimaryAxis.DataRange.Minimum = min;

            if (PrimaryAxis.DataRange != null)
            {
                PrimaryAxis.DataRange.Maximum = max;
                if (!PrimaryAxis.Scrollable && !PrimaryAxis.Zoomable && PrimaryAxis.VisualRange != null)
                {
                    PrimaryAxis.VisualRange.Minimum = min;
                    PrimaryAxis.VisualRange.Maximum = max;
                }

                PrimaryAxis.DataPresenter = this;

                //Set all other axes
                foreach (var axis in Axes)
                {
                    //PrimaryAxis is already set
                    if (axis.Mode == AxisMode.Primary)
                        continue;

                    axis.AxisChanged += OnAxisMoved;
                    axis.DataPresenter = this;

                    switch (axis.Scaling)
                    {
                        case AxisScaling.None:
                            min = axis.Minimum;
                            max = axis.Maximum;
                            if (axis is NumericAxis { ShowZero: true } nAxis)
                            {
                                if (nAxis.DataRange != null) nAxis.DataRange.Minimum = min as double? <= 0.0 ? min : 0.0;
                            }
                            else if (axis.DataRange != null) axis.DataRange.Minimum = min;

                            if (axis.DataRange != null) axis.DataRange.Maximum = max;
                            if (axis.Scrollable || axis.Zoomable)
                                continue;
                            break;
                        case AxisScaling.Visual:
                            if (PrimaryAxis.VisualRange != null)
                            {
                                DataProvider?.GetRangeMinMax(PrimaryAxis.Source, PrimaryAxis.VisualRange, axis.Source,
                                    out min, out max);
                                if (Math.Abs(axis.ScalingMargin) > 0.0001)
                                    axis.VisualRange?.Zoom(-axis.ScalingMargin);
                            }
                            return;
                        case AxisScaling.Predictive:
                            break;
                        case AxisScaling.All:
                            DataProvider?.GetMinMax(axis.Source, out min, out max);
                            if (Math.Abs(axis.ScalingMargin) > 0.0001)
                                axis.VisualRange?.Zoom(-axis.ScalingMargin);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (axis is not DiscreteAxis && axis.VisualRange != null)
                    {
                        axis.VisualRange.Minimum = min;
                        axis.VisualRange.Maximum = max;
                    }
                }
            }
        }

        protected virtual void SetMarkers()
        {
            if (PresenterHost != null)
                foreach (var marker in PresenterHost.Markers)
                    marker.DataPresenter = this;
        }

        protected void SetDataItemsSource(IEnumerable? newCollection, IEnumerable? oldCollection)
        {
            if (DataProvider is not CollectionDataProvider provider)
            {
                provider = new CollectionDataProvider();
                SetDataProvider(provider);
            }
            provider.UnHook(oldCollection as INotifyCollectionChanged);
            provider.Hook(newCollection as INotifyCollectionChanged);
        }

        protected virtual void SetDataProvider(IDataProvider dataProvider)
        {
            DataProvider = dataProvider;
            DataProvider.DataChanged += OnProviderDataChanged;
            SetAxes();
            SetMarkers();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in Children)
            {
                child.InvalidateMeasure();
                child.Measure(availableSize);
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);
            foreach (UIElement child in Children)
            {
                child.InvalidateArrange();
                child.Arrange(new Rect(new Point(0, 0), finalSize));
            }

            return finalSize;
        }

        #endregion Protected Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private void CanvasesChanged(ObservableCollection<ChartCanvas>? newCanvases, ObservableCollection<ChartCanvas>? oldCanvases)
        {
            if (oldCanvases != null)
            {
                foreach (var canvas in oldCanvases)
                {
                    canvas.CanvasHost = null;
                    Children.Remove(canvas);
                }
                oldCanvases.CollectionChanged -= OnCanvasesChanged;
            }

            if (newCanvases != null)
            {
                foreach (var canvas in newCanvases)
                {
                    canvas.CanvasHost = this;
                    Children.Add(canvas);
                }
                newCanvases.CollectionChanged += OnCanvasesChanged;
            }
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnAxisMoved(object? sender, EventArgs e)
        {
            if(sender is not Axis axis)
                return;
            if (axis.Mode == AxisMode.Primary)
            {
                foreach (var cax in Axes)
                {
                    if (cax.Mode != AxisMode.Auxiliary)
                        continue;

                    object? min = null;
                    object? max = null;
                    if (cax.Scaling == AxisScaling.Visual)
                        if (axis.VisualRange != null)
                            DataProvider?.GetRangeMinMax(axis.Source, axis.VisualRange, cax.Source, out min, out max);
                    if (min == null || max == null) continue;
                    if (cax.VisualRange != null)
                    {
                        cax.VisualRange.Minimum = min;
                        cax.VisualRange.Maximum = max;
                        if (Math.Abs(cax.ScalingMargin) > 0.0001)
                            cax.VisualRange.Zoom(-cax.ScalingMargin);
                    }

                    cax.InvalidateAxis();
                }
            }
            else
            {
                if (axis.Scaling != AxisScaling.None)
                    return;
            }
            InvalidateMeasure();
            foreach (UIElement child in Children)
                child.InvalidateMeasure();
        }

        private void OnCanvasesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ChartCanvas canvas in e.OldItems)
                {
                    canvas.CanvasHost = null;
                    Children.Remove(canvas);
                }
            }

            if (e.NewItems != null)
            {
                foreach (ChartCanvas canvas in e.NewItems)
                {
                    canvas.CanvasHost = this;
                    Children.Add(canvas);
                }
            }
        }

        public void OnFocalDataChanged(object? sender, FocalDataEventArgs args)
        {
            RaiseFocalDataChanged(args);
        }

        private void OnProviderDataChanged(object? sender, EventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var primaryAxis = Axes.FirstOrDefault(a => a.Mode == AxisMode.Primary);
                if (primaryAxis != null)
                {
                    foreach (var cax in Axes)
                    {
                        object? min = null;
                        object? max = null;
                        if (cax.Scaling == AxisScaling.Visual)
                            if (primaryAxis.VisualRange != null)
                                DataProvider?.GetRangeMinMax(primaryAxis.Source, primaryAxis.VisualRange, cax.Source,
                                    out min, out max);
                        if (min == null || max == null) continue;
                        if (cax.VisualRange != null)
                        {
                            cax.VisualRange.Minimum = min;
                            cax.VisualRange.Maximum = max;
                            if (Math.Abs(cax.ScalingMargin) > 0.0001)
                                cax.VisualRange.Zoom(-cax.ScalingMargin);
                        }

                        cax.InvalidateAxis();
                    }
                }

                PresenterHost?.Refresh();
            });
        }

        #endregion Event Handlers
    }
}
