namespace Arctos.Wpf.ViewModels
{
    public interface ITreeViewItemHostVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        BaseVM Content { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        bool IsEnabled { get; set; }
        object? Icon { get; set; }

        #endregion Properties

    }
}