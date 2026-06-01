using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Coordinates editing of the selected node definition's components.
/// </summary>
public partial class NodeComponentsVM : ObservableObject
{
    #region Fields

    private readonly NodesSessionVM _session;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the component VMs for the selected node definition.
    /// </summary>
    public ObservableCollection<NodeComponentVM> Components { get; } = [];

    /// <summary>
    /// Gets or sets the selected component.
    /// </summary>
    [ObservableProperty]
    private NodeComponentVM? _selectedComponent;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node components VM.
    /// </summary>
    /// <param name="session">The owning nodes session.</param>
    public NodeComponentsVM(NodesSessionVM session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _session.PropertyChanged += SessionPropertyChanged;
        Refresh();
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Adds a new default string component to the selected node definition.
    /// </summary>
    public void AddComponent()
    {
        AddComponent(InputTypeRegistry.Default[InputTypeIds.String]);
    }

    /// <summary>
    /// Adds a new component to the selected node definition using the provided type definition.
    /// </summary>
    /// <param name="typeDefinition">The component type definition to add.</param>
    public void AddComponent(InputTypeDefinition typeDefinition)
    {
        ArgumentNullException.ThrowIfNull(typeDefinition);

        if (_session.SelectedDefinition == null)
            return;

        var model = new NodeComponent
        {
            Id = CreateUniqueComponentId(),
            DisplayName = typeDefinition.DisplayName,
            TypeId = typeDefinition.Id,
        };

        _session.SelectedDefinition.Model.Components.Add(model);
        Refresh();
        SelectedComponent = Components.FirstOrDefault(item => ReferenceEquals(item.Model, model));
    }

    /// <summary>
    /// Removes the selected component from the selected node definition.
    /// </summary>
    public void RemoveSelectedComponent()
    {
        if (_session.SelectedDefinition == null || SelectedComponent == null)
            return;

        _session.SelectedDefinition.Model.Components.Remove(SelectedComponent.Model);
        Refresh();
        SelectedComponent = Components.FirstOrDefault();
    }

    /// <summary>
    /// Reloads the component collection from the selected node definition.
    /// </summary>
    public void Refresh()
    {
        var selectedModel = SelectedComponent?.Model;

        Components.Clear();

        if (_session.SelectedDefinition == null)
        {
            SelectedComponent = null;
            return;
        }

        foreach (var component in _session.SelectedDefinition.Model.Components)
            Components.Add(new NodeComponentVM(component));

        SelectedComponent = selectedModel == null
            ? Components.FirstOrDefault()
            : Components.FirstOrDefault(item => ReferenceEquals(item.Model, selectedModel)) ?? Components.FirstOrDefault();
    }

    #endregion Public Methods

    #region Private Methods

    private string CreateUniqueComponentId()
    {
        const string baseId = "component";
        var components = _session.SelectedDefinition?.Model.Components;
        if (components == null)
            return baseId;

        if (components.All(item => !string.Equals(item.Id, baseId, StringComparison.Ordinal)))
            return baseId;

        var index = 1;
        while (components.Any(item => string.Equals(item.Id, $"{baseId}{index}", StringComparison.Ordinal)))
            index++;

        return $"{baseId}{index}";
    }

    private void SessionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(NodesSessionVM.SelectedDefinition))
            return;

        Refresh();
    }

    #endregion Private Methods
}
