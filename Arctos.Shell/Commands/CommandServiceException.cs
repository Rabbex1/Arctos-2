namespace Arctos.Shell.Commands
{
  public class CommandServiceException : Exception
  {
      //------------------------------------------------------------------------------------------------------------------------------------------
      #region Constructors

      public CommandServiceException()
      {
      }

      public CommandServiceException(string message)
          : base(message)
      {
      }

      public CommandServiceException(string message, Exception inner)
          : base(message, inner)
      {
      }

      #endregion Constructors
  }
}
