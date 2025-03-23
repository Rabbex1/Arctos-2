using System;

namespace Arctos.Shell.Options
{
  public class OptionsDocumentEventArgs : EventArgs
  {
      //------------------------------------------------------------------------------------------------------------------------------------------
      #region Properties

      public OptionsPage Page { get; set; }

      #endregion Properties

      //------------------------------------------------------------------------------------------------------------------------------------------
      #region Constructors

      public OptionsDocumentEventArgs(OptionsPage page)
      {
          Page = page;
      }

      #endregion Constructors

  }
}
