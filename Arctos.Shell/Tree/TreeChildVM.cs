using Arctos.Shell.Menu;

namespace Arctos.Shell
{
    public class TreeChildVM : ChildVM<TreeParentVM>, ITreeItemVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        #endregion

        #region ContextMenu
        private IMenuNodeVM? _contextMenu;
        public IMenuNodeVM? ContextMenu
        {
            get => _contextMenu;
            set => SetProperty(ref _contextMenu, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        protected TreeChildVM(TreeParentVM parent) 
            : base(parent)
        {
        }

        #endregion Constructors


    }
}
