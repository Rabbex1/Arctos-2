using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Provides a general-purpose palette of items that can be added based on the current mode and focus.
/// </summary>
public partial class NodePaletteVM : ObservableObject
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
    /// Gets the current set of palette items.
    /// </summary>
    public ObservableCollection<NodePaletteItemVM> Items { get; } = [];

    /// <summary>
    /// Gets or sets the selected palette item.
    /// </summary>
    [ObservableProperty]
    private NodePaletteItemVM? _selectedItem;

    /// <summary>
    /// Gets the current palette title.
    /// </summary>
    public string Title => _session.IsLibraryEditMode ? "Available Components" : "Available Nodes";

    /// <summary>
    /// Gets a value indicating whether the palette surface should be shown.
    /// </summary>
    public bool IsVisible => !_session.IsRunMode;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node palette VM.
    /// </summary>
    /// <param name="session">The shared nodes session.</param>
    public NodePaletteVM(NodesSessionVM session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _session.PropertyChanged += SessionPropertyChanged;
        Refresh();
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Adds the currently selected palette item to the active editing target.
    /// </summary>
    public void AddSelectedItem()
    {
        if (SelectedItem == null)
            return;

        if (SelectedItem.Kind == NodePaletteItemKind.ComponentType && SelectedItem.TypeDefinition != null)
        {
            _session.Components.AddComponent(SelectedItem.TypeDefinition);
            return;
        }

        // Graph placement is not implemented yet; when it is, graph-edit palette selections
        // will transition the canvas into placement mode here.
    }

    /// <summary>
    /// Reloads the palette items for the current editing context.
    /// </summary>
    public void Refresh()
    {
        var selectedItemTypeId = SelectedItem?.TypeDefinition?.Id;
        var selectedNodeDefinitionTypeName = SelectedItem?.NodeDefinition?.TypeName;

        Items.Clear();

        if (_session.CurrentNodeDocument == null || _session.IsRunMode)
        {
            SelectedItem = null;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(IsVisible));
            return;
        }

        if (_session.IsLibraryEditMode)
        {
            if (_session.SelectedDefinition == null)
            {
                SelectedItem = null;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(IsVisible));
                return;
            }

            foreach (var typeDefinition in InputTypeRegistry.Default.OrderBy(item => item.DisplayName, StringComparer.Ordinal))
            {
                Items.Add(new NodePaletteItemVM
                {
                    Kind = NodePaletteItemKind.ComponentType,
                    DisplayName = typeDefinition.DisplayName,
                    TypeDefinition = typeDefinition,
                });
            }
        }
        else if (_session.IsGraphEditMode)
        {
            foreach (var definition in _session.Definitions.OrderBy(item => item.TypeName, StringComparer.Ordinal))
            {
                Items.Add(new NodePaletteItemVM
                {
                    Kind = NodePaletteItemKind.NodeDefinition,
                    DisplayName = definition.DisplayName ?? definition.TypeName,
                    NodeDefinition = definition,
                });
            }
        }

        SelectedItem = _session.IsLibraryEditMode
            ? selectedItemTypeId == null
                ? Items.FirstOrDefault()
                : Items.FirstOrDefault(item => item.TypeDefinition?.Id.Equals(selectedItemTypeId.Value) == true) ?? Items.FirstOrDefault()
            : selectedNodeDefinitionTypeName == null
                ? Items.FirstOrDefault()
                : Items.FirstOrDefault(item => string.Equals(item.NodeDefinition?.TypeName, selectedNodeDefinitionTypeName, StringComparison.Ordinal)) ?? Items.FirstOrDefault();

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(IsVisible));
    }

    #endregion Public Methods

    #region Private Methods

    private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(NodesSessionVM.CurrentDocument)
            && e.PropertyName != nameof(NodesSessionVM.CurrentMode)
            && e.PropertyName != nameof(NodesSessionVM.SelectedDefinition))
            return;

        Refresh();
    }

    #endregion Private Methods
}
