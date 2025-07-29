using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Arctos.Wpf.Charts
{
    public class Chart : Control, IPresenterHost
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private const string ChartAreaName = "PART_ChartArea";

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ChartPanel? ChartArea { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        static Chart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Chart), new FrameworkPropertyMetadata(typeof(Chart)));
        }

        public Chart()
        {
            Presenters = new ObservableCollection<IPresenter>();
            Axes = new ObservableCollection<Axis>();
            Markers = new ObservableCollection<AxisMarker>();
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region Title

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        (
          "Title",
          typeof(string),
          typeof(Chart),
          new PropertyMetadata("Title", OnTitleChanged)
        );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Presenters

        public static readonly DependencyProperty PresentersProperty = DependencyProperty.Register
        (
          nameof(Presenters),
          typeof(ObservableCollection<IPresenter>),
          typeof(Chart),
          new PropertyMetadata(null, OnPresentersChanged)
        );

        public ObservableCollection<IPresenter> Presenters
        {
            get => (ObservableCollection<IPresenter>)GetValue(PresentersProperty);
            set => SetValue(PresentersProperty, value);
        }

        private static void OnPresentersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Chart chart)
                return;
            var newPresenters = (ObservableCollection<IPresenter>)e.NewValue;
            var oldPresenters = (ObservableCollection<IPresenter>)e.OldValue;
            chart.PresentersChanged(newPresenters, oldPresenters);
        }

        #endregion

        #region Axes
        public static readonly DependencyProperty AxesProperty = DependencyProperty.Register
        (
          nameof(Axes),
          typeof(ObservableCollection<Axis>),
          typeof(Chart),
          new PropertyMetadata(null, OnAxesChanged)
        );

        public ObservableCollection<Axis> Axes
        {
            get => (ObservableCollection<Axis>)GetValue(AxesProperty);
            set => SetValue(AxesProperty, value);
        }

        private static void OnAxesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Chart chart)
                return;
            var newCollection = (ObservableCollection<Axis>)e.NewValue;
            var oldCollection = (ObservableCollection<Axis>)e.OldValue;
            chart.AxesChanged(newCollection, oldCollection);
        }
        #endregion

        #region Markers

        public static readonly DependencyProperty MarkersProperty = DependencyProperty.Register
        (
          nameof(Markers),
          typeof(ObservableCollection<AxisMarker>),
          typeof(Chart),
          new PropertyMetadata(null, OnMarkerChanged)
        );

        public ObservableCollection<AxisMarker> Markers
        {
            get => (ObservableCollection<AxisMarker>)GetValue(MarkersProperty);
            set => SetValue(MarkersProperty, value);
        }

        private static void OnMarkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Chart chart)
                return;
            var newCollection = (ObservableCollection<AxisMarker>)e.NewValue;
            var oldCollection = (ObservableCollection<AxisMarker>)e.OldValue;
            chart.MarkersChanged(newCollection, oldCollection);
        }

        #endregion SharedMarker

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void Refresh()
        {
            ChartArea?.Refresh();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ChartArea = GetTemplateChild(ChartAreaName) as ChartPanel;

            if (ChartArea != null)
            {
                foreach (var axis in Axes)
                    if (!axis.IsShared)
                        ChartArea.Children.Add(axis);

                foreach(var marker in Markers)
                    AddMarker(marker);

                foreach (var presenter in Presenters)
                    ChartArea.Children.Add(presenter as UIElement 
                                            ?? throw new InvalidOperationException("presenter must be UIElement"));
            }
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private void PresentersChanged(ObservableCollection<IPresenter>? newPresenters, ObservableCollection<IPresenter>? oldPresenters)
        {
            if (oldPresenters != null)
            {
                foreach (var presenter in oldPresenters)
                {
                    presenter.PresenterHost = null;
                    if (!(presenter is UIElement p)) continue;
                    ChartArea?.Children.Remove(p);
                }
                oldPresenters.CollectionChanged -= OnPresentersChanged;
            }

            if (newPresenters != null)
            {
                foreach (var presenter in newPresenters)
                {
                    presenter.PresenterHost = this;
                    if (presenter is not UIElement p) 
                        continue;
                    ChartArea?.Children.Add(p);
                }
                newPresenters.CollectionChanged += OnPresentersChanged;
            }
        }

        private void AxesChanged(ObservableCollection<Axis>? newAxes, ObservableCollection<Axis>? oldAxes)
        {
            if (oldAxes != null)
            {
                foreach (var axis in oldAxes)
                    ChartArea?.Children.Remove(axis);
                oldAxes.CollectionChanged -= OnAxesChanged;
            }

            if (newAxes != null)
            {
                foreach (var axis in newAxes)
                    ChartArea?.Children.Add(axis);
                newAxes.CollectionChanged += OnAxesChanged;
            }
        }

        private void MarkersChanged(ObservableCollection<AxisMarker>? newMarkers, ObservableCollection<AxisMarker>? oldMarkers)
        {
            if (oldMarkers != null)
            {
                foreach (var marker in oldMarkers)
                    ChartArea?.Children.Remove(marker);
                oldMarkers.CollectionChanged -= OnMarkersChanged;
            }

            if (newMarkers != null)
            {
                foreach (var marker in newMarkers)
                    AddMarker(marker);
                newMarkers.CollectionChanged += OnMarkersChanged;
            }
        }

        private void AddMarker(AxisMarker marker)
        {
            var line = new Rectangle()
            {
                Style = marker.LineStyle,
            };
            ChartArea?.Children.Add(marker);
            ChartArea?.Children.Add(line);
        }



        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnPresentersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IPresenter presenter in e.OldItems)
                {
                    presenter.PresenterHost = null;
                    ChartArea?.Children.Remove(presenter as UIElement
                                               ?? throw new InvalidOperationException("presenter must be UIElement"));
                }
            }

            if (e.NewItems != null)
            {
                foreach (IPresenter presenter in e.NewItems)
                {
                    presenter.PresenterHost = this;
                    ChartArea?.Children.Add(presenter as UIElement
                                            ?? throw new InvalidOperationException("presenter must be UIElement"));
                    //if (presenter is GridLinePresenter)
                    //  AttacheAxesToGrid(presenter as GridLinePresenter);
                }
            }
        }

        private void OnAxesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Axis axis in e.OldItems)
                    ChartArea?.Children.Remove(axis);
            }

            if (e.NewItems != null)
            {
                foreach (Axis axis in e.NewItems)
                    ChartArea?.Children.Add(axis);
            }
        }

        private void OnMarkersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (AxisMarker marker in e.OldItems)
                    ChartArea?.Children.Remove(marker);
            }

            if (e.NewItems != null)
            {
                foreach (AxisMarker marker in e.NewItems)
                    AddMarker(marker);
            }
        }


        #endregion Event Handlers
    }
}
