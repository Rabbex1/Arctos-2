using Arctos.Shell.Commands;
using CommunityToolkit.Mvvm.Input;

namespace Arctos.Shell.Menu
{
    public partial class MenuItemVM(MenuVM parent) : ChildVM<MenuVM>(parent), IMenuNodeVM
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

        #region Priority
        private int _priority;
        public int Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }
        #endregion

        #region IgtText
        private string? _igtText;
        public string? IgtText
        {
            get => _igtText;
            set => SetProperty(ref _igtText, value);
        }
        #endregion

        #region IsCheckable
        private bool _isCheckable;
        public bool IsCheckable
        {
            get => _isCheckable;
            set => SetProperty(ref _isCheckable, value);
        }
        #endregion

        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
        #endregion

        #region Command
        private string _command = string.Empty;
        public string Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }
        #endregion

        #region Params

        private object? _parameters;

        public object? Parameters
        {
            get => _parameters;
            set => SetProperty(ref _parameters, value);
        }

        #endregion

        public ICommandService? CommandService { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        public void Execute(object context)
        {
            
            CommandService?.Execute(Command, new ExecuteCommandArgs(context, _parameters));
        }

        #endregion Commands
    }
}