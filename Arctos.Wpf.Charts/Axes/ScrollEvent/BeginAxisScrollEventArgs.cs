using System.Windows;
using System.Windows.Input;

namespace Arctos.Wpf.Charts
{
  public class BeginAxisScrollEventArgs : RoutedEventArgs
  {
    public InputDevice? Device { get; set; }
    public Point ScrollOrigin { get; set; }
    public Point CurrentPosition { get; set; }
  }
}
