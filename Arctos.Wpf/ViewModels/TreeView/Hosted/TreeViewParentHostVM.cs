using Arctos;
using Arctos.Wpf.ViewModels;

namespace Ursa.Wpf.ViewModels
{
    public class TreeViewParentHostVM : ParentVM<ITreeViewItemHostVM, TreeViewParentHostVM>, ITreeViewItemHostVM
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
            set
            {
                SetProperty(ref _isExpanded, value);
                if (value && Parent != null)
                    Parent.IsExpanded = true;
            }
        }
        #endregion

        public BaseVM Content { get; }

        #endregion Properties


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public TreeViewParentHostVM(TreeViewParentHostVM parent, BaseVM content, bool isExpanded = false)
            : base(parent)
        {
            Content = content;
            IsExpanded = _isExpanded;
        }

        #endregion  Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Methods

        public override void AddChild(ITreeViewItemHostVM child)
        {
            Children.Add(child);
        }

        public override void RemoveChild(ITreeViewItemHostVM child)
        {
            Children.Remove(child);
        }

        public virtual void SetContextMenu(ContextMenuSet set)
        {
            if (set == ContextMenuSet.Self)
                return;
            foreach (var child in Children)
            {
                switch (child)
                {
                    case TreeViewChildHostVM childVM:
                        childVM.SetContextMenu();
                        break;
                    case TreeViewParentHostVM parentVM:
                        parentVM.SetContextMenu(set == ContextMenuSet.All ? ContextMenuSet.All : ContextMenuSet.Self);
                        break;
                }
            }
        }
        #endregion Methods



    }
}