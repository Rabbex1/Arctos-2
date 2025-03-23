using System.Windows.Input;
using Arctos.Wpf.AvalonDock;
using Arctos.Wpf.Utilities;
using CommunityToolkit.Mvvm.Input;

namespace Arctos.Shell.Documents
{
    public abstract partial class DocumentVM : DocumentPaneVM, IDisposable
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected readonly IDocumentService DocumentService;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        protected DocumentVM(IDocumentService documentService)
          : base("New Document")
        {
            DocumentService = documentService;
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public virtual void Closing()
        {
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        public void Close()
        {
            DocumentService.CloseDocument(this);
        }

        [RelayCommand]
        public void CloseAll()
        {
            DocumentService.CloseAllDocuments(null);
        }

        [RelayCommand]
        public void CloseAllButThis()
        {
            DocumentService.CloseAllDocuments(this);
        }

        #endregion Commands

        public virtual void Dispose()
        {
        }
    }
}
