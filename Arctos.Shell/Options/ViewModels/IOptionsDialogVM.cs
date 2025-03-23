using Arctos.Shell.Options.Selectors;
using Arctos.Utils;
using System.Collections.ObjectModel;
using Arctos.Wpf.Dialogs;

namespace Arctos.Shell.Options
{
    public interface IOptionsDialogVM : IDialogBaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public double FontSize { get; set; }
        public ObservableCollection<ITreeItemVM> Tree { get; }
        public List<TreeParentVM> List { get; }
        public object? SelectedPage { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public OptionsFormVM? AddDialogPage(string displayName, OptionsPage? optionsPage, Type? vmType, Type viewType,
            string? parentId, string pageId, Guid ownerId, bool expandParent = false);
        public OptionsFormVM? AddDialogPagePending(string displayName, OptionsPage optionsPage, Type vmType,
            Type viewType, string parentId, string pageId, Guid ownerId, bool expandParent = false);
        public void RemoveDialogPage(string? parentId, string? pageId, Guid ownerId);
        public void RemoveDialogPagePending(string parentId, string pageId, Guid ownerId);

        #endregion Methods
    }
}
