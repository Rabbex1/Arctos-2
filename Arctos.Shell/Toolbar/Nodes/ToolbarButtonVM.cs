using Arctos.Shell.Toolbar;

namespace Arctos.Shell.Toolbar
{
    public class ToolbarButtonVM : ToolbarItemVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

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

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public ToolbarButtonVM(ToolbarVM? parent) 
            : base(parent)
        {
        }

        #endregion Constructors
    }
}
