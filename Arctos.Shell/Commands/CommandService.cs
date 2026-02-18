using System;
using System.Collections.Generic;
using System.Windows;
using Arctos.Shell.Dialogs;

namespace Arctos.Shell.Commands
{
    public class CommandService(IDialogService dialogService) : ICommandService
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Dictionary<string, Dictionary<string, Command>> _commands = new();
        private readonly IDialogService _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        #endregion Fields

        public void AddCommand(Command command)
        {
            var result = TryAddCommand(command);
            if (result == CommandAddResult.Added)
                return;

            if (result == CommandAddResult.AlreadyExists)
            {
                throw new CommandServiceException($"Cannot add command: {command.CommandSpace}:{command.Name} (already exists)");
            }

            throw new CommandServiceException($"Cannot add command: {command.CommandSpace}:{command.Name} (conflict)");
        }

        public CommandAddResult TryAddCommand(Command command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!_commands.TryGetValue(command.CommandSpace, out var commandSpace))
            {
                commandSpace = new Dictionary<string, Command>();
                _commands.Add(command.CommandSpace, commandSpace);
            }

            if (commandSpace.TryAdd(command.Name, command))
                return CommandAddResult.Added;

            var existing = commandSpace[command.Name];
            return HaveSameHandler(existing.OnExecute, command.OnExecute)
                ? CommandAddResult.AlreadyExists
                : CommandAddResult.Conflict;
        }

        public void Execute(string commandSpace, string command, ExecuteCommandArgs args)
        {
            if (!_commands.TryGetValue(commandSpace, out var cmdSpace))
            {
                throw new CommandServiceException($"Cannot find command space: {commandSpace}");
            }

            if (!cmdSpace.TryGetValue(command, out var cmd))
            {
                throw new CommandServiceException($"Cannot find command: {commandSpace}:{command}");
            }

            cmd.Execute(args);
        }

        public void Execute(string fullCommand, ExecuteCommandArgs args)
        {
            var parts = fullCommand.Split('.');
            if (parts.Length != 2)
                _dialogService.ShowMessageBox($"Cannot execute command {fullCommand}.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                Execute(parts[0], parts[1], args);
        }

        private static bool HaveSameHandler(Action<Command, ExecuteCommandArgs> left, Action<Command, ExecuteCommandArgs> right)
        {
            var leftList = left.GetInvocationList();
            var rightList = right.GetInvocationList();

            if (leftList.Length != rightList.Length)
                return false;

            for (int i = 0; i < leftList.Length; i++)
            {
                if (leftList[i].Method != rightList[i].Method)
                    return false;

                var leftTargetType = leftList[i].Target?.GetType();
                var rightTargetType = rightList[i].Target?.GetType();
                if (leftTargetType != rightTargetType)
                    return false;
            }

            return true;
        }
    }
}
