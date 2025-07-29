using Arctos.Wpf.DragDrop;
using Ursa.Wpf.ViewModels;

namespace Arctos.Wpf.ViewModels
{
    public class TreeViewChildVM(TreeViewParentVM? parent)
        : ChildVM<TreeViewParentVM>(parent), ITreeViewItemVM, IDragable
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
                Parent?.DescendantSelected(this);
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
        #region Methods

        public virtual void SetContextMenu()
        {
            //Handel in inherited classes
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region IDragable Members

        public Type DataType => GetType();

        public object DataObject => this;

        public void Remove(object i)
        {
            //Do nothing
        }

        #endregion  IDragable Members
        

    }
}
