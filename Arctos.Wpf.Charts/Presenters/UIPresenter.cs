using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
  public class UiPresenter : UserControl, IPresenter
  {
    static UiPresenter()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(UiPresenter), new FrameworkPropertyMetadata(typeof(UiPresenter)));
    }

    public IPresenterHost? PresenterHost { get; set; }
  }
}
