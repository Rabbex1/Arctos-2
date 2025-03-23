using Arctos.Shell.Menu;

namespace Arctos.Shell
{
    public class TreeParentVM : ParentVM<ITreeItemVM, TreeParentVM>, ITreeItemVM
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

        #region IsExpanded
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
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

        public TreeParentVM(TreeParentVM? parent, bool isExpanded = false)
            : base(parent)
        {
            IsExpanded = isExpanded;
        }

        #endregion  Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override void AddChild(ITreeItemVM child)
        {
            Children.Add(child);
        }

        public override void RemoveChild(ITreeItemVM child)
        {
            Children.Remove(child);
        }

        public ITreeItemVM? FindSelectedNode()
        {
            if (IsSelected)
                return this;
            foreach (var child in Children)
            {
                if (child is TreeParentVM parent)
                {
                    if (parent.IsSelected)
                        return parent;
                    var selectedChild = parent.FindSelectedNode();
                    if (selectedChild == null)
                        continue;
                    return selectedChild;
                }
                if (child.IsSelected)
                    return child;
            }

            return null;
        }

        #endregion Methods

    }
}
