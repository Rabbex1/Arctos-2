using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Provides a general-purpose properties surface for the current selection.
/// </summary>
public partial class NodePropertiesVM : ObservableObject
{
    #region Fields

    private readonly NodesSessionVM _session;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the shared nodes session.
    /// </summary>
    public NodesSessionVM Session => _session;

    /// <summary>
    /// Gets the object currently selected for property editing.
    /// </summary>
    public object? SelectedObject => _session.SelectedObject;

    /// <summary>
    /// Gets a value indicating whether the properties surface should be shown.
    /// </summary>
    public bool IsVisible => !_session.IsRunMode;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node properties VM.
    /// </summary>
    /// <param name="session">The shared nodes session.</param>
    public NodePropertiesVM(NodesSessionVM session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _session.PropertyChanged += SessionPropertyChanged;
    }

    #endregion Constructors

    #region Private Methods

    private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodesSessionVM.SelectedObject))
        {
            OnPropertyChanged(nameof(SelectedObject));
            return;
        }

        if (e.PropertyName != nameof(NodesSessionVM.CurrentMode))
            return;

        OnPropertyChanged(nameof(IsVisible));
    }

    #endregion Private Methods
}
