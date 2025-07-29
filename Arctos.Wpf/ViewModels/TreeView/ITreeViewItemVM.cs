using Ursa.Wpf.ViewModels;

namespace Arctos.Wpf.ViewModels
{

    public interface ITreeViewItemVM : IBaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        TreeViewParentVM? Parent { get; set; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        bool IsEnabled { get; set; }

        #endregion Properties

    }
}
