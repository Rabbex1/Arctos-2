using Arctos.Wpf.DragDrop;
using Ursa.Wpf.ViewModels;

namespace Arctos.Wpf.ViewModels
{
    public class TreeViewChildHostVM(TreeViewParentHostVM parent, BaseVM content)
        : ChildVM<TreeViewParentHostVM>(parent), ITreeViewItemHostVM, IDragable
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

        public BaseVM Content { get; } = content;

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

        public Type DataType
        {
            get
            {
                if (Content is IDragable dragableContent)
                    return dragableContent.DataType;
                return Content.GetType();
            }
        }

        public object DataObject
        {
            get
            {
                if (Content is IDragable dragableContent)
                    return dragableContent.DataObject;
                return Content;
            }
        }

        public void Remove(object i)
        {
            //Do nothing
        }

        #endregion  IDragable Members

    }
}
