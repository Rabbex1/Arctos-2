namespace Arctos.Shell.Commands
{
    public class ExecuteCommandArgs(object? context, object? param) : EventArgs
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public object? Context { get; } = context;
        public object? Parameters { get; } = param;

        #endregion Properties
    }
}