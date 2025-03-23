using AvalonDock.Themes;

namespace Arctos.Wpf.AvalonDock
{
  public class DynamicTheme : Theme
  {
    public override Uri GetResourceUri()
    {
      return new Uri("/Arctos.Wpf;component/Themes/DockTheme.xaml", UriKind.Relative);
    }
  }
}
