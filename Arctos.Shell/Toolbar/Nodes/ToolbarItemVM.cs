using Arctos.Shell.Commands;
using Arctos.Shell.Menu;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arctos.Shell.Toolbar;

namespace Arctos.Shell.Toolbar
{
    public partial class ToolbarItemVM : ChildVM<ToolbarVM>, IToolbarNodeVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Id
        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        #endregion

        public Action? OnExecute { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public ToolbarItemVM(ToolbarVM? parent) 
            : base(parent)
        {
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        public void Execute()
        {
            OnExecute?.Invoke();
        }

        #endregion Commands

    }
}
