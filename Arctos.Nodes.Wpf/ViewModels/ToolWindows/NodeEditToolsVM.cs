using System.ComponentModel;
using Arctos.Shell.ToolWindows;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Minimal shell tool-window VM for node editing tools.
/// </summary>
public sealed class NodeEditToolsVM : ToolWindowVM
{
    #region Properties

    /// <summary>
    /// Gets the current node session.
    /// </summary>
    public NodeSession Session { get; }

    public INodeDocumentVM? ActiveNodeDocument => Session.ActiveNodeDocument;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node edit tool-window VM.
    /// </summary>
    /// <param name="session">The shared node session.</param>
    public NodeEditToolsVM(NodeSession session)
        : base("Node Edit Tools")
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Session.PropertyChanged += OnSessionPropertyChanged;
    }

    #endregion Constructors

    #region Private Methods

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeSession.ActiveNodeDocument))
            OnPropertyChanged(nameof(ActiveNodeDocument));
    }

    #endregion Private Methods
}
