namespace Arctos.Wpf.ViewModels
{
    public class TreeViewParentVM : ParentVM<ITreeViewItemVM, TreeViewParentVM>, ITreeViewItemVM
    {

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
                if (!value)
                    return;
                if(Parent != null)
                    Parent.DescendantSelected(this);
                else
                    DescendantSelected(this);

            }
        }
        #endregion

        #region IsExpanded
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                SetProperty(ref _isExpanded, value);
                if (value && Parent != null)
                    Parent.IsExpanded = true;
            }
        }
        #endregion

        #endregion Properties


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public TreeViewParentVM(TreeViewParentVM? parent,bool isExpanded = false)
            : base(parent)
        {
            IsExpanded = isExpanded;
        }

        #endregion  Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Methods

        public override void AddChild(ITreeViewItemVM child)
        {
            Children.Add(child);
        }

        public override void RemoveChild(ITreeViewItemVM child)
        {
            Children.Remove(child);
        }

        public ITreeViewItemVM? FindSelectedNode()
        {
            if (IsSelected)
                return this;
            foreach (var child in Children)
            {
                if(child is TreeViewParentVM parent)
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

        public virtual void DescendantSelected(ITreeViewItemVM? selected)
        {
            // Pass up tree
            Parent?.DescendantSelected(selected);
        }

        public virtual void SetContextMenu(ContextMenuSet set)
        {
            if (set == ContextMenuSet.Self)
                return;
            foreach (var child in Children)
            {
                switch (child)
                {
                    case TreeViewChildVM childVM:
                        childVM.SetContextMenu();
                        break;
                    case TreeViewParentVM parentVM:
                        parentVM.SetContextMenu(set == ContextMenuSet.All ? ContextMenuSet.All : ContextMenuSet.Self);
                        break;
                }
            }
        }

        #endregion Methods

    }
}
