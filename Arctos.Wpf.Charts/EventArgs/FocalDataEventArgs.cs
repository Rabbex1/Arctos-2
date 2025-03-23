using System;

namespace Arctos.Wpf.Charts
{
  public class FocalDataEventArgs : EventArgs
  {
      //------------------------------------------------------------------------------------------------------------------------------------------
      #region Properties

      public object Data { get; }

      #endregion Properties

      //------------------------------------------------------------------------------------------------------------------------------------------
      #region Constructors

      public FocalDataEventArgs(object data)
      {
          Data = data;
      }

      #endregion Constructors

  }
}
