using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace Arctos.Wpf.DragDrop
{
  public class RectangleDropAdorner(UIElement adornedElement) : DropAdorner(adornedElement)
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private Rect _adornedRect;
    private SolidColorBrush? _renderBrush;
    private SolidColorBrush? _renderPenBrush;
    private Pen? _renderPen;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public override void Update(Rect rect, Color fillColor, Color penCcolor, double penThickness)
    {
      if (_adornedRect.Equals(rect) && Visibility != Visibility.Collapsed)
        return;
      _adornedRect = rect;
      _renderBrush = new SolidColorBrush(fillColor);
      _renderPenBrush = new SolidColorBrush(penCcolor);
      _renderPen = new Pen(_renderPenBrush, penThickness);

      AdornerLayer.Update(AdornedElement);
      Visibility = Visibility.Visible;
    }

    public override void Remove()
    {
      Visibility = Visibility.Collapsed;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      drawingContext.DrawRectangle(_renderBrush, _renderPen, _adornedRect);
    }

    #endregion Methods

  }
}
