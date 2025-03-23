using System;
using System.Collections.Generic;

namespace Arctos.Wpf.Charts
{
  public interface IDataPresenter : IPresenter
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    IDataProvider? DataProvider { get; set; }
    List<Axis> Axes { get; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    void SetFocalData(object? obj);

    #endregion Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events

    event EventHandler<FocalDataEventArgs> FocalDataChanged;

    #endregion Events
  }
}
