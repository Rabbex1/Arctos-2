using System.Windows;
using System.Windows.Documents;
using Color = System.Windows.Media.Color;

#nullable disable

namespace Arctos.Wpf.DragDrop
{
  public abstract class DropAdorner : Adorner
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    protected AdornerLayer AdornerLayer;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    protected DropAdorner(UIElement adornedElement)
      : base(adornedElement)
    {
      IsHitTestVisible = false;
      AdornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
      AdornerLayer?.Add(this);
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public abstract void Update(Rect rect, Color fillColor, Color penCcolor, double penThickness);
    public abstract void Remove();

    #endregion Methods
  }
}
