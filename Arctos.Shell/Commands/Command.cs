using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Shell.Commands
{
    public class Command
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties

        public string CommandSpace { get; }
        public string Name { get; }
        public Action<Command, ExecuteCommandArgs> OnExecute { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public Command(string commandSpace, string name, Action<Command, ExecuteCommandArgs> onExecute)
        {
            CommandSpace = commandSpace;
            Name = name;
            OnExecute = onExecute;
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void Execute(ExecuteCommandArgs args)
        {
            OnExecute.Invoke(this, args);
        }

        #endregion Methods
    }
}
