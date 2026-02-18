using Arctos.Messenger;
using Arctos.Wpf.AvalonDock;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Shell.Documents
{
    public abstract partial class DocumentVM()
        : DocumentPaneVM("New Document"), IDocumentVM
    {
        private DocumentMetadata? _metadata;

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public DocumentMetadata? Metadata
        {
            get => _metadata;
            set
            {
                if (!SetProperty(ref _metadata, value))
                    return;
                OnPropertyChanged(nameof(ContentId));
            }
        }

        public override string ContentId => DocumentMetadataSerializer.Encode(EnsureContentMetadata());

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public virtual void Closing()
        {
        }

        protected internal virtual DocumentMetadata CreateContentMetadata()
        {
            var metadata = new DocumentMetadata
            {
                DocumentType = "Document",
                Source = string.Empty,
                Title = string.Empty
            };

            var className = GetType().FullName;
            if (!string.IsNullOrWhiteSpace(className))
                metadata.Properties["class"] = className;

            return metadata;
        }

        private DocumentMetadata EnsureContentMetadata()
        {
            Metadata ??= CreateContentMetadata();
            return Metadata;
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        public void Close()
        {
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.Close, [this]));
        }

        [RelayCommand]
        public void CloseAll()
        {
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.CloseAll));
        }

        [RelayCommand]
        public void CloseAllButThis()
        {
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.CloseAllButThis, [this]));
        }

        #endregion Commands

    }
}

