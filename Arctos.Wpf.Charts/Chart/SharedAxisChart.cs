using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Arctos.Wpf.Charts
{
    public class SharedAxisChart : Grid
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        static SharedAxisChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SharedAxisChart), new FrameworkPropertyMetadata(typeof(SharedAxisChart)));
        }

        public SharedAxisChart()
        {
            Charts = new ObservableCollection<ChartPanel>();
            SharedAxis = new Axis() { IsShared = true };
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region dependency Properties

        #region Charts
        public static readonly DependencyProperty ChartsProperty = DependencyProperty.Register
        (
          "Charts",
          typeof(ObservableCollection<ChartPanel>),
          typeof(SharedAxisChart),
          new PropertyMetadata(default(ObservableCollection<ChartPanel>), OnChartsChanged)
        );

        public ObservableCollection<ChartPanel> Charts
        {
            get => (ObservableCollection<ChartPanel>)GetValue(ChartsProperty);
            set => SetValue(ChartsProperty, value);
        }

        private static void OnChartsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SharedAxisChart sac)
                return;
            var newCollection = (ObservableCollection<ChartPanel>)e.NewValue;
            var oldCollection = (ObservableCollection<ChartPanel>)e.OldValue;
            oldCollection.CollectionChanged -= sac.OnChartsChanged;
            newCollection.CollectionChanged += sac.OnChartsChanged;

            sac.CreateLayout();
        }
        #endregion

        #region ChartsSource

        public static readonly DependencyProperty ChartsSourceProperty = DependencyProperty.Register
        (
          "ChartsSource",
          typeof(IEnumerable),
          typeof(SharedAxisChart),
          new PropertyMetadata(default(IEnumerable), OnChartsSourceChanged)
        );

        public IEnumerable ChartsSource
        {
            get => (IEnumerable)GetValue(ChartsSourceProperty);
            set => SetValue(ChartsSourceProperty, value);
        }

        private static void OnChartsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SharedAxisChart sac)
                return;
            sac.DetachChartsSource(e.OldValue as IEnumerable);
            sac.AttachChartsSource(e.NewValue as IEnumerable);

        }

        #endregion ChartsSource

        #region SharedAxis
        public static readonly DependencyProperty SharedAxisProperty = DependencyProperty.Register
        (
          "SharedAxis",
          typeof(Axis),
          typeof(SharedAxisChart),
          new PropertyMetadata(null, OnSharedAxisChanged)
        );

        public Axis? SharedAxis
        {
            get => (Axis?)GetValue(SharedAxisProperty);
            set => SetValue(SharedAxisProperty, value);
        }

        private static void OnSharedAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SharedAxisChart sac)
                return;

            switch (sac.AxisLocation)
            {
                case SharedAxisEdge.Unknown:
                    break;
                case SharedAxisEdge.Bottom:
                    if (sac.SharedAxis != null)
                    {
                        ChartPanel.SetHorizontalPosition(sac.SharedAxis, HorizontalPosition.Center);
                        ChartPanel.SetVerticalPosition(sac.SharedAxis, VerticalPosition.Bottom);
                    }
                    break;
                default:
                    Debug.WriteLine("Shared axis/marker edge is not yet implemented.");
                        break;
            }

            sac.CreateLayout();

            var axis = e.NewValue as Axis;
            if (axis == null)
                return;

            axis.IsShared = true;
            axis.Mode = AxisMode.Primary;

            foreach (var chart in sac.Charts)
            {
                var oldAxis = chart.Axes.FirstOrDefault(a => a.IsShared);
                if (oldAxis != null) 
                    chart.Axes.Remove(oldAxis);
                chart.Axes.Add(axis);
            }
        }
        #endregion

        #region AxisLocation
        public static readonly DependencyProperty AxisLocationProperty = DependencyProperty.Register
        (
          "AxisLocation",
          typeof(SharedAxisEdge),
          typeof(SharedAxisChart),
          new PropertyMetadata(SharedAxisEdge.Unknown, OnAxisLocationChanged)
        );

        public SharedAxisEdge AxisLocation
        {
            get => (SharedAxisEdge)GetValue(AxisLocationProperty);
            set => SetValue(AxisLocationProperty, value);
        }

        private static void OnAxisLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SharedAxisChart sac)
                return;

            if (sac.SharedAxis != null)
            {
                switch ((SharedAxisEdge)e.NewValue)
                {
                    case SharedAxisEdge.Unknown:
                        break;
                    case SharedAxisEdge.Bottom:
                        ChartPanel.SetHorizontalPosition(sac.SharedAxis, HorizontalPosition.Center);
                        ChartPanel.SetVerticalPosition(sac.SharedAxis, VerticalPosition.Bottom);
                        break;
                    default:
                        Debug.WriteLine("Shared axis/marker edge is not yet implemented.");
                        break;
                }
            }
            sac.CreateLayout();
        }
        #endregion

        #region SharedMarker

        public static readonly DependencyProperty SharedMarkerProperty = DependencyProperty.Register
        (
          "SharedMarker",
          typeof(AxisMarker),
          typeof(SharedAxisChart),
          new PropertyMetadata(default(AxisMarker), OnSharedMarkerChanged)
        );

        public AxisMarker? SharedMarker
        {
            get => (AxisMarker?)GetValue(SharedMarkerProperty);
            set => SetValue(SharedMarkerProperty, value);
        }

        private static void OnSharedMarkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sac = d as SharedAxisChart;
            if (sac == null)
                return;

            switch (sac.MarkerLocation)
            {
                case SharedAxisEdge.Unknown:
                    break;
                case SharedAxisEdge.Top:
                    if (sac.SharedMarker != null) 
                        sac.SharedMarker.Orientation = Orientation.Horizontal;
                    break;
                default:
                    Debug.WriteLine("Shared axis/marker edge is not yet implemented.");
                        break;
            }

            sac.CreateLayout();

            var marker = e.NewValue as AxisMarker;
            if (marker == null)
                return;

            marker.IsShared = true;

            foreach (var chart in sac.Charts)
            {
                var oldMarker = chart.Markers.FirstOrDefault(m => m.IsShared);
                if (oldMarker != null)
                    chart.Markers.Remove(oldMarker);
                chart.Markers.Add(marker);
            }
        }

        #endregion SharedMarker

        #region MarkerLocation
        public static readonly DependencyProperty MarkerLocationProperty = DependencyProperty.Register
        (
          "MarkerLocation",
          typeof(SharedAxisEdge),
          typeof(SharedAxisChart),
          new PropertyMetadata(SharedAxisEdge.Unknown, OnMarkerLocationChanged)
        );

        public SharedAxisEdge MarkerLocation
        {
            get => (SharedAxisEdge)GetValue(MarkerLocationProperty);
            set => SetValue(MarkerLocationProperty, value);
        }

        private static void OnMarkerLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sac = d as SharedAxisChart;
            if (sac == null)
                return;

            if (sac.SharedMarker != null)
            {
                switch ((SharedAxisEdge)e.NewValue)
                {
                    case SharedAxisEdge.Unknown:
                        break;
                    case SharedAxisEdge.Top:
                        sac.SharedMarker.Orientation = Orientation.Horizontal;
                        break;
                    default:
                        Debug.WriteLine("Shared axis/marker edge is not yet implemented.");
                        break;
                }
            }

            sac.CreateLayout();
        }
        #endregion MarkerLocation

        #region SplitterThickness
        public static readonly DependencyProperty SplitterThicknessProperty = DependencyProperty.Register
        (
          "SplitterThickness",
          typeof(double),
          typeof(SharedAxisChart),
          new PropertyMetadata(3.0, OnSplitterThicknessChanged)
        );

        public double SplitterThickness
        {
            get => (double)GetValue(SplitterThicknessProperty);
            set => SetValue(SplitterThicknessProperty, value);
        }

        private static void OnSplitterThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #endregion dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void Refresh()
        {
            InvalidateVisual();
        }

        //---------------------------------------------------------------------
        #region Protected Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            var maxLeft = 0.0;
            var maxRight = 0.0;

            foreach (UIElement uie in Children)
            {
                var chart = uie as ChartPanel;
                if (chart == null)
                    continue;
                chart.Measure(availableSize);
                if (chart.Boundary.Left > maxLeft) maxLeft = chart.Boundary.Left;
                if (chart.Boundary.Right > maxRight) maxRight = chart.Boundary.Right;
            }

            var maxBoundary = new Thickness(maxLeft, 0.0, maxRight, 0.0);
            var markerWidth = availableSize.Width - maxLeft - maxRight;

            foreach (UIElement uie in Children)
            {
                if (uie is ChartPanel chart)
                {
                    chart.Boundary = maxBoundary;
                    chart.InvalidateMeasure();
                    continue;
                }

                if (uie is Rectangle markerLine && SharedMarker != null)
                {
                    var offset = SharedMarker.Value * markerWidth - 1.0;
                    markerLine.Margin = new Thickness(offset, 0, 0, 0);
                    markerLine.Measure(availableSize);
                }
            }
            if (ColumnDefinitions.Count <= 0)
                return availableSize;

            ColumnDefinitions[0] = new ColumnDefinition { Width = new GridLength(maxBoundary.Left, GridUnitType.Pixel) };
            ColumnDefinitions[2] = new ColumnDefinition { Width = new GridLength(maxBoundary.Right, GridUnitType.Pixel) };

            base.MeasureOverride(availableSize);
            return availableSize;
        }

        #endregion Protected Methods

        //---------------------------------------------------------------------
        #region Private Methods

        public void CreateLayout()
        {
            if (AxisLocation == SharedAxisEdge.Unknown)
                return;

            var row = 0;

            //Clear the grid
            Children.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            //Create columns
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Pixel) });
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Pixel) });

            //Add the shared marker
            if (MarkerLocation == SharedAxisEdge.Top && SharedMarker != null)
            {
                RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                SetRow(SharedMarker, row);
                SetColumn(SharedMarker, 1);
                Children.Add(SharedMarker);
                row++;

            }

            if (Charts.Count > 0)
            {
                //Add first child
                RowDefinitions.Add(new RowDefinition());
                UIElement uie = Charts[0];
                SetRow(uie, row);
                SetColumn(uie, 0);
                SetColumnSpan(uie, 3);
                Children.Add(uie);
                if (SharedMarker != null)
                    AddMarkerLine(row);
                row += 2;

                //Add rest with gridSplitters
                for (; row < Charts.Count * 2; row += 2)
                {
                    //GridSplitter
                    var splitterRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };
                    RowDefinitions.Add(splitterRow);
                    var splitter = new GridSplitter
                    {
                        Height = SplitterThickness,
                        Background = Background,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center,
                        ResizeBehavior = GridResizeBehavior.BasedOnAlignment
                    };
                    SetRow(splitter, row - 1);
                    SetColumn(splitter, 1);
                    //SetColumnSpan(uie, 3);
                    Children.Add(splitter);
                    //Child
                    RowDefinitions.Add(new RowDefinition());
                    uie = Charts[row / 2];
                    SetRow(uie, row);
                    SetColumn(uie, 0);
                    SetColumnSpan(uie, 3);
                    Children.Add(uie);
                    if (SharedMarker != null)
                        AddMarkerLine(row);
                }

            }

            //Add the shared axis
            if (AxisLocation != SharedAxisEdge.Bottom || SharedAxis == null)
                return;
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            SetRow(SharedAxis, Charts.Count * 2);
            SetColumn(SharedAxis, 1);
            Children.Add(SharedAxis);
        }

        private void AddMarkerLine(int row)
        {
            var line = new Rectangle()
            {
                Style = SharedMarker?.LineStyle,
            };
            SetRow(line, row);
            SetColumn(line, 1);
            Children.Add(line);
            SetZIndex(line, 100);
        }

        private void AttachChartsSource(IEnumerable? chartsSource)
        {
            if (chartsSource == null)
                return;

            AddCharts(chartsSource);

            var chartsSourceAsNotifier = chartsSource as INotifyCollectionChanged;
            if (chartsSourceAsNotifier != null)
                chartsSourceAsNotifier.CollectionChanged += ChartsSourceItemsChanged;
        }

        private void AddCharts(IEnumerable charts, int index = 0)
        {
            var currentCharts = Charts.Select(d => d.DataContext).ToArray();
            var chartsToAdd = new List<object>(charts.OfType<object>());
            foreach (var chartSource in chartsToAdd)
            {
                if (currentCharts.Contains(chartSource))
                    continue;
                if (CreateVisualFromObject(chartSource) is not ChartPanel chart)
                    continue;

                chart.Loaded += (_, _) =>
                {
                    var presenters = chart.Children.OfType<DataPresenter>();
                    foreach (var p in presenters)
                    {
                        p.XAxis = SharedAxis;
                        p.YAxis = p.Axes.FirstOrDefault(a => a.Name == p.YAxisName);
                    }
                };
                Charts.Insert(index, chart);
                //chart.Refresh();
            }
        }

        private void RemoveCharts(IEnumerable chartsToRemove)
        {
            var currentCharts = Charts.Select(d => d.DataContext).ToList();
            var oldCharts = new List<ChartPanel>();

            foreach (var oldChartObj in chartsToRemove)
            {
                oldCharts.AddRange(Charts.Where(chart => chart.DataContext == oldChartObj));
            }

            foreach (var chart in oldCharts)
            {
                Charts.Remove(chart);
            }
        }

        private void DetachChartsSource(IEnumerable? chartsSource)
        {
            if (chartsSource == null)
                return;

            RemoveCharts(chartsSource);

            if (chartsSource is INotifyCollectionChanged chartsSourceAsNotifier)
                chartsSourceAsNotifier.CollectionChanged -= ChartsSourceItemsChanged;
        }

        public UIElement? CreateVisualFromObject(object sourceObject)
        {
            // Find a DataTemplate defined for the DataType
            DataTemplateKey key;
            if (sourceObject is IChartVM chartObject)
                key = new DataTemplateKey(chartObject.DataTemplateKey);
            else
                key = new DataTemplateKey(sourceObject.GetType());
            var resource = FindResource(key);
            var dataTemplate = (DataTemplate)resource;
            if (dataTemplate.LoadContent() is FrameworkElement fe)
            {
                fe.DataContext = sourceObject;
                return fe;
            }
            return null;
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnChartsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.OldItems != null)
            //{
            //}

            if (e.NewItems != null)
            {
                foreach (ChartPanel chart in e.NewItems)
                    if (SharedAxis != null)
                        chart.Axes.Add(SharedAxis);
            }
            CreateLayout();
        }

        private void ChartsSourceItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //handle remove
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                    RemoveCharts(e.OldItems);
            }

            //handle add
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.NewItems != null)
                    AddCharts(e.NewItems, e.NewStartingIndex);
            }
        }

        #endregion Event Handlers
    }
}




