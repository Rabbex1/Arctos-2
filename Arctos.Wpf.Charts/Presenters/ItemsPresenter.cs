using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
  public class ItemsPresenter : ItemsControl, IPresenter
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public IPresenterHost? PresenterHost { get; set; }

    #endregion Properties
  }
}
