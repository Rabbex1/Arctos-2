using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Presents the current graph document settings together with the current node library definitions.
/// </summary>
public partial class NodeDocumentVM : ObservableObject
{
    #region Fields

    private readonly NodesSessionVM _session;
    private NodeDocumentSettingsVM? _settings;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the shared nodes session.
    /// </summary>
    public NodesSessionVM Session => _session;

    /// <summary>
    /// Gets the current graph document.
    /// </summary>
    public NodeDocument? CurrentDocument => _session.CurrentNodeDocument;

    /// <summary>
    /// Gets the current document settings editor.
    /// </summary>
    public NodeDocumentSettingsVM? Settings => _settings;

    /// <summary>
    /// Gets the current editor mode.
    /// </summary>
    public OperationMode CurrentMode => _session.CurrentMode;

    /// <summary>
    /// Gets the current document's node definitions.
    /// </summary>
    public ObservableCollection<NodeDefinitionVM> Definitions => _session.Definitions;

    /// <summary>
    /// Gets or sets the selected node definition.
    /// </summary>
    public NodeDefinitionVM? SelectedDefinition
    {
        get => _session.SelectedDefinition;
        set => _session.SelectedDefinition = value;
    }

    /// <summary>
    /// Gets a value indicating whether the document settings section should be shown.
    /// </summary>
    public bool ShowSettings => !_session.IsRunMode;

    /// <summary>
    /// Gets a value indicating whether the library list should be shown.
    /// </summary>
    public bool ShowLibrary => _session.IsLibraryEditMode;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node document VM.
    /// </summary>
    /// <param name="session">The shared nodes session.</param>
    public NodeDocumentVM(NodesSessionVM session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _session.PropertyChanged += SessionPropertyChanged;
        UpdateSettings();
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Adds a new node definition to the current node library.
    /// </summary>
    public void AddDefinition()
    {
        _session.AddDefinition();
    }

    /// <summary>
    /// Removes the selected node definition from the current node library.
    /// </summary>
    public void RemoveSelectedDefinition()
    {
        _session.RemoveSelectedDefinition();
    }

    /// <summary>
    /// Renames the selected node definition in the current node library.
    /// </summary>
    /// <param name="newTypeName">The new stable type name.</param>
    public void RenameSelectedDefinition(string? newTypeName)
    {
        _session.RenameSelectedDefinition(newTypeName);
    }

    #endregion Public Methods

    #region Private Methods

    private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodesSessionVM.CurrentDocument))
        {
            UpdateSettings();
            OnPropertyChanged(nameof(CurrentDocument));
            return;
        }

        if (e.PropertyName == nameof(NodesSessionVM.SelectedDefinition))
        {
            OnPropertyChanged(nameof(SelectedDefinition));
            return;
        }

        if (e.PropertyName != nameof(NodesSessionVM.CurrentMode))
            return;

        OnPropertyChanged(nameof(CurrentMode));
        OnPropertyChanged(nameof(ShowSettings));
        OnPropertyChanged(nameof(ShowLibrary));
    }

    private void UpdateSettings()
    {
        _settings = CurrentDocument == null ? null : new NodeDocumentSettingsVM(CurrentDocument.Settings);
        OnPropertyChanged(nameof(Settings));
    }

    #endregion Private Methods
}
