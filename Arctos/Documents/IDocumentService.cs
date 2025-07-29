using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace
namespace Arctos.Shell.Documents
{
    public interface IDocumentService
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        IDocumentVM? ActiveDocument { get; }
        ObservableCollection<IDocumentVM> Documents { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        List<T> DocumentsOfType<T>() where T : IDocumentVM;
        bool SwitchTo(string name);
        bool SwitchToType(Type type);
        void AddDocument(IDocumentVM document, bool makeCurrent = true);
        void RemoveDocument(IDocumentVM document);
        void CloseDocument(IDocumentVM document);
        void CloseAllDocuments(IDocumentVM? except = null);
        IDocumentVM? OpenFile(string path);
        public IDocumentVM? OpenFromMetadata(DocumentMetadata metadata);
        void SaveDocument(IPersistentDocumentVM document);
        void SaveDocuments(IEnumerable<IPersistentDocumentVM> documents);
        bool SaveDocumentsOnExit();
        void SaveAllDocuments();

        #endregion Methods
    }
}