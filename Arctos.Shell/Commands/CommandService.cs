using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Shell.Commands
{
    public class CommandService : ICommandService
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Dictionary<string, Dictionary<string, Command>> _commands = new();

        #endregion Fields

        public void AddCommand(Command command)
        {
            if (!_commands.TryGetValue(command.CommandSpace, out var commandSpace))
            {
                commandSpace = new Dictionary<string, Command>();
                _commands.Add(command.CommandSpace, commandSpace);
            }

            if (!commandSpace.TryAdd(command.Name, command))
            {
                throw new CommandServiceException($"Cannot add command: {command.CommandSpace}:{command.Name}");
            }
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
            var parts = fullCommand.Split(".");
            if(parts.Length != 2)
                throw new CommandServiceException($"invalid command: {fullCommand}");
            Execute(parts[0], parts[1], args);
        }
    }
}
