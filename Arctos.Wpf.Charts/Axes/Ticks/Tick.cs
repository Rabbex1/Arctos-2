using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
    public class Tick : Canvas
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Point Center { get; private set; }
        public Point Position { get; private set; }
        public object Value { get; set; } = new();
        public UIElement Glyph { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public Tick(UIElement glyph)
        {
            Glyph = glyph;
        }

        #endregion Constructor


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void SetCenter(double a, double b, PartOrientation orientation)
        {
            if (orientation == PartOrientation.Horizontal)
            {
                Center = new Point(a, b);
                Position = new Point(a - DesiredSize.Width / 2, b);
            }
            else
            {
                Center = new Point(b, a);
                Position = new Point(b, a - DesiredSize.Height / 2);
            }
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            Children.Clear();
            Glyph.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Children.Add(Glyph);
            return Glyph.DesiredSize;
        }

        protected override Size ArrangeOverride(Size totalSize)
        {
            Children[0].Arrange(new Rect(new Point(0, 0), Children[0].DesiredSize));
            return totalSize;
        }

        #endregion Protected Methods

        #endregion Methods
    }
}
