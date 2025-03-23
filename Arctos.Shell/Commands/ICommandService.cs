using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Shell.Commands
{
    public interface ICommandService
    {
        public void AddCommand(Command command);
        public void Execute(string commandSpace, string command, ExecuteCommandArgs args);
        public void Execute(string fullCommand, ExecuteCommandArgs args);
    }
}
