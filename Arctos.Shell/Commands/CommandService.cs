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

        private readonly Dictionary<string, Dictionary<string, Command>> _commands = [];
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
            ArgumentNullException.ThrowIfNull(command);

            if (!_commands.TryGetValue(command.CommandSpace, out var commandSpace))
            {
                commandSpace = [];
                _commands.Add(command.CommandSpace, commandSpace);
            }

            if (commandSpace.TryAdd(command.Name, command))
                return CommandAddResult.Added;

            var existing = commandSpace[command.Name];
            return HaveSameHandler(existing.OnExecute, command.OnExecute)
                ? CommandAddResult.AlreadyExists
                : CommandAddResult.Conflict;
        }

        public bool TryRemoveCommand(string commandSpace, string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandSpace) || string.IsNullOrWhiteSpace(commandName))
                return false;

            if (!_commands.TryGetValue(commandSpace, out var commandMap))
                return false;

            var removed = commandMap.Remove(commandName);
            if (!removed)
                return false;

            if (commandMap.Count == 0)
                _commands.Remove(commandSpace);

            return true;
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
            if (string.IsNullOrWhiteSpace(fullCommand))
            {
                _dialogService.ShowMessageBox($"Cannot execute command {fullCommand}.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var separatorIndex = fullCommand.IndexOf('.');
            if (separatorIndex <= 0 || separatorIndex >= fullCommand.Length - 1)
            {
                _dialogService.ShowMessageBox($"Cannot execute command {fullCommand}.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var commandSpace = fullCommand[..separatorIndex];
            var commandName = fullCommand[(separatorIndex + 1)..];
            Execute(commandSpace, commandName, args);
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
