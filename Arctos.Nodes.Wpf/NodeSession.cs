using Arctos.Messenger;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Tracks active node-document UI state for the current shell session.
/// </summary>
public sealed class NodeSession : ObservableObject, IRecipient<DocumentMessage>
{
    #region Fields

    private INodeDocumentVM? _activeNodeDocument;

    #endregion Fields

    #region Properties

    public NodesManager NodesManager { get; }

    public INodeDocumentVM? ActiveNodeDocument
    {
        get => _activeNodeDocument;
        private set
        {
            if (!SetProperty(ref _activeNodeDocument, value))
                return;

            OnPropertyChanged(nameof(ActiveMapDocument));
            OnPropertyChanged(nameof(ActiveLibraryDocument));
            OnPropertyChanged(nameof(ActiveMap));
            OnPropertyChanged(nameof(ActiveLibrary));
        }
    }

    public INodeMapDocumentVM? ActiveMapDocument => ActiveNodeDocument as INodeMapDocumentVM;

    public INodeLibraryDocumentVM? ActiveLibraryDocument => ActiveNodeDocument as INodeLibraryDocumentVM;

    public NodeMap? ActiveMap => ActiveMapDocument?.NodeMap.Map;

    public NodeLibrary? ActiveLibrary => ActiveLibraryDocument?.NodeLibrary.Library;

    #endregion Properties

    #region Constructors

    public NodeSession(NodesManager nodesManager)
    {
        NodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
        WeakReferenceMessenger.Default.Register<DocumentMessage>(this);
    }

    #endregion Constructors

    #region Public Methods

    public void Receive(DocumentMessage message)
    {
        if (message.Action != DocumentAction.ActiveDocumentChanged)
            return;

        ActiveNodeDocument = message.Params?[0] as INodeDocumentVM;
    }

    #endregion Public Methods
}
