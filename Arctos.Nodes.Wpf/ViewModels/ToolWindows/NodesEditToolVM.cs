using Arctos.Shell.ToolWindows;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Provides the shell tool-window VM for generic node properties and palette editing.
/// </summary>
public partial class NodesEditToolVM : ToolWindowVM
{
    #region Properties

    /// <summary>
    /// Gets the shared nodes session.
    /// </summary>
    public NodesSessionVM Session { get; }

    /// <summary>
    /// Gets the properties toolbox VM.
    /// </summary>
    public NodePropertiesVM Properties { get; }

    /// <summary>
    /// Gets the palette toolbox VM.
    /// </summary>
    public NodePaletteVM Palette { get; }

    /// <summary>
    /// Gets a value indicating whether the tool should be shown for the current mode.
    /// </summary>
    public bool ShouldShow => !Session.IsRunMode;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new nodes edit tool VM.
    /// </summary>
    /// <param name="session">The shared nodes session.</param>
    /// <param name="properties">The properties toolbox VM.</param>
    /// <param name="palette">The palette toolbox VM.</param>
    public NodesEditToolVM(NodesSessionVM session, NodePropertiesVM properties, NodePaletteVM palette)
        : base("Node Edit Tools")
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
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
