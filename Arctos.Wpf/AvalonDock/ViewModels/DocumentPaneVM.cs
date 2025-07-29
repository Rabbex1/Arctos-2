using System.Windows.Input;

namespace Arctos.Wpf.AvalonDock
{
    public abstract class DocumentPaneVM(string title) 
        : DockingPaneVM(title), IDocumentPaneVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        #region CloseCommand
        // ReSharper disable once InconsistentNaming
        protected ICommand? _closeCommand;
        public virtual ICommand? CloseCommand => null;
        #endregion

        #endregion Commands

    }
}
