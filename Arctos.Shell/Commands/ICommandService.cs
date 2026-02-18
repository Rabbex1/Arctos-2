namespace Arctos.Shell.Commands
{
    public interface ICommandService
    {
        void AddCommand(Command command);
        CommandAddResult TryAddCommand(Command command);
        void Execute(string commandSpace, string command, ExecuteCommandArgs args);
        void Execute(string fullCommand, ExecuteCommandArgs args);
    }
}
