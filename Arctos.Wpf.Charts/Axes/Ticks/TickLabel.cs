using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
  public class TickLabel : TextBlock, ITickDecorator
  {
    public Point Center { get; set; }
  }
}
