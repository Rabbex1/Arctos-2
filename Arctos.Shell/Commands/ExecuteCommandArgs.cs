namespace Arctos.Shell.Commands
{
    public class ExecuteCommandArgs : EventArgs
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public object? Param { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public ExecuteCommandArgs(object? param)
        {
            Param = param;
        }

        #endregion Constructors

    }
}