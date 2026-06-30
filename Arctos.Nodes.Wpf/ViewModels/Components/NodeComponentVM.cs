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
    private Action<string, Action, Action>? _recordHistory;

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
            ApplyComponentChange(
                "Change component label",
                Model.Label,
                label,
                resolvedLabel =>
                {
                    Model.Label = resolvedLabel;
                    DisplayName = GetDisplayName();
                    OnPropertyChanged();
                });
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
            ApplyComponentChange(
                "Change component id",
                Model.Id,
                id,
                resolvedId =>
                {
                    Model.Id = resolvedId;
                    OnPropertyChanged();
                });
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

    /// <summary>
    /// Gets or sets the zero-based layout column used by the component.
    /// </summary>
    public int Column
    {
        get => Math.Max(0, Model.Column);
        set => SetColumn(value);
    }

    /// <summary>
    /// Gets or sets the optional parent container identifier used by hierarchical layouts.
    /// </summary>
    public string? ParentId
    {
        get => string.IsNullOrWhiteSpace(Model.ParentId) ? null : Model.ParentId;
        set => SetParentId(value);
    }

    /// <summary>
    /// Gets or sets the persisted vertical ordering slot used within the current parent container.
    /// </summary>
    public int YOrder
    {
        get => Math.Max(0, Model.YOrder);
        set => SetYOrder(value);
    }

    /// <summary>
    /// Gets or sets the persisted horizontal ordering slot used within a shared row.
    /// </summary>
    public int XOrder
    {
        get => Math.Max(0, Model.XOrder);
        set => SetXOrder(value);
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
    /// Sets the callback used to record history entries for component edits.
    /// </summary>
    /// <param name="recordHistory">The callback that records redo/undo actions.</param>
    public void SetHistoryRecorder(Action<string, Action, Action>? recordHistory)
    {
        _recordHistory = recordHistory;
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

    /// <summary>
    /// Updates the component column, optionally suppressing the component changed callback.
    /// </summary>
    /// <param name="column">The new zero-based layout column.</param>
    /// <param name="notifyComponentChanged"><see langword="true"/> to notify the owner that the component changed; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the column changed; otherwise, <see langword="false"/>.</returns>
    public bool SetColumn(int column, bool notifyComponentChanged = true)
    {
        var resolvedColumn = Math.Max(0, column);
        if (Model.Column == resolvedColumn)
            return false;

        Model.Column = resolvedColumn;
        OnPropertyChanged(nameof(Column));

        if (notifyComponentChanged)
            NotifyComponentChanged();

        return true;
    }

    /// <summary>
    /// Updates the component parent identifier, optionally suppressing the component changed callback.
    /// </summary>
    /// <param name="parentId">The new parent identifier, or <see langword="null"/> for the root container.</param>
    /// <param name="notifyComponentChanged"><see langword="true"/> to notify the owner that the component changed; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the parent changed; otherwise, <see langword="false"/>.</returns>
    public bool SetParentId(string? parentId, bool notifyComponentChanged = true)
    {
        var resolvedParentId = string.IsNullOrWhiteSpace(parentId) ? null : parentId;
        if (string.Equals(Model.ParentId, resolvedParentId, StringComparison.Ordinal))
            return false;

        Model.ParentId = resolvedParentId;
        OnPropertyChanged(nameof(ParentId));

        if (notifyComponentChanged)
            NotifyComponentChanged();

        return true;
    }

    /// <summary>
    /// Updates the component vertical ordering slot, optionally suppressing the component changed callback.
    /// </summary>
    /// <param name="yOrder">The new zero-based row order.</param>
    /// <param name="notifyComponentChanged"><see langword="true"/> to notify the owner that the component changed; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the order changed; otherwise, <see langword="false"/>.</returns>
    public bool SetYOrder(int yOrder, bool notifyComponentChanged = true)
    {
        var resolvedYOrder = Math.Max(0, yOrder);
        if (Model.YOrder == resolvedYOrder)
            return false;

        Model.YOrder = resolvedYOrder;
        OnPropertyChanged(nameof(YOrder));

        if (notifyComponentChanged)
            NotifyComponentChanged();

        return true;
    }

    /// <summary>
    /// Updates the component horizontal ordering slot, optionally suppressing the component changed callback.
    /// </summary>
    /// <param name="xOrder">The new zero-based inline order.</param>
    /// <param name="notifyComponentChanged"><see langword="true"/> to notify the owner that the component changed; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the order changed; otherwise, <see langword="false"/>.</returns>
    public bool SetXOrder(int xOrder, bool notifyComponentChanged = true)
    {
        var resolvedXOrder = Math.Max(0, xOrder);
        if (Model.XOrder == resolvedXOrder)
            return false;

        Model.XOrder = resolvedXOrder;
        OnPropertyChanged(nameof(XOrder));

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
    /// Applies a component edit, optionally recording it into the active history context.
    /// </summary>
    /// <typeparam name="T">The edited value type.</typeparam>
    /// <param name="description">The user-facing history description.</param>
    /// <param name="oldValue">The previous value.</param>
    /// <param name="newValue">The new value.</param>
    /// <param name="applyValue">The action that applies the supplied value to the component.</param>
    /// <returns><see langword="true"/> when the value changed; otherwise, <see langword="false"/>.</returns>
    protected bool ApplyComponentChange<T>(string description, T oldValue, T newValue, Action<T> applyValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(applyValue);

        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return false;

        if (_recordHistory != null)
        {
            _recordHistory(
                description,
                () => applyValue(newValue),
                () => applyValue(oldValue));
            return true;
        }

        applyValue(newValue);
        NotifyComponentChanged();
        return true;
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
