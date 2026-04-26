namespace Arctos.Shell.Commands
{
    public interface ICommandService
    {
        void AddCommand(Command command);
        CommandAddResult TryAddCommand(Command command);
        bool TryRemoveCommand(string commandSpace, string commandName);
        void Execute(string commandSpace, string command, ExecuteCommandArgs args);
        void Execute(string fullCommand, ExecuteCommandArgs args);
    }
}