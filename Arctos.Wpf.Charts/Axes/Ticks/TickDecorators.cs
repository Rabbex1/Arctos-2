using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
  public class TickDecorators<T> : Panel
    where T : UIElement, ITickDecorator
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly List<T> _labels;
    private readonly Axis _owner;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructor

    public TickDecorators(List<T> decorators, Axis owner)
    {
      _labels = decorators;
      _owner = owner;
    }

    #endregion Constructor

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    protected override Size MeasureOverride(Size availableSize)
    {
      var maxWidth = 0.0;
      var maxHeight = 0.0;

      Children.Clear();
      foreach (var label in _labels)
      {
        label.Measure(availableSize);
        if (label.DesiredSize.Width > maxWidth) maxWidth = label.DesiredSize.Width;
        if (label.DesiredSize.Height > maxHeight) maxHeight = label.DesiredSize.Height;
        Children.Add(label);
      }
      return _owner.Orientation == PartOrientation.Horizontal
        ? new Size(availableSize.Width, maxHeight)
        : new Size(maxWidth, availableSize.Height);
    }

    protected override Size ArrangeOverride(Size totalSize)
    {
      var vertical = ChartPanel.GetVerticalPosition(_owner);
      var horizontal = ChartPanel.GetHorizontalPosition(_owner);

      foreach (T child in Children)
      {
        var point = new Point(0, 0);
        if (vertical == VerticalPosition.Center) //Horizonatal axis
        {
          point.Y = child.Center.Y - child.DesiredSize.Height / 2;
          point.X = horizontal == HorizontalPosition.Left ? DesiredSize.Width - child.DesiredSize.Width : 0;
        }
        else
        {
          point.X = child.Center.X - child.DesiredSize.Width / 2;
          point.Y = vertical == VerticalPosition.Top ? DesiredSize.Height - child.DesiredSize.Height : 0;
        }
        child.Arrange(new Rect(point, child.DesiredSize));
      }
      return totalSize;
    }

    #endregion Methods
  }
}
