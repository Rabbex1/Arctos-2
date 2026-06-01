using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Represents the live preview state for the selected node definition in library-editing mode.
/// </summary>
public partial class LibraryCanvasVM : ObservableObject
{
    #region Fields

    private readonly NodesSessionVM _session;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the selected node definition from the library session.
    /// </summary>
    public NodeDefinitionVM? SelectedDefinition => _session.SelectedDefinition;

    /// <summary>
    /// Gets or sets the preview mode used by the library canvas.
    /// </summary>
    [ObservableProperty]
    private NodePreviewMode _previewMode = NodePreviewMode.Collapsed;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new library canvas VM.
    /// </summary>
    /// <param name="session">The owning nodes session.</param>
    public LibraryCanvasVM(NodesSessionVM session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _session.PropertyChanged += SessionPropertyChanged;
    }

    #endregion Constructors

    #region Private Methods

    private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(NodesSessionVM.SelectedDefinition))
            return;

        OnPropertyChanged(nameof(SelectedDefinition));
    }

    #endregion Private Methods
}
