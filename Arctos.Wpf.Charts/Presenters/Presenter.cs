using System.Windows.Controls;
using Arctos.Wpf.Helpers;

namespace Arctos.Wpf.Charts
{
  public abstract class Presenter : Canvas, IPresenter
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    private IPresenterHost? _presenterHost;
    public IPresenterHost? PresenterHost
    {
      get
      {
        if (_presenterHost != null)
          return _presenterHost;
        //If not set then climb the visual tree and set it to the chart;
        _presenterHost = this.TryFindAncestor<Chart>();
        
        return _presenterHost;
      }

      set => _presenterHost = value;
    }

    public IDataProvider? DataProvider { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public abstract void SetFocalData(object obj);

    #endregion Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events

    public event EventHandler<FocalDataEventArgs>? FocalDataChanged;
    public void RaiseFocalDataChanged(FocalDataEventArgs args)
    {
      FocalDataChanged?.Invoke(this, args);
    }

    #endregion Events


  }
}
