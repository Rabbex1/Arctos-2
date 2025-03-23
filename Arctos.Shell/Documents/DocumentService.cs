using Arctos.Shell.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arctos.Shell.Dialogs;
using Arctos.Wpf.Dialogs;
using System.Collections.ObjectModel;
using Arctos.Messenger;
using Arctos.Wpf.AvalonDock;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Interop;
using AvalonDock.Layout.Serialization;
using System.Windows.Forms;
using DialogResult = Arctos.Wpf.Dialogs.DialogResult;

namespace Arctos.Shell.Documents
{
    public partial class DocumentService : ObservableObject, 
        IDocumentService, 
        IRecipient<DocumentMessage>,
        IRecipient<AvalonDockMessage>,
        IRecipient<AppMessage>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly IServiceProvider _services;
        private readonly Dictionary<string, Type> _registeredDocumentsTypes = new();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region ActiveDocument
        private DocumentVM? _activeDocument;
        public DocumentVM? ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (_activeDocument == value)
                    return;
                if (_activeDocument != null)
                    _activeDocument.PropertyChanged -= OnActiveDocumentPropertyChanged;
                _activeDocument = value;
                if (_activeDocument != null)
                    _activeDocument.PropertyChanged += OnActiveDocumentPropertyChanged;
                SetProperty(ref _activeDocument, value);
                WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.ActiveDocumentChanged, new object?[] { ActiveDocument }));
            }
        }

        #endregion

        #region Documents
        private ObservableCollection<DocumentVM> _documents = new();
        public ObservableCollection<DocumentVM> Documents
        {
            get => _documents;
            set => SetProperty(ref _documents, value);
        }
        #endregion DocumentWindows

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public DocumentService(IServiceProvider services)
        {
            _services = services;

            WeakReferenceMessenger.Default.Register<DocumentMessage>(this);
            WeakReferenceMessenger.Default.Register<AvalonDockMessage>(this);
            WeakReferenceMessenger.Default.Register<AppMessage>(this);
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public List<T> DocumentsOfType<T>()
          where T : DocumentVM
        {
            var docs = new List<T>();
            foreach (var doc in Documents)
                if (doc is T docT)
                    docs.Add(docT);
            return docs;
        }

        public bool SwitchTo(string name)
        {
            var document = Documents.FirstOrDefault(d => d.DisplayName == name);
            if (document == null)
                return false;
            document.IsActive = true;
            document.IsSelected = true;
            return true;
        }

        public bool SwitchToType(Type type)
        {
            var document = Documents.FirstOrDefault(d => d.GetType() == type);
            if (document == null)
                return false;
            document.IsActive = true;
            document.IsSelected = true;
            return true;
        }

        public void AddDocument(DocumentVM document, bool makeCurrent = true)
        {
            if (!Documents.Contains(document))
                Documents.Add(document);
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.DocumentAdded, new object[] { document }));
            if (makeCurrent)
            {
                document.IsActive = true;
                document.IsSelected = true;
                ActiveDocument = document;
            }
            document.PropertyChanged += DocumentPropertyChanged;
        }

        public DocumentVM? CreateDocument<T>(string? title, bool makeCurrent = true) 
            where T : DocumentVM
        {
            var document = _services.GetRequiredService<T>();
            document.DisplayName = title ?? "Unknown";
            AddDocument(document, makeCurrent);
            return document;
        }

        public DocumentVM? CreateDocument(Type type, string title, bool makeCurrent = true)
        {
            if (!typeof(DocumentVM).IsAssignableFrom(type))
                throw new ArctosDocumentException($"Cannot create documentVM of type:{type}");
            var document = _services.GetRequiredService(type) as DocumentVM 
                           ?? throw new ArctosDocumentException($"Could not resolve documentVM of type:{type}");
            document.DisplayName = title;
            AddDocument(document, makeCurrent);
            return document;
        }

        public void RemoveDocument(DocumentVM? document)
        {
            if (document == null)
                return;
            document.IsActive = false;
            document.IsSelected = false;
            if (document == ActiveDocument)
                ActiveDocument = null;
            Documents.Remove(document);
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.DocumentRemoved, new object[] { document }));
            document.PropertyChanged -= DocumentPropertyChanged;
        }

        public void CloseDocument(DocumentVM document)
        {
            document.Closing();

            if (document is not PersistentDocumentVM {IsChanged: true} pDoc)
            {
                RemoveDocument(document);
                return;
            }

            var saveChangesDialog = new SaveChangesDialogVM
            {
                Documents = new List<PersistentDocumentVM?> {pDoc},
                CloseIfSaved = Dialogs.CloseDocument.Yes,
                CloseIfNotSaved = Dialogs.CloseDocument.Yes
            };
            saveChangesDialog.DialogClosing += OnSaveChangesClosing;
            WeakReferenceMessenger.Default.Send(new AppMessage(this, AppAction.ShowDialog, new object[] {saveChangesDialog}));
        }

        public void CloseAllDocuments(DocumentVM? except = null)
        {
            var documentsToClose = new List<DocumentVM>();
            var changedDocs = new List<PersistentDocumentVM?>();

            foreach (var document in Documents)
            {
                if (document == except)
                    continue;

                //If document is not persistent or unchanged add it to the close list
                if (!(document is PersistentDocumentVM pDoc) || !pDoc.IsChanged)
                {
                    documentsToClose.Add(document);
                    continue;
                }
                //else add it to the changed list
                changedDocs.Add(pDoc);
            }

            //close the non-persistent and unchanged documents
            foreach (var doc in documentsToClose)
                Documents.Remove(doc);

            //if there are no changed docs return
            if (changedDocs.Count == 0)
                return;

            //now query whether to save the changed docs
            var saveChangesDialog = new SaveChangesDialogVM
            {
                Documents = changedDocs,
                //because this is a close request
                CloseIfSaved = Dialogs.CloseDocument.Yes,
                CloseIfNotSaved = Dialogs.CloseDocument.Yes
            };
            saveChangesDialog.DialogClosing += OnSaveChangesClosing;
            WeakReferenceMessenger.Default.Send(new AppMessage(this, AppAction.ShowDialog, new object[] { saveChangesDialog }));
        }

        public void SaveDocument(PersistentDocumentVM? document)
        {
            //because we don't know which save method is implemented call them both
            document?.Save();
            var _ = document?.SaveAsync();
        }

        public void SaveDocuments(IEnumerable<PersistentDocumentVM> documents)
        {
            foreach (var document in documents)
            {
                SaveDocument(document);
            }
        }

        public bool SaveDocumentsOnExit()
        {
            var changedDocs = new List<PersistentDocumentVM?>();

            //make a list of changed docs
            foreach (var document in Documents)
            {
                if (document is PersistentDocumentVM pDoc && pDoc.IsChanged)
                    changedDocs.Add(pDoc);
            }

            //if there are no changed docs return
            if (changedDocs.Count == 0)
            {
                return false;
            }

            //now query whether to save the changed docs
            var saveChangesDialog = new SaveChangesDialogVM
            {
                Documents = changedDocs,
                //because we are exiting the app leave the docs open regardless if they are saved or not.
                CloseIfSaved = Dialogs.CloseDocument.No,
                CloseIfNotSaved = Dialogs.CloseDocument.No
            };
            saveChangesDialog.DialogClosing += OnSaveChangesClosing;
            WeakReferenceMessenger.Default.Send(new AppMessage(this, AppAction.ShowDialog, new object[] { saveChangesDialog }));
            return saveChangesDialog.WasCanceled;
        }

        public void SaveAllDocuments()
        {
            var changedDocs = new List<PersistentDocumentVM>();

            //make a list of changed docs
            foreach (var document in Documents)
            {
                if (document is PersistentDocumentVM pDoc && pDoc.IsChanged)
                    changedDocs.Add(pDoc);
            }
            //now save them
            SaveDocuments(changedDocs);
        }

        public void RegisterDocumentType(Type docType)
        {
            if (!docType.IsSubclassOf(typeof(DocumentVM)))
                return;

            var key = docType.FullName;
            if (key == null || _registeredDocumentsTypes.ContainsKey(key))
                return;
            _registeredDocumentsTypes.Add(key, docType);
        }

        private static Type? GetTypeByName(string name)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .Reverse()
                    .Select(assembly => assembly.GetType(name))
                    .FirstOrDefault(t => t != null)
                // Safely delete the following part
                // if you do not want fall back to first partial result
                ??
                AppDomain.CurrentDomain.GetAssemblies()
                    .Reverse()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => t.Name.Contains(name));
        }

        private DocumentVM? ResolveDocument(string contentId, string displayName)
        {
            var content = DockingPaneVM.GetContentComponents(contentId);
            if (!content.TryGetValue("Class", out var value))
                return null;
            var documentClassName = value;
            if (!_registeredDocumentsTypes.ContainsKey(documentClassName))
                return null;

            var type = GetTypeByName(documentClassName);
            if (type == null)
                return null;
            var document = CreateDocument(type, displayName, false);
            if (document == null)
                return null;


            bool loaded;
            bool loadedAsync;
            switch (document)
            {
                case FileDocumentVM fileDoc:
                    if (content.TryGetValue("FilePath", out var path))
                    {
                        //since we don't know which version of 'Load()' the document implements run both.
                        loaded = fileDoc.Load(path);
                        var fDocTask = fileDoc.LoadAsync(path);
                        if (fDocTask == null)
                            throw new ArgumentNullException(nameof(fDocTask));
                        loadedAsync = fDocTask.GetAwaiter().GetResult();

                        if(!loaded && !loadedAsync)
                            RemoveDocument(fileDoc);
                    }
                    break;
                case PersistentDocumentVM persistentDoc:
                    loaded = persistentDoc.Load();
                    var pDockTask = persistentDoc.LoadAsync();
                    if (pDockTask == null)
                        throw new ArgumentNullException(nameof(pDockTask));
                    loadedAsync = pDockTask.GetAwaiter().GetResult();

                    if (!loaded && !loadedAsync)
                    {
                        document = null;
                        RemoveDocument(persistentDoc);
                    }
                    break;
                    //case DatabaseDocumentVM dbDoc:
                    //    //since we don't know which version of 'Load()' the document implements run both.
                    //    loaded = dbDoc.Load();
                    //    var task1 = dbDoc.LoadAsync();
                    //    if (task1 == null)
                    //        throw new ArgumentNullException(nameof(task1));
                    //    loadedAsync = task1.GetAwaiter().GetResult();
                    //    dbDoc.IsChanged = false;
                    //    break;

            }

            return document;
        }
        
        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnActiveDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayName")
                WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.ActiveDocumentNameChanged, new object?[] { ActiveDocument }));
        }

        private void DocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DocumentVM doc)
                return;
            if (e.PropertyName != "IsActive" || !doc.IsActive || ActiveDocument == sender)
                return;
            ActiveDocument = doc;
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.ActiveDocumentChanged, new object?[] { ActiveDocument }));
        }

        public void OnSaveChangesClosing(object? sender, CloseDialogArgs args)
        {
            if (!(sender is SaveChangesDialogVM saveChangesDialog))
                return;
            saveChangesDialog.DialogClosing -= OnSaveChangesClosing;

            switch (args.Result)
            {
                case DialogResult.Yes:
                    if (saveChangesDialog.Documents != null)
                        foreach (var document in saveChangesDialog.Documents)
                        {
                            SaveDocument(document);
                            if (document != null && saveChangesDialog.CloseIfSaved == Dialogs.CloseDocument.Yes && !document.IsChanged)
                                RemoveDocument(document);
                        }

                    break;
                case DialogResult.No:
                    if (saveChangesDialog.Documents != null)
                        foreach (var document in saveChangesDialog.Documents)
                        {
                            if (saveChangesDialog.CloseIfNotSaved == Dialogs.CloseDocument.Yes)
                                RemoveDocument(document);
                        }

                    break;
                case DialogResult.Cancel:
                    return;
                case DialogResult.Ok:
                    return;
                case DialogResult.Close:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //todo: this should always be empty
            //foreach (var doc in _documentsToClose)
            //  Documents.Remove(doc);
            //_documentsToClose.Clear();
        }


        #endregion Event Handlers


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Message Handlers

        public void Receive(DocumentMessage message)
        {
            switch (message)
            {
                case {Action: DocumentAction.Add}:
                    if (message.Params?[0] is not DocumentVM document)
                        throw new ArctosDocumentException($"Cannot add document. Param[0] must be 'DocumentVM'");
                    AddDocument(document);
                    break;
            }
        }

        public void Receive(AvalonDockMessage message)
        {
            if (message.Action != AvalonDockAction.DocumentPaneLoaded)
                return;

            if (!(message.Params?[0] is LayoutSerializationCallbackEventArgs loadEventArgs))
                return;

            if (ResolveDocument(loadEventArgs.Model.ContentId, loadEventArgs.Model.Title) is not { } document)
            {
                message.Cancel = true;
                return;
            }

            loadEventArgs.Content = document;
        }

        public void Receive(AppMessage message)
        {
            if (message.Action == AppAction.RequestCloseApp)
            {
                //if a request to save changed docs is cancelled this will stop the app closing
                message.Cancel = SaveDocumentsOnExit();
            }
        }

        #endregion Message Handlers
    }
}
