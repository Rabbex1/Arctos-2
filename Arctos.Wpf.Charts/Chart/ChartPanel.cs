using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Arctos.Wpf.Charts
{
    [ContentProperty("NotifiableChildren")]
    public class ChartPanel : Canvas, INotifiableParent, IPresenterHost
    {

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Boundary
        private Thickness _boundary;
        public Thickness Boundary
        {
            get => _boundary;
            set => _boundary = value;
        }
        #endregion

        public ObservableCollection<Axis> Axes { get; } = new ObservableCollection<Axis>();
        public ObservableCollection<AxisMarker> Markers { get; } = new ObservableCollection<AxisMarker>();
        public NotifiableUiElementCollection NotifiableChildren { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors
        static ChartPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartPanel), new FrameworkPropertyMetadata(typeof(ChartPanel)));
        }

        public ChartPanel()
        {
            NotifiableChildren = new NotifiableUiElementCollection(this, this);
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region HorizontalPosition
        public static readonly DependencyProperty HorizontalPositionProperty = DependencyProperty.RegisterAttached
        (
          "HorizontalPosition",
          typeof(HorizontalPosition),
          typeof(ChartPanel),
          new PropertyMetadata(default(HorizontalPosition))
        );

        public static void SetHorizontalPosition(DependencyObject element, HorizontalPosition value)
        {
            element.SetValue(HorizontalPositionProperty, value);
        }

        public static HorizontalPosition GetHorizontalPosition(DependencyObject element)
        {
            return (HorizontalPosition)element.GetValue(HorizontalPositionProperty);
        }
        #endregion

        #region VerticalPosition
        public static readonly DependencyProperty VerticalPositionProperty = DependencyProperty.RegisterAttached
        (
          "VerticalPosition",
          typeof(VerticalPosition),
          typeof(ChartPanel),
          new PropertyMetadata(default(VerticalPosition))
        );

        public static void SetVerticalPosition(DependencyObject element, VerticalPosition value)
        {
            element.SetValue(VerticalPositionProperty, value);
        }

        public static VerticalPosition GetVerticalPosition(DependencyObject element)
        {
            return (VerticalPosition)element.GetValue(VerticalPositionProperty);
        }
        #endregion

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods
        //---------------------------------------------------------------------
        #region Public Methods

        public int AddChild(UIElement child)
        {
            if (child is Axis axis)
                Axes.Add(axis);
            else if (child is AxisMarker marker)
                Markers.Add(marker);
            else if (child is IPresenter presenter)
                presenter.PresenterHost = this;
            return Children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (child is Axis item)
                Axes.Remove(item);
            else if (child is IPresenter presenter)
                presenter.PresenterHost = null;
            Children.Remove(child);
        }

        public void Refresh()
        {
            InvalidateVisual();
        }


        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            #region First Pass
            IList<UIElement> topElements = Children.OfType<UIElement>().Where(element => GetVerticalPosition(element) == VerticalPosition.Top).ToList();
            var topArray = new double[3];
            foreach (var element in topElements)
            {
                element.Measure(availableSize);
                switch (GetHorizontalPosition(element))
                {
                    case HorizontalPosition.Left:
                        topArray[0] += element.DesiredSize.Height;
                        break;
                    case HorizontalPosition.Center:
                        topArray[1] += element.DesiredSize.Height;
                        break;
                    case HorizontalPosition.Right:
                        topArray[2] += element.DesiredSize.Height;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _boundary.Top = topArray.Max();

            IList<UIElement> bottomElements = Children.OfType<UIElement>().Where(element => GetVerticalPosition(element) == VerticalPosition.Bottom).ToList();
            var bottomArray = new double[3];
            foreach (var element in bottomElements)
            {
                element.Measure(availableSize);
                switch (GetHorizontalPosition(element))
                {
                    case HorizontalPosition.Left:
                        bottomArray[0] += element.DesiredSize.Height;
                        break;
                    case HorizontalPosition.Center:
                        bottomArray[1] += element.DesiredSize.Height;
                        break;
                    case HorizontalPosition.Right:
                        bottomArray[2] += element.DesiredSize.Height;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _boundary.Bottom = bottomArray.Max();


            IList<UIElement> leftElements = Children.OfType<UIElement>().Where(element => GetHorizontalPosition(element) == HorizontalPosition.Left).ToList();
            var leftArray = new double[3];
            foreach (var element in leftElements)
            {
                element.Measure(availableSize);
                switch (GetVerticalPosition(element))
                {
                    case VerticalPosition.Top:
                        leftArray[0] += element.DesiredSize.Width;
                        break;
                    case VerticalPosition.Center:
                        leftArray[1] += element.DesiredSize.Width;
                        break;
                    case VerticalPosition.Bottom:
                        leftArray[2] += element.DesiredSize.Width;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _boundary.Left = leftArray.Max();

            IList<UIElement> rightElements = Children.OfType<UIElement>().Where(element => GetHorizontalPosition(element) == HorizontalPosition.Right).ToList();
            var rightArray = new double[3];
            foreach (var element in rightElements)
            {
                element.Measure(availableSize);
                switch (GetVerticalPosition(element))
                {
                    case VerticalPosition.Top:
                        rightArray[0] += element.DesiredSize.Width;
                        break;
                    case VerticalPosition.Center:
                        rightArray[1] += element.DesiredSize.Width;
                        break;
                    case VerticalPosition.Bottom:
                        rightArray[2] += element.DesiredSize.Width;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _boundary.Right = rightArray.Max();

            var centerSize = new Size(availableSize.Width - _boundary.Left - _boundary.Right, availableSize.Height - _boundary.Top - _boundary.Bottom);
            IList<UIElement> centerElements = this.Children.OfType<UIElement>().Where(element => GetHorizontalPosition(element) == HorizontalPosition.Center &&
                                                                                                 GetVerticalPosition(element) == VerticalPosition.Center).ToList();
            foreach (var element in centerElements)
            {
                if(element is CanvasPresenter)
                    element.InvalidateMeasure();
                element.Measure(centerSize);
            }
            #endregion

            #region Second Pass

            foreach (UIElement element in Children)
            {
                var hPos = GetHorizontalPosition(element);
                var vPos = GetVerticalPosition(element);
                var size = new Size();

                switch (hPos)
                {
                    case HorizontalPosition.Center when vPos == VerticalPosition.Top:
                        size.Width = availableSize.Width - _boundary.Left - _boundary.Right;
                        size.Height = element.DesiredSize.Height;
                        break;
                    case HorizontalPosition.Center when vPos == VerticalPosition.Bottom:
                        size.Width = availableSize.Width - _boundary.Left - _boundary.Right;
                        size.Height = element.DesiredSize.Height;
                        break;
                    case HorizontalPosition.Left when vPos == VerticalPosition.Center:
                        size.Width = element.DesiredSize.Width;
                        size.Height = availableSize.Height - _boundary.Top - _boundary.Bottom;
                        break;
                    case HorizontalPosition.Right when vPos == VerticalPosition.Center:
                        size.Width = element.DesiredSize.Width;
                        size.Height = availableSize.Height - _boundary.Top - _boundary.Bottom;
                        break;
                    default:
                        size.Width = availableSize.Width - _boundary.Left - _boundary.Right;
                        size.Height = availableSize.Height - _boundary.Top - _boundary.Bottom;
                        break;
                }
                element.Measure(size);
            }

            #endregion

            return availableSize;
        }

        protected override Size ArrangeOverride(Size totalSize)
        {
            var currentTop = 0.0;
            var currentBottom = totalSize.Height;
            var currentLeft = 0.0;
            var currentRight = totalSize.Width;

            foreach (UIElement element in Children)
            {
                var hPos = GetHorizontalPosition(element);
                var vPos = GetVerticalPosition(element);
                var rect = new Rect();

                switch (hPos)
                {
                    case HorizontalPosition.Center when vPos == VerticalPosition.Top:
                        rect.X = _boundary.Left;
                        rect.Y = currentTop;
                        rect.Width = totalSize.Width - _boundary.Left - _boundary.Right;
                        rect.Height = element.DesiredSize.Height;
                        currentTop += rect.Height;
                        break;
                    case HorizontalPosition.Center when vPos == VerticalPosition.Bottom:
                        rect.X = _boundary.Left;
                        rect.Y = currentBottom - element.DesiredSize.Height;
                        rect.Width = totalSize.Width - _boundary.Left - _boundary.Right;
                        rect.Height = element.DesiredSize.Height;
                        currentBottom -= rect.Height;
                        break;
                    case HorizontalPosition.Left when vPos == VerticalPosition.Center:
                        rect.X = currentLeft;
                        rect.Y = _boundary.Top;
                        rect.Width = element.DesiredSize.Width;
                        rect.Height = totalSize.Height - _boundary.Top - _boundary.Bottom;
                        currentLeft += rect.Width;
                        break;
                    case HorizontalPosition.Right when vPos == VerticalPosition.Center:
                        rect.X = currentRight - element.DesiredSize.Width;
                        rect.Y = _boundary.Top;
                        rect.Width = element.DesiredSize.Width;
                        rect.Height = totalSize.Height - _boundary.Top - _boundary.Bottom;
                        currentRight -= rect.Width;
                        break;
                    default:
                        rect.X = _boundary.Left;
                        rect.Y = _boundary.Top;
                        rect.Width = totalSize.Width - _boundary.Left - _boundary.Right;
                        rect.Height = totalSize.Height - _boundary.Top - _boundary.Bottom;
                        break;
                }
                element.Arrange(rect);
            }

            return totalSize;
        }


        #endregion Protected Methods


        #endregion Methods
    }
}
