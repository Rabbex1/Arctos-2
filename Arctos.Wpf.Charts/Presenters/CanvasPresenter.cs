using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class CanvasPresenter : Presenter, ICanvasHost
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Axis? XAxis { get; set; }
        public Axis? YAxis { get; set; }
        public Axis? PrimaryAxis { get; set; }

        #endregion Properties

        //----------------------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependancy Properties

        #region Canvases
        public static readonly DependencyProperty CanvasesProperty = DependencyProperty.Register
        (
          "Canvases",
          typeof(ObservableCollection<ChartCanvas>),
          typeof(CanvasPresenter),
          new PropertyMetadata(default(ObservableCollection<ChartCanvas>), OnCanvasesChanged)
        );

        public ObservableCollection<ChartCanvas> Canvases
        {
            get => (ObservableCollection<ChartCanvas>)GetValue(CanvasesProperty);
            set => SetValue(CanvasesProperty, value);
        }

        private static void OnCanvasesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CanvasPresenter presenter)
                return;
            var newCanvases = (ObservableCollection<ChartCanvas>)e.NewValue;
            var oldCanvases = (ObservableCollection<ChartCanvas>)e.OldValue;
            presenter.CanvasesChanged(newCanvases, oldCanvases);
        }
        #endregion

        #region XAxisName
        public static readonly DependencyProperty XAxisNameProperty = DependencyProperty.Register
        (
          "XAxisName",
          typeof(string),
          typeof(CanvasPresenter),
          new PropertyMetadata(default(string), OnXAxisNameChanged)
        );

        public string XAxisName
        {
            get => (string)GetValue(XAxisNameProperty);
            set => SetValue(XAxisNameProperty, value);
        }

        private static void OnXAxisNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CanvasPresenter presenter)
                return;
            var namedAxis = presenter.PresenterHost?.Axes.FirstOrDefault(a => a.Name == presenter.XAxisName);
            if (namedAxis != null)
                presenter.XAxis = namedAxis;
        }
        #endregion

        #region YAxisName
        public static readonly DependencyProperty YAxisNameProperty = DependencyProperty.Register
        (
          "YAxisName",
          typeof(string),
          typeof(CanvasPresenter),
          new PropertyMetadata(default(string), OnYAxisNameChanged)
        );

        public string YAxisName
        {
            get => (string)GetValue(YAxisNameProperty);
            set => SetValue(YAxisNameProperty, value);
        }

        private static void OnYAxisNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CanvasPresenter presenter)
                return;
            var namedAxis = presenter.PresenterHost?.Axes.FirstOrDefault(a => a.Name == presenter.YAxisName);
            if (namedAxis != null)
                presenter.YAxis = namedAxis;
        }
        #endregion

        #endregion Dependancy Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public CanvasPresenter()
        {
            Canvases = new ObservableCollection<ChartCanvas>();
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------

        #region Public Methods

        public override void SetFocalData(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

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

        #endregion Constructors


    }
}
