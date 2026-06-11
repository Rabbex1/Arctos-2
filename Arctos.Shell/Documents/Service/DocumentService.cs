using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Arctos.Messenger;
using Arctos.Shell.Dialogs;
using Arctos.Shell.Messages;
using Arctos.Wpf.AvalonDock;
using Arctos.Wpf.Dialogs;
using Arctos.Wpf.Helpers;
using AvalonDock.Layout.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Shell.Documents
{
    public partial class DocumentService : ObservableObject, IDocumentService,
        IRecipient<DocumentMessage>, IRecipient<AvalonDockMessage>, IRecipient<AppMessage>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly DocumentFactoryRegistry _factoryRegistry = null!;
        private readonly IDialogService _dialogService = null!;
        private readonly SemaphoreSlim _openFromMetadataGate = new(1, 1);
        private readonly ConcurrentDictionary<string, byte> _sourcesOpening = new(StringComparer.OrdinalIgnoreCase);

        #endregion Fields

        private IDocumentVM? _activeDocument;

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ObservableCollection<IDocumentVM> Documents { get; } = [];

        #endregion Properties


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

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void AddDocument(IDocumentVM document, bool makeCurrent = true)
        {
            EnsureDocumentMetadata(document);
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
            _dialogService.Show(dialog);
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
            string? sourceKey = null;
            if (!string.IsNullOrWhiteSpace(metadata.Source))
            {
                sourceKey = Path.GetFullPath(metadata.Source);
                if (!_sourcesOpening.TryAdd(sourceKey, 0))
                {
                    Debug.WriteLine($"Skipping duplicate open request for '{metadata.Source}'.");
                    return null;
                }
            }

            var factory = _factoryRegistry.GetMatchingFactories(metadata).FirstOrDefault();
            if(factory == null)
            {
                if (sourceKey != null)
                    _sourcesOpening.TryRemove(sourceKey, out _);
                return null;
                //throw new FileNotFoundException($"\nNo document factory found for file: {metadata.Source}");
            }
            var document = factory?.Create(metadata);
            if (document is not IFileDocumentVM fileDoc)
            {
                if (sourceKey != null)
                    _sourcesOpening.TryRemove(sourceKey, out _);
                return null;
            }

            document.Metadata = metadata;

            // Background async load
            _ = Task.Run(async () =>
            {
                await _openFromMetadataGate.WaitAsync();
                try
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    var loaded = fileDoc.Load(metadata.Source);
                    if (!loaded)
                        loaded = await fileDoc.LoadAsync(metadata.Source);

                    if (!loaded)
                    {
                        Debug.WriteLine("Document failed to load: " + metadata.Source);
                        await UiHelper.ThreadAsync(() =>
                        {
                            _dialogService.ShowMessageBox(
                                $"Failed to load document: {metadata.Source}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            CleanupFailedDocument(document);
                            RemoveDocument(document);
                        });
                        return;
                    }

                    await UiHelper.ThreadAsync(() => AddDocument(document)); 
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading document '{metadata.Source}': {ex.Message}");
                    await UiHelper.ThreadAsync(() =>
                    {
                        _dialogService.ShowMessageBox(
                            $"Failed to load document: {metadata.Source}\n\n{ex.GetAllMessages()}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        CleanupFailedDocument(document);
                        RemoveDocument(document);
                    });
                }
                finally
                {
                    if (sourceKey != null)
                        _sourcesOpening.TryRemove(sourceKey, out _);
                    _openFromMetadataGate.Release();
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
            if (doc is FileDocumentVM fileDocument)
                fileDocument.Metadata = fileDocument.CreateContentMetadata();
            else if (doc is IDocumentVM document)
                EnsureDocumentMetadata(document);

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
                CloseIfNotSaved = Dialogs.CloseDocument.No,
            };
            dialog.DialogClosing += OnSaveChangesClosing;
            _dialogService.Show(dialog);
            return dialog.WasCanceled;
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Private Methods
        
        private void DocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not IDocumentVM doc) return;
            if (e.PropertyName != nameof(IDocumentVM.IsActive) || !doc.IsActive || ActiveDocument == doc)
                return;
            ActiveDocument = doc;
        }

        private void OnActiveDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
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

        private static void EnsureDocumentMetadata(IDocumentVM document)
        {
            document.Metadata ??= document switch
            {
                FileDocumentVM fileDocument => fileDocument.CreateContentMetadata(),
                _ => new DocumentMetadata
                {
                    DocumentType = "Document",
                    Title = document.DisplayName ?? string.Empty,
                    Source = string.Empty,
                    Properties =
                    {
                        ["class"] = document.GetType().FullName ?? string.Empty
                    }
                }
            };
        }

        private static void CleanupFailedDocument(IDocumentVM document)
        {
            try
            {
                if (document is DocumentVM documentVm)
                    documentVm.Closing();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Document cleanup (Closing) failed: {ex.Message}");
            }

            try
            {
                if (document is IDisposable disposable)
                    disposable.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Document cleanup (Dispose) failed: {ex.Message}");
            }
        }

        #endregion Private Methods

        #endregion Methods
       
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Message Handlers

        public void Receive(DocumentMessage message)
        {
            if (message.Action == DocumentAction.Add && message.Params?[0] is IDocumentVM addDoc)
                AddDocument(addDoc);
            else if (message.Action == DocumentAction.Close && message.Params?[0] is IDocumentVM closeDoc)
                CloseDocument(closeDoc);
            else if (message.Action == DocumentAction.CloseAll)
                CloseAllDocuments();
            else if (message.Action == DocumentAction.CloseAllButThis && message.Params?[0] is IDocumentVM butDoc)
                CloseAllDocuments(butDoc);
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

            if(metadata.Source != "")
            {
                var doc = OpenFromMetadata(metadata);
                if (doc == null)
                {
                    message.Cancel = true;
                    return;
                }
            }
            // Document panes are driven by DocumentsSource. Prevent AvalonDock from
            // creating an extra pane from callback content while OpenFromMetadata
            // asynchronously loads and then adds the document to the collection.
            message.Cancel = true;
        }

        public void Receive(AppMessage message)
        {
            if (message.Action == AppAction.RequestCloseApp)
                message.Cancel = SaveDocumentsOnExit();
        }

        #endregion Message Handlers

    }
}
