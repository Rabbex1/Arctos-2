using Arctos.Shell.ToolWindows;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Provides the shell tool-window VM for document settings and node library editing.
/// </summary>
public partial class NodesDocumentToolVM : ToolWindowVM
{
    #region Properties

    /// <summary>
    /// Gets the shared nodes session.
    /// </summary>
    public NodesSessionVM Session { get; }

    /// <summary>
    /// Gets the document toolbox VM.
    /// </summary>
    public NodeDocumentVM Document { get; }

    /// <summary>
    /// Gets a value indicating whether the tool should be shown for the current mode.
    /// </summary>
    public bool ShouldShow => !Session.IsRunMode;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new nodes document tool VM.
    /// </summary>
    /// <param name="session">The shared nodes session.</param>
    /// <param name="document">The document toolbox VM.</param>
    public NodesDocumentToolVM(NodesSessionVM session, NodeDocumentVM document)
        : base("Node Document")
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Document = document ?? throw new ArgumentNullException(nameof(document));
        Session.PropertyChanged += SessionPropertyChanged;
    }

    #endregion Constructors

    #region Private Methods

    private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(NodesSessionVM.CurrentMode))
            return;

        OnPropertyChanged(nameof(ShouldShow));
    }

    #endregion Private Methods
}
