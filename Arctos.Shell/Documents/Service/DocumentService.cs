using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Arctos.Messenger;
using Arctos.Shell.Dialogs;
using Arctos.Shell.Messages;
using Arctos.Wpf.AvalonDock;
using Arctos.Wpf.Dialogs;
using AvalonDock.Layout.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Shell.Documents
{
    public partial class DocumentService : ObservableObject, IDocumentService,
        IRecipient<DocumentMessage>, IRecipient<AvalonDockMessage>, IRecipient<AppMessage>
    {
        private readonly DocumentFactoryRegistry _factoryRegistry = null!;
        private readonly IDialogService _dialogService = null!;

        private IDocumentVM? _activeDocument;

         //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public DocumentService()
        {
        }

        #endregion Constructor

        public IDocumentVM? ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (_activeDocument == value) return;
                if (_activeDocument != null)
                    _activeDocument.PropertyChanged -= OnActiveDocumentPropertyChanged;

                _activeDocument = value;

                if (_activeDocument != null)
                    _activeDocument.PropertyChanged += OnActiveDocumentPropertyChanged;

                SetProperty(ref _activeDocument, value);
                WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.ActiveDocumentChanged,
                    [ActiveDocument]));
            }
        }

        public ObservableCollection<IDocumentVM> Documents { get; } = [];

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public DocumentService(IDialogService dialogService, DocumentFactoryRegistry factoryRegistry)
        {
            _dialogService = dialogService;
            _factoryRegistry = factoryRegistry;

            WeakReferenceMessenger.Default.Register<DocumentMessage>(this);
            WeakReferenceMessenger.Default.Register<AvalonDockMessage>(this);
            WeakReferenceMessenger.Default.Register<AppMessage>(this);
        }

        #endregion Constructors

        public void AddDocument(IDocumentVM document, bool makeCurrent = true)
        {
            if (!Documents.Contains(document))
                Documents.Add(document);
            if (makeCurrent)
            {
                document.IsActive = true;
                document.IsSelected = true;
                ActiveDocument = document;
            }
            document.PropertyChanged += DocumentPropertyChanged;
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.DocumentAdded, [document]));
        }

        public void RemoveDocument(IDocumentVM? document)
        {
            if (document == null) return;
            document.IsActive = false;
            document.IsSelected = false;
            if (document == ActiveDocument)
                ActiveDocument = null;
            document.PropertyChanged -= DocumentPropertyChanged;
            Documents.Remove(document);
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.DocumentRemoved, [document]));
        }

        public void CloseDocument(IDocumentVM document)
        {
            if (document is not IPersistentDocumentVM pDoc || !pDoc.IsChanged)
            {
                RemoveDocument(document);
                return;
            }

            var dialog = new SaveChangesDialogVM
            {
                Documents = [pDoc],
                CloseIfSaved = Dialogs.CloseDocument.Yes,
                CloseIfNotSaved = Dialogs.CloseDocument.Yes
            };
            dialog.DialogClosing += OnSaveChangesClosing;
            WeakReferenceMessenger.Default.Send(new AppMessage(this, AppAction.ShowDialog, [dialog]));
        }

        public void CloseAllDocuments(IDocumentVM? except = null)
        {
            var toClose = Documents.Where(d => d != except).ToList();
            foreach (var doc in toClose)
                CloseDocument(doc);
        }

        public IDocumentVM? OpenFile(string path)
        {
            var ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            var title = Path.GetFileName(path);

            var metadata = new DocumentMetadata
            {
                DocumentType = "File",
                Source = path,
                Title = title,
                Properties = new() { { "extension", ext } }
            };

            try
            {
                return OpenFromMetadata(metadata);
            }
            catch (FileNotFoundException e)
            {
                _dialogService.ShowMessageBox($"Failed to open document: {e.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);        
                Console.WriteLine(e);
                return null;
            }
        }

        public IDocumentVM? OpenFromMetadata(DocumentMetadata metadata)
        {
            var factory = _factoryRegistry.GetMatchingFactories(metadata).FirstOrDefault();
            if(factory == null)
                throw new FileNotFoundException($"\nNo document factory found for file: {metadata.Source}");
            var document = factory?.Create(metadata);
            if (document is not IFileDocumentVM fileDoc)
                return null;

            AddDocument(document); // Add immediately — even before load

            // Background async load
            _ = Task.Run(async () =>
            {
                try
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    var loaded = fileDoc.Load(metadata.Source);
                    if (!loaded)
                        loaded = await fileDoc.LoadAsync(metadata.Source);

                    if (!loaded)
                    {
                        Debug.WriteLine("Document failed to load: " + metadata.Source);
                        RemoveDocument(document); // Optionally remove failed doc
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading document '{metadata.Source}': {ex.Message}");
                    RemoveDocument(document);
                }
            });

            return document;
        }

        public List<T> DocumentsOfType<T>() where T : IDocumentVM => Documents.OfType<T>().ToList();

        public bool SwitchTo(string name)
        {
            var doc = Documents.FirstOrDefault(d => d.DisplayName == name);
            if (doc == null) return false;
            doc.IsActive = doc.IsSelected = true;
            return true;
        }

        public bool SwitchToType(Type type)
        {
            var doc = Documents.FirstOrDefault(d => d.GetType() == type);
            if (doc == null) return false;
            doc.IsActive = doc.IsSelected = true;
            return true;
        }

        public void SaveDocument(IPersistentDocumentVM? doc)
        {
            doc?.Save();
            _ = doc?.SaveAsync();
        }

        public void SaveDocuments(IEnumerable<IPersistentDocumentVM> documents)
        {
            foreach (var document in documents)
            {
                SaveDocument(document);
            }
        }

        public void SaveAllDocuments()
        {
            foreach (var doc in Documents.OfType<IPersistentDocumentVM>().Where(d => d.IsChanged))
                SaveDocument(doc);
        }

        public bool SaveDocumentsOnExit()
        {
            var changed = Documents.OfType<IPersistentDocumentVM>().Where(d => d.IsChanged).ToList();
            if (!changed.Any()) return false;

            var dialog = new SaveChangesDialogVM
            {
                Documents = changed,
                CloseIfSaved = Dialogs.CloseDocument.No,
                CloseIfNotSaved = Dialogs.CloseDocument.No
            };
            dialog.DialogClosing += OnSaveChangesClosing;
            WeakReferenceMessenger.Default.Send(new AppMessage(this, AppAction.ShowDialog, [dialog]));
            return dialog.WasCanceled;
        }

        public void Receive(DocumentMessage message)
        {
            if (message.Action == DocumentAction.Add && message.Params?[0] is IDocumentVM doc)
                AddDocument(doc);
        }

        public void Receive(AvalonDockMessage message)
        {
            if (message.Action != AvalonDockAction.DocumentPaneLoaded) return;

            if (message.Params?[0] is not LayoutSerializationCallbackEventArgs args)
                return;

            var metadata = DocumentMetadataSerializer.Decode(args.Model.ContentId);
            if (metadata == null)
            {
                message.Cancel = true;
                return;
            }

            var doc = OpenFromMetadata(metadata);
            if (doc == null)
            {
                message.Cancel = true;
                return;
            }

            args.Content = doc;
        }

        public void Receive(AppMessage message)
        {
            if (message.Action == AppAction.RequestCloseApp)
                message.Cancel = SaveDocumentsOnExit();
        }

        private void DocumentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not IDocumentVM doc) return;
            if (e.PropertyName != nameof(IDocumentVM.IsActive) || !doc.IsActive || ActiveDocument == doc)
                return;
            ActiveDocument = doc;
        }

        private void OnActiveDocumentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IDocumentVM.DisplayName))
                WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.ActiveDocumentNameChanged,
                    [ActiveDocument]));
        }

        private void OnSaveChangesClosing(object? sender, CloseDialogArgs args)
        {
            if (sender is not SaveChangesDialogVM dialog) return;
            dialog.DialogClosing -= OnSaveChangesClosing;

            switch (args.Result)
            {
                case DialogResult.Yes:
                    foreach (var doc in dialog.Documents ?? Enumerable.Empty<IPersistentDocumentVM?>())
                    {
                        SaveDocument(doc);
                        if (doc != null && dialog.CloseIfSaved == Dialogs.CloseDocument.Yes && !doc.IsChanged)
                            RemoveDocument(doc);
                    }
                    break;
                case DialogResult.No:
                    foreach (var doc in dialog.Documents ?? Enumerable.Empty<IPersistentDocumentVM?>())
                    {
                        if (dialog.CloseIfNotSaved == Dialogs.CloseDocument.Yes)
                            RemoveDocument(doc);
                    }
                    break;
            }
        }
    }
}
