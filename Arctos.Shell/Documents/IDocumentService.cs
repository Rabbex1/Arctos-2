using Arctos.Messenger;
using Arctos.Shell.Dialogs;
using Arctos.Shell.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Shell.Documents
{
    public interface IDocumentService
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        DocumentVM? ActiveDocument { get; set; }
        ObservableCollection<DocumentVM> Documents { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        List<T> DocumentsOfType<T>() where T : DocumentVM;
        bool SwitchTo(string name);
        bool SwitchToType(Type type);
        void AddDocument(DocumentVM document, bool makeCurrent = true);
        DocumentVM? CreateDocument<T>(string? title, bool makeCurrent = true) where T : DocumentVM;
        DocumentVM? CreateDocument(Type type, string title, bool makeCurrent = true);
        void RemoveDocument(DocumentVM document);
        void CloseDocument(DocumentVM document);
        void CloseAllDocuments(DocumentVM? except = null);
        void SaveDocument(PersistentDocumentVM document);
        void SaveDocuments(IEnumerable<PersistentDocumentVM> documents);
        bool SaveDocumentsOnExit();
        void SaveAllDocuments();
        void RegisterDocumentType(Type docType);


        #endregion Methods
    }
}