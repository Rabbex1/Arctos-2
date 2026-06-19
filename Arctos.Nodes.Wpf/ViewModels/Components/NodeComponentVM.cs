using System.Collections.ObjectModel;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Base view model for a node component that may later be placed inside a detailed node surface.
/// </summary>
public abstract class NodeComponentVM : BaseVM, IEditTabs
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly InputFormVM _generalForm = new();
    private bool _isHovered;
    private bool _isSelected;
    private readonly bool _supportsId;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the persisted component model wrapped by this view model.
    /// </summary>
    public NodeComponent Model { get; }

    /// <summary>
    /// Gets the palette kind represented by this component.
    /// </summary>
    public PaletteItemKind Kind { get; }

    /// <summary>
    /// Gets the human-readable component type name.
    /// </summary>
    public string ComponentTypeName { get; }

    /// <summary>
    /// Gets or sets the component label shown to the user.
    /// </summary>
    public virtual string Label
    {
        get => Model.Label;
        set
        {
            var label = value ?? string.Empty;
            if (string.Equals(Model.Label, label, StringComparison.Ordinal))
                return;

            Model.Label = label;
            DisplayName = GetDisplayName();
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    /// <summary>
    /// Gets or sets the host-facing component identifier.
    /// </summary>
    public string Id
    {
        get => Model.Id ?? string.Empty;
        set
        {
            var id = string.IsNullOrWhiteSpace(value) ? null : value;
            if (string.Equals(Model.Id, id, StringComparison.Ordinal))
                return;

            Model.Id = id;
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the component exposes an editable host-facing identifier.
    /// </summary>
    public bool SupportsId => _supportsId;

    /// <summary>
    /// Gets the edit tabs shown when this component is selected.
    /// </summary>
    public IReadOnlyList<EditTabVM> EditTabs { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is currently selected in the detailed node editor.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the component is currently hovered in the detailed node editor.
    /// </summary>
    public bool IsHovered
    {
        get => _isHovered;
        set => SetProperty(ref _isHovered, value);
    }

    /// <summary>
    /// Gets the callback invoked when the component changes.
    /// </summary>
    public Action? OnComponentChanged { get; private set; }

    /// <summary>
    /// Gets or sets the desired node-space position of the component in minor node units.
    /// </summary>
    public NodePoint Position
    {
        get => Model.Position;
        set => SetPosition(value);
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node component view model.
    /// </summary>
    /// <param name="kind">The palette kind represented by the component.</param>
    /// <param name="componentTypeName">The human-readable component type name.</param>
    /// <param name="label">The initial component label.</param>
    protected NodeComponentVM(NodeComponent model, PaletteItemKind kind, string componentTypeName, bool supportsId = false)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(componentTypeName);

        Model = model;
        Kind = kind;
        ComponentTypeName = componentTypeName;
        _supportsId = supportsId;

        _generalForm.OnInputChanged = NotifyComponentChanged;

        EditTabs =
        [
            new EditTabVM("General", _generalForm),
        ];

        DisplayName = GetDisplayName();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Sets the callback invoked when the component changes.
    /// </summary>
    /// <param name="onComponentChanged">The callback invoked when the component mutates.</param>
    public void SetComponentChangedCallback(Action? onComponentChanged)
    {
        OnComponentChanged = onComponentChanged;
    }

    /// <summary>
    /// Updates the component position, optionally suppressing the component changed callback.
    /// </summary>
    /// <param name="position">The new node-space position.</param>
    /// <param name="notifyComponentChanged"><see langword="true"/> to notify the owner that the component changed; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the position changed; otherwise, <see langword="false"/>.</returns>
    public bool SetPosition(NodePoint position, bool notifyComponentChanged = true)
    {
        if (Model.Position == position)
            return false;

        Model.Position = position;
        OnPropertyChanged(nameof(Position));

        if (notifyComponentChanged)
            NotifyComponentChanged();

        return true;
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    /// <summary>
    /// Rebuilds the currently exposed edit tabs from the component state.
    /// </summary>
    protected void RebuildEditTabs()
    {
        var inputs = new List<DataInputWrapperVM>();

        if (SupportsId)
        {
            inputs.Add(new InputStringVM("Id:", Id, id =>
            {
                Id = id ?? string.Empty;
            }));
        }

        inputs.AddRange(CreateGeneralInputs());
        _generalForm.Inputs = new ObservableCollection<DataInputWrapperVM>(inputs);
    }

    /// <summary>
    /// Returns the edit inputs shown in the general tab.
    /// </summary>
    /// <returns>The general-tab inputs.</returns>
    protected abstract List<DataInputWrapperVM> CreateGeneralInputs();

    /// <summary>
    /// Notifies the owner that the component changed.
    /// </summary>
    protected void NotifyComponentChanged()
    {
        OnComponentChanged?.Invoke();
    }

    /// <summary>
    /// Returns the display name shown for the component.
    /// </summary>
    /// <returns>The current display name.</returns>
    protected virtual string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(Label)
            ? ComponentTypeName
            : Label;
    }

    #endregion Protected Methods
}
