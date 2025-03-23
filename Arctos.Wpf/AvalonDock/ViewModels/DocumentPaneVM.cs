using System.Windows.Input;

namespace Arctos.Wpf.AvalonDock
{
    public abstract class DocumentPaneVM : DockingPaneVM, IDocumentPaneVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Consrtuctors

        protected DocumentPaneVM(string title)
          : base(title)
        { }

        #endregion Consrtuctors

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
