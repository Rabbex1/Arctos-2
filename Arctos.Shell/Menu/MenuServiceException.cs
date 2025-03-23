namespace Arctos.Shell.Menu
{
  public class MenuServiceException : Exception
  {
      //------------------------------------------------------------------------------------------------------------------------------------------
      #region Constructors

      public MenuServiceException()
      {
      }

      public MenuServiceException(string message)
          : base(message)
      {
      }

      public MenuServiceException(string message, Exception inner)
          : base(message, inner)
      {
      }

      #endregion Constructors
  }
}
