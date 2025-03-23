using Arctos.Shell.Menu;

namespace Arctos.Shell
{
    public interface ITreeItemVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        string? DisplayName { get; set; }
        bool IsEnabled { get; set; }
        bool IsSelected { get; set; }
        object? Icon { get; set; }
        IMenuNodeVM? ContextMenu { get; set; }

        #endregion Properties
    }
}
