using System.ComponentModel;
using Arctos.Shell.ToolWindows;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Minimal shell tool-window VM for node structure tools.
/// </summary>
public sealed class NodeStructureToolsVM : ToolWindowVM
{
    #region Properties

    /// <summary>
    /// Gets the current node session.
    /// </summary>
    public NodeSession Session { get; }

    public INodeDocumentVM? ActiveNodeDocument => Session.ActiveNodeDocument;

    public INodeMapDocumentVM? ActiveMapDocument => Session.ActiveMapDocument;

    public INodeLibraryDocumentVM? ActiveLibraryDocument => Session.ActiveLibraryDocument;

    public NodeMapVM? ActiveNodeMap => ActiveMapDocument?.NodeMap;

    public NodeMapSettingsVM? ActiveMapSettings => ActiveNodeMap?.Settings;

    public NodeGraphVM? ActiveMapGraph => ActiveNodeMap?.Graph;

    public NodeLibraryVM? ActiveNodeLibrary => ActiveLibraryDocument?.NodeLibrary ?? ActiveNodeMap?.LocalLibrary;

    public bool IsMapDocument => ActiveNodeMap != null;

    public bool IsLibraryDocument => ActiveNodeMap == null && ActiveNodeLibrary != null;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node structure tool-window VM.
    /// </summary>
    /// <param name="session">The shared node session.</param>
    public NodeStructureToolsVM(NodeSession session)
        : base("Node Structure")
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Session.PropertyChanged += OnSessionPropertyChanged;
    }

    #endregion Constructors

    #region Private Methods

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeSession.ActiveNodeDocument))
        {
            OnPropertyChanged(nameof(ActiveNodeDocument));
            OnPropertyChanged(nameof(ActiveMapDocument));
            OnPropertyChanged(nameof(ActiveLibraryDocument));
            OnPropertyChanged(nameof(ActiveNodeMap));
            OnPropertyChanged(nameof(ActiveMapSettings));
            OnPropertyChanged(nameof(ActiveMapGraph));
            OnPropertyChanged(nameof(ActiveNodeLibrary));
            OnPropertyChanged(nameof(IsMapDocument));
            OnPropertyChanged(nameof(IsLibraryDocument));
        }
    }

    #endregion Private Methods
}
