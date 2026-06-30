using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.DragImage;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Wpf;
using Arctos.Wpf.Inputs;
using Arctos.Wpf.Utilities;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Editable tree view model wrapper over a <see cref="NodeDefinition"/>.
/// </summary>
public sealed class NodeDefinitionVM : NodeTreeItemVM<INodeTreeItemVM>, IEditTabs, IModelHost<NodeDefinition>, ICanvasViewStateHost, IDragImageHost
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly NodeDefinition _definition;
    private readonly ObservableCollection<NodeComponentVM> _nodeComponents = [];
    private readonly ObservableCollection<NodeComponentVM> _selectedComponents = [];
    private readonly InputFormVM _generalForm = new();
    private readonly InputFormVM _headerForm = new();
    private readonly InputFormVM _styleForm = new();
    private readonly InputFormVM _columnForm = new();
    private readonly InputFormVM _multiSelectionForm = new();
    private readonly IReadOnlyList<EditTabVM> _editTabs;
    private readonly IReadOnlyList<EditTabVM> _columnEditTabs;
    private readonly IReadOnlyList<EditTabVM> _multiSelectionEditTabs;
    private ICommand? _deleteCommand;
    private NodeComponentVM? _selectedComponent;
    private NodeColumn? _selectedColumn;
    private Action<string, Action, Action>? _recordHistory;
    private NodePoint _canvasPan;
    private float _canvasZoom;
    private DragImage.DragImage? _dragImage;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the hosted definition model.
    /// </summary>
    public NodeDefinition Model => _definition;

    object IModelHost.Model => Model;

    string IModelHost.DisplayName => DisplayName ?? string.Empty;

    bool IModelHost.HasOverview => HasOverview;

    /// <summary>
    /// Gets a value indicating whether this definition comes from an external library.
    /// </summary>
    public bool IsExternal { get; }

    /// <summary>
    /// Gets the stable node type name.
    /// </summary>
    public string TypeName => _definition.TypeName;

    /// <summary>
    /// Gets or sets the human-readable display name.
    /// </summary>
    public string DisplayNameText
    {
        get => _definition.DisplayName ?? string.Empty;
        set
        {
            ApplyDefinitionChange(
                "Change node name",
                _definition.DisplayName,
                value,
                displayName =>
                {
                    _definition.DisplayName = displayName;
                    DisplayName = displayName;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResolvedHeaderText));
                    OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
                });
        }
    }

    /// <summary>
    /// Gets or sets the optional header text override.
    /// </summary>
    public string HeaderText
    {
        get => _definition.HeaderText ?? string.Empty;
        set
        {
            var headerText = string.IsNullOrWhiteSpace(value) ? null : value;
            ApplyDefinitionChange(
                "Change header text",
                _definition.HeaderText,
                headerText,
                resolvedHeaderText =>
                {
                    _definition.HeaderText = resolvedHeaderText;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResolvedHeaderText));
                });
        }
    }

    /// <summary>
    /// Gets the effective header text, using the node name when no override is defined.
    /// </summary>
    public string ResolvedHeaderText => string.IsNullOrWhiteSpace(_definition.HeaderText)
        ? DisplayNameText
        : _definition.HeaderText;

    /// <summary>
    /// Gets or sets the application-defined domain type for the definition.
    /// </summary>
    public string DomainType
    {
        get => _definition.DomainType;
        set
        {
            ApplyDefinitionChange(
                "Change domain type",
                _definition.DomainType,
                value,
                domainType =>
                {
                    _definition.DomainType = domainType;
                    OnPropertyChanged();
                });
        }
    }

    /// <summary>
    /// Gets or sets the primitive editor shape used by the definition.
    /// </summary>
    public NodeShape NodeShape
    {
        get => _definition.NodeShape;
        set
        {
            ApplyDefinitionChange(
                "Change node shape",
                _definition.NodeShape,
                value,
                shape =>
                {
                    _definition.NodeShape = shape;
                    _definition.Size = NodeSize.CreateDefault(shape);
                    _definition.ShapeRules = ShapeRules.CreateDefault(shape);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Size));
                    OnPropertyChanged(nameof(ShapeRules));
                });
        }
    }

    /// <summary>
    /// Gets the intrinsic node size associated with the current definition.
    /// </summary>
    public NodeSize Size => _definition.Size;

    /// <summary>
    /// Gets or sets a value indicating whether the definition supports an overview mode.
    /// </summary>
    public bool HasOverview
    {
        get => _definition.HasOverview;
        set
        {
            ApplyDefinitionChange(
                "Change overview mode",
                _definition.HasOverview,
                value,
                hasOverview =>
                {
                    _definition.HasOverview = hasOverview;
                    OnPropertyChanged();
                });
        }
    }

    /// <summary>
    /// Gets or sets the number of layout columns available to the definition.
    /// </summary>
    public int ColumnCount
    {
        get => Math.Clamp(_definition.Columns.Count == 0 ? 1 : _definition.Columns.Count, 1, 4);
        set
        {
            var columnCount = Math.Clamp(value, 1, 4);
            EnsureModelStructure();
            var currentColumnCount = Math.Clamp(_definition.Columns.Count, 1, 4);
            if (currentColumnCount == columnCount)
                return;

            if (columnCount < currentColumnCount)
            {
                CollapseRemovedColumns(currentColumnCount, columnCount);
            }
            else
            {
                for (var index = currentColumnCount; index < columnCount; index++)
                    _definition.Columns.Add(CreateColumn(index));

                NormalizeColumns();
                SynchronizeComponentPlacementFromModel();
            }

            if (SelectedColumn != null && !_definition.Columns.Contains(SelectedColumn))
                SetSelectedColumn(null);

            OnPropertyChanged();
            NotifyDocumentChanged();
        }
    }

    /// <summary>
    /// Inserts a new empty column at the requested visual index.
    /// </summary>
    /// <param name="columnIndex">The zero-based insertion index.</param>
    /// <param name="notifyDocumentChanged"><see langword="true"/> to notify the owning document; otherwise, <see langword="false"/>.</param>
    public void InsertColumn(int columnIndex, bool notifyDocumentChanged = true)
    {
        EnsureModelStructure();

        var orderedColumns = GetOrderedColumns();
        var insertIndex = Math.Clamp(columnIndex, 0, orderedColumns.Count);
        var newColumn = CreateColumn(insertIndex);
        orderedColumns.Insert(insertIndex, newColumn);

        for (var index = 0; index < orderedColumns.Count; index++)
            orderedColumns[index].XOrder = index;

        _definition.Columns = orderedColumns;
        NormalizeColumns();
        SynchronizeComponentPlacementFromModel();
        if (SelectedComponent != null)
            SelectComponent(null);

        SetSelectedColumn(newColumn);
        OnPropertyChanged(nameof(ColumnCount));

        if (notifyDocumentChanged)
            NotifyDocumentChanged();
    }

    /// <summary>
    /// Moves the specified column to a new visual index.
    /// </summary>
    /// <param name="sourceIndex">The zero-based source column index.</param>
    /// <param name="targetIndex">The zero-based destination column index after the move.</param>
    /// <param name="notifyDocumentChanged"><see langword="true"/> to notify the owning document; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the column order changed; otherwise, <see langword="false"/>.</returns>
    public bool MoveColumn(int sourceIndex, int targetIndex, bool notifyDocumentChanged = true)
    {
        EnsureModelStructure();
        var orderedColumns = GetOrderedColumns();
        if (orderedColumns.Count <= 1)
            return false;

        var resolvedSourceIndex = Math.Clamp(sourceIndex, 0, orderedColumns.Count - 1);
        var resolvedTargetIndex = Math.Clamp(targetIndex, 0, orderedColumns.Count - 1);
        if (resolvedSourceIndex == resolvedTargetIndex)
            return false;

        var movedColumn = orderedColumns[resolvedSourceIndex];
        orderedColumns.RemoveAt(resolvedSourceIndex);
        orderedColumns.Insert(resolvedTargetIndex, movedColumn);

        _definition.Columns = orderedColumns;
        NormalizeColumns();
        SynchronizeComponentPlacementFromModel();

        if (SelectedColumn != null)
        {
            OnPropertyChanged(nameof(SelectedColumnIndex));
            OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
        }

        if (notifyDocumentChanged)
            NotifyDocumentChanged();

        return true;
    }

    /// <summary>
    /// Gets the shape rules associated with the current definition.
    /// </summary>
    public ShapeRules ShapeRules
    {
        get
        {
            _definition.ShapeRules ??= ShapeRules.CreateDefault(_definition.NodeShape);
            return _definition.ShapeRules;
        }
    }

    /// <summary>
    /// Gets the edit tabs shown for the current definition.
    /// </summary>
    public IReadOnlyList<EditTabVM> EditTabs => HasMultipleComponentSelection
        ? _multiSelectionEditTabs
        : SelectedComponent?.EditTabs ?? (SelectedColumn != null ? _columnEditTabs : _editTabs);

    /// <summary>
    /// Gets the currently selected component inside the detailed node editor.
    /// </summary>
    public NodeComponentVM? SelectedComponent
    {
        get => _selectedComponent;
        private set => SetProperty(ref _selectedComponent, value);
    }

    /// <summary>
    /// Gets the currently selected detailed-node components.
    /// </summary>
    public IReadOnlyList<NodeComponentVM> SelectedComponents => _selectedComponents;

    /// <summary>
    /// Gets the number of selected detailed-node components.
    /// </summary>
    public int SelectedComponentCount => _selectedComponents.Count;

    /// <summary>
    /// Gets a value indicating whether more than one detailed-node component is currently selected.
    /// </summary>
    public bool HasMultipleComponentSelection => _selectedComponents.Count > 1;

    /// <summary>
    /// Gets the currently selected column inside the detailed node editor.
    /// </summary>
    public NodeColumn? SelectedColumn
    {
        get => _selectedColumn;
        private set => SetProperty(ref _selectedColumn, value);
    }

    /// <summary>
    /// Gets the zero-based visual index of the currently selected column, when any.
    /// </summary>
    public int? SelectedColumnIndex
    {
        get
        {
            if (SelectedColumn == null)
                return null;

            var orderedColumns = GetOrderedColumns();
            var index = orderedColumns.IndexOf(SelectedColumn);
            return index >= 0 ? index : null;
        }
    }

    /// <summary>
    /// Gets the display name for the active edit target.
    /// </summary>
    public string CurrentEditTargetDisplayName => HasMultipleComponentSelection
        ? $"{SelectedComponentCount} Components Selected"
        : SelectedComponent?.DisplayName
        ?? (SelectedColumn != null ? GetColumnDisplayName(SelectedColumn) : DisplayName ?? string.Empty);

    /// <summary>
    /// Gets the temporary in-memory component instances currently associated with this definition.
    /// </summary>
    public ObservableCollection<NodeComponentVM> NodeComponents => _nodeComponents;

    /// <summary>
    /// Gets or sets the last canvas pan used while this definition was active.
    /// </summary>
    public NodePoint CanvasPan
    {
        get => _canvasPan;
        set => SetProperty(ref _canvasPan, value);
    }

    /// <summary>
    /// Gets or sets the last canvas zoom used while this definition was active.
    /// A value less than or equal to zero indicates that the definition has not been initialized in the canvas yet.
    /// </summary>
    public float CanvasZoom
    {
        get => _canvasZoom;
        set => SetProperty(ref _canvasZoom, value);
    }

    /// <summary>
    /// Gets or sets the cached drag image owned by this definition host.
    /// </summary>
    public DragImage.DragImage? DragImage
    {
        get => _dragImage;
        set => SetProperty(ref _dragImage, value);
    }

    /// <summary>
    /// Gets the callback used to notify the owning document that the definition changed.
    /// </summary>
    public Action? OnDocumentChanged { get; private set; }

    /// <summary>
    /// Gets the command that deletes the definition from the local library.
    /// </summary>
    public ICommand DeleteCommand => _deleteCommand ??= new RelayCommand(
        _ => Delete(),
        _ => !IsExternal && Parent is NodeLibraryVM);

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new definition view model.
    /// </summary>
    /// <param name="definition">The definition model to wrap.</param>
    /// <param name="parent">The parent tree item.</param>
    /// <param name="isExternal"><see langword="true"/> when the definition came from an external shared library; otherwise, <see langword="false"/>.</param>
    public NodeDefinitionVM(NodeDefinition definition, INodeTreeItemVM? parent = null, bool isExternal = false)
        : base(parent)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        IsExternal = isExternal;
        DisplayName = _definition.DisplayName ?? _definition.TypeName;
        _generalForm.OnInputChanged = NotifyDocumentChanged;
        _headerForm.OnInputChanged = NotifyDocumentChanged;
        _styleForm.OnInputChanged = NotifyDocumentChanged;
        _columnForm.OnInputChanged = NotifyDocumentChanged;
        _editTabs =
        [
            new EditTabVM("General", _generalForm),
            new EditTabVM("Header", _headerForm),
            new EditTabVM("Style", _styleForm),
        ];
        _columnEditTabs =
        [
            new EditTabVM("Column", _columnForm),
        ];
        _multiSelectionForm.Inputs = new ObservableCollection<DataInputWrapperVM>(
        [
            new InputLabelVM(string.Empty, "Multiple components selected. Delete is available; common property editing will come next.", "Left")
        ]);
        _multiSelectionEditTabs =
        [
            new EditTabVM("Selection", _multiSelectionForm),
        ];
        EnsureModelStructure();
        LoadComponentsFromModel();
        MakeEditTabs();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Sets the callback invoked when the definition mutates the owning document.
    /// </summary>
    /// <param name="onDocumentChanged">The callback invoked when the definition changes.</param>
    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        OnDocumentChanged = onDocumentChanged;
    }

    /// <summary>
    /// Sets the callback used to record history entries for node definition edits.
    /// </summary>
    /// <param name="recordHistory">The callback that records redo/undo actions.</param>
    public void SetHistoryRecorder(Action<string, Action, Action>? recordHistory)
    {
        _recordHistory = recordHistory;

        foreach (var component in _nodeComponents)
            component.SetHistoryRecorder(recordHistory);
    }

    /// <summary>
    /// Adds a temporary component instance to the current definition.
    /// </summary>
    /// <param name="component">The component instance to add.</param>
    public void AddComponent(NodeComponentVM component, bool notifyDocumentChanged = true)
    {
        ArgumentNullException.ThrowIfNull(component);

        if (_nodeComponents.Contains(component))
            return;

        component.SetComponentChangedCallback(NotifyDocumentChanged);
        component.SetHistoryRecorder(_recordHistory);
        _nodeComponents.Add(component);
        SelectComponent(component);
        if (notifyDocumentChanged)
            NotifyDocumentChanged();
    }

    /// <summary>
    /// Places the supplied component into the requested column and row slot, normalizing sibling row orders as needed.
    /// </summary>
    /// <param name="component">The component to place.</param>
    /// <param name="column">The zero-based target column.</param>
    /// <param name="rowIndex">The zero-based target row index within the parent container.</param>
    /// <param name="inlineWithExistingRow"><see langword="true"/> to place the component into an existing row; otherwise, <see langword="false"/> to insert a new row.</param>
    /// <param name="parentId">The target parent identifier, or <see langword="null"/> for the root container.</param>
    /// <param name="notifyDocumentChanged"><see langword="true"/> to notify the owning document; otherwise, <see langword="false"/>.</param>
    public void PlaceComponent(NodeComponentVM component, int column, int rowIndex, bool inlineWithExistingRow, int xOrder = 0, string? parentId = null, bool notifyDocumentChanged = true)
    {
        ArgumentNullException.ThrowIfNull(component);
        EnsureModelStructure();

        var resolvedColumn = Math.Max(0, column);
        var orderedColumns = GetOrderedColumns();
        var targetColumn = orderedColumns[Math.Clamp(resolvedColumn, 0, orderedColumns.Count - 1)];

        RemoveComponentFromStructure(component.Model, out var sourceColumn);

        NormalizeRows(targetColumn);
        if (sourceColumn != null && !ReferenceEquals(sourceColumn, targetColumn))
            NormalizeRows(sourceColumn);

        var targetRows = targetColumn.Rows
            .OrderBy(row => row.YOrder)
            .ToList();

        if (inlineWithExistingRow && targetRows.Count > 0)
        {
            var targetRowIndex = Math.Clamp(rowIndex, 0, targetRows.Count - 1);
            var targetRow = targetRows[targetRowIndex];
            var orderedRowComponents = targetRow.Components
                .OrderBy(candidate => IsButtonLikeComponent(candidate) ? candidate.XOrder : 0)
                .ToList();
            var targetXOrder = Math.Clamp(xOrder, 0, orderedRowComponents.Count);
            orderedRowComponents.Insert(targetXOrder, component.Model);
            for (var index = 0; index < orderedRowComponents.Count; index++)
                orderedRowComponents[index].XOrder = IsButtonLikeComponent(orderedRowComponents[index]) ? index : 0;

            targetRow.Components = orderedRowComponents;
            NormalizeRow(targetRow);
        }
        else
        {
            var targetRowIndex = Math.Clamp(rowIndex, 0, targetRows.Count);
            targetRows.Insert(targetRowIndex, CreateRow([component.Model]));
            for (var index = 0; index < targetRows.Count; index++)
                targetRows[index].YOrder = index;

            targetColumn.Rows = targetRows;
        }

        NormalizeRows(targetColumn);
        SynchronizeComponentPlacementFromModel();

        if (notifyDocumentChanged)
            NotifyDocumentChanged();
    }

    /// <summary>
    /// Updates the intrinsic node size without forcing a document-changed callback.
    /// </summary>
    /// <param name="size">The new intrinsic node size.</param>
    public void SetSize(NodeSize size)
    {
        if (_definition.Size == size)
            return;

        _definition.Size = size;
        OnPropertyChanged(nameof(Size));
    }

    /// <summary>
    /// Selects the supplied component in the detailed node editor.
    /// </summary>
    /// <param name="component">The component to select, or <see langword="null"/> to clear the selection.</param>
    /// <returns><see langword="true"/> when the selection changed; otherwise, <see langword="false"/>.</returns>
    public bool SelectComponent(NodeComponentVM? component)
    {
        if (component != null && _selectedComponents.Count == 1 && ReferenceEquals(_selectedComponents[0], component))
            return false;

        if (component != null)
            SetSelectedColumn(null);

        return SetComponentSelection(component == null ? [] : [component], component);
    }

    /// <summary>
    /// Clears the current component selection.
    /// </summary>
    /// <returns><see langword="true"/> when the selection changed; otherwise, <see langword="false"/>.</returns>
    public bool ClearComponentSelection()
    {
        return SetComponentSelection([], null);
    }

    /// <summary>
    /// Toggles the supplied component in the current detailed-node selection set.
    /// </summary>
    /// <param name="component">The component to toggle.</param>
    /// <returns><see langword="true"/> when the selection changed; otherwise, <see langword="false"/>.</returns>
    public bool ToggleComponentSelection(NodeComponentVM? component)
    {
        if (component == null || !_nodeComponents.Contains(component))
            return false;

        if (_selectedComponents.Contains(component))
        {
            var remainingComponents = _selectedComponents
                .Where(selectedComponent => !ReferenceEquals(selectedComponent, component))
                .ToList();
            var nextPrimaryComponent = ReferenceEquals(SelectedComponent, component)
                ? remainingComponents.LastOrDefault()
                : SelectedComponent;
            return SetComponentSelection(remainingComponents, nextPrimaryComponent);
        }

        if (SelectedColumn != null)
            SetSelectedColumn(null);

        var updatedComponents = _selectedComponents.ToList();
        updatedComponents.Add(component);
        return SetComponentSelection(updatedComponents, component);
    }

    /// <summary>
    /// Selects the supplied column in the detailed node editor.
    /// </summary>
    /// <param name="columnIndex">The zero-based visual column index to select.</param>
    /// <returns><see langword="true"/> when the selection changed; otherwise, <see langword="false"/>.</returns>
    public bool SelectColumn(int columnIndex)
    {
        EnsureModelStructure();
        var orderedColumns = GetOrderedColumns();
        var selectedColumn = columnIndex >= 0 && columnIndex < orderedColumns.Count
            ? orderedColumns[columnIndex]
            : null;

        var selectionChanged = false;
        if (SelectedComponent != null)
            selectionChanged |= SelectComponent(null);

        selectionChanged |= SetSelectedColumn(selectedColumn);
        return selectionChanged;
    }

    /// <summary>
    /// Clears the current column selection.
    /// </summary>
    /// <returns><see langword="true"/> when the selection changed; otherwise, <see langword="false"/>.</returns>
    public bool ClearColumnSelection()
    {
        return SetSelectedColumn(null);
    }

    /// <summary>
    /// Removes the supplied component from the definition.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <returns><see langword="true"/> when the component was removed; otherwise, <see langword="false"/>.</returns>
    public bool RemoveComponent(NodeComponentVM? component)
    {
        if (component == null || !_nodeComponents.Contains(component))
            return false;

        RemoveSelectedComponentReference(component);

        component.SetComponentChangedCallback(null);
        _nodeComponents.Remove(component);
        RemoveComponentFromStructure(component.Model, out _);
        SynchronizeComponentPlacementFromModel();
        NotifyDocumentChanged();
        return true;
    }

    /// <summary>
    /// Removes the currently selected component from the definition.
    /// </summary>
    /// <returns><see langword="true"/> when the selected component was removed; otherwise, <see langword="false"/>.</returns>
    public bool RemoveSelectedComponent()
    {
        return RemoveComponent(SelectedComponent);
    }

    /// <summary>
    /// Removes all currently selected components from the definition.
    /// </summary>
    /// <returns><see langword="true"/> when one or more components were removed; otherwise, <see langword="false"/>.</returns>
    public bool RemoveSelectedComponents()
    {
        var componentsToRemove = _selectedComponents.ToList();
        if (componentsToRemove.Count == 0)
            return false;

        ClearComponentSelection();

        foreach (var component in componentsToRemove)
        {
            component.SetComponentChangedCallback(null);
            _nodeComponents.Remove(component);
            RemoveComponentFromStructure(component.Model, out _);
        }

        SynchronizeComponentPlacementFromModel();
        NotifyDocumentChanged();
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the specified column currently owns any rows with components.
    /// </summary>
    /// <param name="columnIndex">The zero-based visual column index.</param>
    /// <returns><see langword="true"/> when the column contains at least one component; otherwise, <see langword="false"/>.</returns>
    public bool ColumnHasContent(int columnIndex)
    {
        EnsureModelStructure();
        var orderedColumns = GetOrderedColumns();
        if (columnIndex < 0 || columnIndex >= orderedColumns.Count)
            return false;

        return orderedColumns[columnIndex].Rows.Any(row => row.Components.Count > 0);
    }

    /// <summary>
    /// Gets a value indicating whether the specified delete rule can be applied to the supplied column.
    /// </summary>
    /// <param name="columnIndex">The zero-based visual column index.</param>
    /// <param name="rule">The requested delete rule.</param>
    /// <returns><see langword="true"/> when the rule is valid for the column; otherwise, <see langword="false"/>.</returns>
    public bool CanDeleteColumn(int columnIndex, NodeColumnDeleteRule rule)
    {
        EnsureModelStructure();
        var orderedColumns = GetOrderedColumns();
        if (columnIndex < 0 || columnIndex >= orderedColumns.Count)
            return false;

        return rule switch
        {
            NodeColumnDeleteRule.Cancel => true,
            NodeColumnDeleteRule.Delete => orderedColumns.Count > 1,
            NodeColumnDeleteRule.MoveLeft => columnIndex > 0,
            NodeColumnDeleteRule.MoveRight => columnIndex < orderedColumns.Count - 1,
            _ => false,
        };
    }

    /// <summary>
    /// Deletes the specified column using the supplied content resolution rule.
    /// </summary>
    /// <param name="columnIndex">The zero-based visual column index to delete.</param>
    /// <param name="rule">The rule used to resolve any content owned by the column.</param>
    /// <returns><see langword="true"/> when the definition changed; otherwise, <see langword="false"/>.</returns>
    public bool DeleteColumn(int columnIndex, NodeColumnDeleteRule rule)
    {
        if (rule == NodeColumnDeleteRule.Cancel || !CanDeleteColumn(columnIndex, rule))
            return false;

        EnsureModelStructure();
        var orderedColumns = GetOrderedColumns();
        var sourceColumn = orderedColumns[columnIndex];

        if (SelectedComponent != null)
            ClearComponentSelection();

        if (rule is NodeColumnDeleteRule.MoveLeft or NodeColumnDeleteRule.MoveRight)
        {
            var targetIndex = rule == NodeColumnDeleteRule.MoveLeft ? columnIndex - 1 : columnIndex + 1;
            var targetColumn = orderedColumns[targetIndex];

            foreach (var row in sourceColumn.Rows.OrderBy(row => row.YOrder))
                targetColumn.Rows.Add(row);

            NormalizeRows(targetColumn);
            orderedColumns.RemoveAt(columnIndex);
            _definition.Columns = orderedColumns;
            NormalizeColumns();
            SetSelectedColumn(targetColumn);
        }
        else
        {
            var deletedComponents = sourceColumn.Rows
                .SelectMany(row => row.Components)
                .ToHashSet();
            RemoveComponentViewModels(deletedComponents);

            orderedColumns.RemoveAt(columnIndex);
            _definition.Columns = orderedColumns;
            NormalizeColumns();

            var fallbackIndex = orderedColumns.Count == 0
                ? -1
                : Math.Clamp(columnIndex, 0, orderedColumns.Count - 1);
            SetSelectedColumn(fallbackIndex >= 0 ? orderedColumns[fallbackIndex] : null);
        }

        SynchronizeComponentPlacementFromModel();
        OnPropertyChanged(nameof(ColumnCount));
        NotifyDocumentChanged();
        return true;
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void Delete()
    {
        if (IsExternal)
            return;

        if (Parent is NodeLibraryVM library)
            library.RemoveDefinition(this);
    }

    private void NotifyDocumentChanged()
    {
        OnDocumentChanged?.Invoke();
    }

    private bool ApplyDefinitionChange<T>(string description, T oldValue, T newValue, Action<T> applyValue)
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
        NotifyDocumentChanged();
        return true;
    }

    private void OnSelectedComponentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName) || e.PropertyName == nameof(BaseVM.DisplayName))
            OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
    }

    private void MakeEditTabs()
    {
        _generalForm.Inputs = new System.Collections.ObjectModel.ObservableCollection<DataInputWrapperVM>(CreateGeneralInputs());
        _headerForm.Inputs = new System.Collections.ObjectModel.ObservableCollection<DataInputWrapperVM>(CreateHeaderInputs());
        _styleForm.Inputs = new System.Collections.ObjectModel.ObservableCollection<DataInputWrapperVM>(CreateStyleInputs());
        RebuildColumnEditTabs();
    }

    private bool SetComponentSelection(IReadOnlyList<NodeComponentVM> selectedComponents, NodeComponentVM? primaryComponent)
    {
        selectedComponents ??= [];

        if (_selectedComponents.Count == selectedComponents.Count
            && _selectedComponents.SequenceEqual(selectedComponents)
            && ReferenceEquals(SelectedComponent, primaryComponent))
        {
            return false;
        }

        var selectedSet = selectedComponents.ToHashSet();
        foreach (var component in _selectedComponents)
        {
            if (!selectedSet.Contains(component))
                component.IsSelected = false;
        }

        foreach (var component in selectedComponents)
            component.IsSelected = true;

        _selectedComponents.Clear();
        foreach (var component in selectedComponents)
            _selectedComponents.Add(component);

        SetPrimarySelectedComponent(primaryComponent != null && selectedSet.Contains(primaryComponent)
            ? primaryComponent
            : _selectedComponents.LastOrDefault());
        OnComponentSelectionChanged();
        return true;
    }

    private void SetPrimarySelectedComponent(NodeComponentVM? component)
    {
        if (ReferenceEquals(SelectedComponent, component))
            return;

        if (SelectedComponent != null)
            SelectedComponent.PropertyChanged -= OnSelectedComponentPropertyChanged;

        SelectedComponent = component;

        if (SelectedComponent != null)
            SelectedComponent.PropertyChanged += OnSelectedComponentPropertyChanged;
    }

    private void OnComponentSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedComponents));
        OnPropertyChanged(nameof(SelectedComponentCount));
        OnPropertyChanged(nameof(HasMultipleComponentSelection));
        OnPropertyChanged(nameof(EditTabs));
        OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
    }

    private void RemoveSelectedComponentReference(NodeComponentVM component)
    {
        if (!_selectedComponents.Contains(component))
            return;

        var remainingComponents = _selectedComponents
            .Where(selectedComponent => !ReferenceEquals(selectedComponent, component))
            .ToList();
        var nextPrimaryComponent = ReferenceEquals(SelectedComponent, component)
            ? remainingComponents.LastOrDefault()
            : SelectedComponent;
        SetComponentSelection(remainingComponents, nextPrimaryComponent);
    }

    private bool SetSelectedColumn(NodeColumn? column)
    {
        if (ReferenceEquals(SelectedColumn, column))
            return false;

        SelectedColumn = column;
        RebuildColumnEditTabs();
        OnPropertyChanged(nameof(EditTabs));
        OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
        OnPropertyChanged(nameof(SelectedColumnIndex));
        return true;
    }

    private void RebuildColumnEditTabs()
    {
        _columnForm.Inputs = new ObservableCollection<DataInputWrapperVM>(CreateColumnInputs());
    }

    private string GetColumnDisplayName(NodeColumn column)
    {
        var orderedColumns = GetOrderedColumns();
        var index = orderedColumns.IndexOf(column);
        return index >= 0
            ? $"Column {index + 1}"
            : "Column";
    }

    private void LoadComponentsFromModel()
    {
        _nodeComponents.Clear();

        foreach (var column in GetOrderedColumns())
        {
            foreach (var row in column.Rows.OrderBy(item => item.YOrder))
            {
                foreach (var component in row.Components)
                {
                    var componentVm = CreateComponentVm(component);
                    componentVm.SetComponentChangedCallback(NotifyDocumentChanged);
                    componentVm.SetHistoryRecorder(_recordHistory);
                    _nodeComponents.Add(componentVm);
                }
            }
        }

        SynchronizeComponentPlacementFromModel();
    }

    private static NodeComponentVM CreateComponentVm(NodeComponent component)
    {
        return component switch
        {
            NodeText text => new NodeLabelComponentVM(text),
            NodeInput input => new NodeInputComponentVM(input),
            NodeValueDisplay valueDisplay => new NodeValueDisplayComponentVM(valueDisplay),
            NodeButton button => new NodeButtonComponentVM(button),
            NodeSeparator separator => new NodeSeparatorComponentVM(separator),
            NodeSpacer spacer => new NodeSpacerComponentVM(spacer),
            NodePort port => new NodePortComponentVM(port),
            NodeGridInput gridInput => new NodeGridInputComponentVM(gridInput),
            NodeIndicator indicator => new NodeIndicatorComponentVM(indicator),
            NodeIconButton iconButton => new NodeIconButtonComponentVM(iconButton),
            _ => throw new NotSupportedException($"The node component type '{component.GetType().Name}' is not supported."),
        };
    }

    private void CollapseRemovedColumns(int oldColumnCount, int newColumnCount)
    {
        EnsureModelStructure();
        if (newColumnCount >= oldColumnCount)
            return;

        var orderedColumns = GetOrderedColumns();
        var targetColumn = orderedColumns[Math.Max(0, newColumnCount - 1)];

        for (var sourceIndex = newColumnCount; sourceIndex < oldColumnCount; sourceIndex++)
        {
            var sourceColumn = orderedColumns[sourceIndex];
            foreach (var row in sourceColumn.Rows.OrderBy(row => row.YOrder))
                targetColumn.Rows.Add(row);
        }

        for (var index = 0; index < targetColumn.Rows.Count; index++)
            targetColumn.Rows[index].YOrder = index;

        _definition.Columns = orderedColumns.Take(newColumnCount).ToList();
        NormalizeColumns();
        NormalizeRows(targetColumn);
        SynchronizeComponentPlacementFromModel();
    }

    private void EnsureModelStructure()
    {
        _definition.Columns ??= [];
        if (_definition.Columns.Count == 0)
            _definition.Columns.Add(CreateColumn(0));

        NormalizeColumns();
        foreach (var column in _definition.Columns)
        {
            column.Rows ??= [];
            foreach (var row in column.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.Id))
                    row.Id = Guid.NewGuid().ToString("N");

                row.Components ??= [];
                row.ColumnSpan = Math.Max(1, row.ColumnSpan);
            }

            NormalizeRows(column);
        }
    }

    private void NormalizeColumns()
    {
        var orderedColumns = _definition.Columns
            .OrderBy(column => column.XOrder)
            .ThenBy(column => column.Id, StringComparer.Ordinal)
            .ToList();
        for (var index = 0; index < orderedColumns.Count; index++)
            orderedColumns[index].XOrder = index;

        _definition.Columns = orderedColumns;
    }

    private static NodeColumn CreateColumn(int xOrder)
    {
        return new NodeColumn
        {
            Id = Guid.NewGuid().ToString("N"),
            XOrder = Math.Max(0, xOrder),
            WidthMode = NodeColumnWidthMode.Auto,
        };
    }

    private static NodeRow CreateRow(IEnumerable<NodeComponent> components)
    {
        return new NodeRow
        {
            Id = Guid.NewGuid().ToString("N"),
            ColumnSpan = 1,
            Components = components.ToList(),
        };
    }

    private List<NodeColumn> GetOrderedColumns()
    {
        return _definition.Columns
            .OrderBy(column => column.XOrder)
            .ThenBy(column => column.Id, StringComparer.Ordinal)
            .ToList();
    }

    private static void NormalizeRows(NodeColumn column)
    {
        var orderedRows = column.Rows
            .OrderBy(row => row.YOrder)
            .ThenBy(row => row.Id, StringComparer.Ordinal)
            .ToList();
        for (var rowIndex = 0; rowIndex < orderedRows.Count; rowIndex++)
        {
            orderedRows[rowIndex].YOrder = rowIndex;
            NormalizeRow(orderedRows[rowIndex]);
        }

        column.Rows = orderedRows;
    }

    private static void NormalizeRow(NodeRow row)
    {
        var orderedComponents = row.Components
            .OrderBy(component => IsButtonLikeComponent(component) ? component.XOrder : 0)
            .ToList();
        for (var index = 0; index < orderedComponents.Count; index++)
            orderedComponents[index].XOrder = IsButtonLikeComponent(orderedComponents[index]) ? index : 0;

        row.Components = orderedComponents;
    }

    private bool RemoveComponentFromStructure(NodeComponent component, out NodeColumn? sourceColumn)
    {
        sourceColumn = null;

        foreach (var column in _definition.Columns)
        {
            foreach (var row in column.Rows.ToList())
            {
                if (!row.Components.Remove(component))
                    continue;

                sourceColumn = column;
                if (row.Components.Count == 0)
                    column.Rows.Remove(row);

                return true;
            }
        }

        return false;
    }

    private void RemoveComponentViewModels(ISet<NodeComponent> deletedComponents)
    {
        if (deletedComponents.Count == 0)
            return;

        foreach (var componentVm in _nodeComponents
                     .Where(componentVm => deletedComponents.Contains(componentVm.Model))
                     .ToList())
        {
            componentVm.SetComponentChangedCallback(null);
            _nodeComponents.Remove(componentVm);
        }
    }

    private void SynchronizeComponentPlacementFromModel()
    {
        var componentVms = _nodeComponents.ToDictionary(componentVm => componentVm.Model);
        var orderedColumns = GetOrderedColumns();

        for (var columnIndex = 0; columnIndex < orderedColumns.Count; columnIndex++)
        {
            var column = orderedColumns[columnIndex];
            NormalizeRows(column);

            for (var rowIndex = 0; rowIndex < column.Rows.Count; rowIndex++)
            {
                var row = column.Rows[rowIndex];
                for (var componentIndex = 0; componentIndex < row.Components.Count; componentIndex++)
                {
                    var component = row.Components[componentIndex];
                    component.Column = columnIndex;
                    component.ParentId = row.Id;
                    component.YOrder = rowIndex;
                    component.XOrder = IsButtonLikeComponent(component) ? componentIndex : 0;

                    if (!componentVms.TryGetValue(component, out var componentVm))
                        continue;

                    componentVm.SetColumn(columnIndex, notifyComponentChanged: false);
                    componentVm.SetParentId(row.Id, notifyComponentChanged: false);
                    componentVm.SetYOrder(rowIndex, notifyComponentChanged: false);
                    componentVm.SetXOrder(IsButtonLikeComponent(componentVm) ? componentIndex : 0, notifyComponentChanged: false);
                }
            }
        }
    }

    private static bool IsButtonLikeComponent(NodeComponentVM component)
    {
        return component is NodeButtonComponentVM or NodeIconButtonComponentVM;
    }

    private static bool IsButtonLikeComponent(NodeComponent component) => component is NodeButton or NodeIconButton;

    private List<DataInputWrapperVM> CreateGeneralInputs()
    {
        return
        [
            new InputStringVM("Name:", _definition.DisplayName, displayName =>
            {
                DisplayNameText = displayName ?? string.Empty;
            }),

            new InputStringVM("Domain Type:", _definition.DomainType, domainType =>
            {
                DomainType = domainType ?? string.Empty;
            }),

            new InputEnumVM("Node Shape:", _definition.NodeShape, shape =>
            {
                if (shape == null)
                    return;

                NodeShape = (NodeShape)shape;
            }),

            new InputBoolVM("Has Overview:", _definition.HasOverview, hasOverview =>
            {
                HasOverview = hasOverview;
            }),
        ];
    }

    private List<DataInputWrapperVM> CreateColumnInputs()
    {
        if (SelectedColumn == null)
            return [];

        InputFloatVM? widthInput = null;
        widthInput = new InputFloatVM("Width (nu):", SelectedColumn.WidthNu, widthNu =>
        {
            SelectedColumn.WidthNu = MathF.Max(0.0f, widthNu);
        })
        {
            Hide = SelectedColumn.WidthMode != NodeColumnWidthMode.Fixed,
        };

        return
        [
            new InputEnumVM("Width Mode:", SelectedColumn.WidthMode, widthMode =>
            {
                if (widthMode is not NodeColumnWidthMode resolvedWidthMode)
                    return;

                SelectedColumn.WidthMode = resolvedWidthMode;
                if (widthInput != null)
                    widthInput.Hide = resolvedWidthMode != NodeColumnWidthMode.Fixed;
            }),

            widthInput,
        ];
    }

    private List<DataInputWrapperVM> CreateHeaderInputs()
    {
        return
        [
            new InputStringVM("Header Text Override:", _definition.HeaderText, headerText =>
            {
                HeaderText = headerText ?? string.Empty;
            }),

            new InputColorVM("Background Color:", HeaderStyle.BackgroundColor.ToMediaColor(), color =>
            {
                HeaderStyle.BackgroundColor = color.ToNodeColor();
            }),

            new InputEnumVM("Alignment:", HeaderStyle.Alignment, alignment =>
            {
                if (alignment == null)
                    return;

                HeaderStyle.Alignment = (NodeTextAlignment)alignment;
            }),

            new InputEnumVM("Font Weight:", HeaderStyle.FontWeight, fontWeight =>
            {
                if (fontWeight == null)
                    return;

                HeaderStyle.FontWeight = (NodeFontWeight)fontWeight;
            }),
        ];
    }

    private List<DataInputWrapperVM> CreateStyleInputs()
    {
        return
        [
            new InputColorVM("Background Color:", ShapeStyle.BackgroundColor.ToMediaColor(), color =>
            {
                ShapeStyle.BackgroundColor = color.ToNodeColor();
            }),

            new InputColorVM("Border Color:", ShapeStyle.BorderColor.ToMediaColor(), color =>
            {
                ShapeStyle.BorderColor = color.ToNodeColor();
            }),

            new InputFloatVM("Border Thickness:", ShapeStyle.BorderThickness, thickness =>
            {
                ShapeStyle.BorderThickness = thickness;
            }),

            new InputEnumVM("Font Weight:", ShapeStyle.FontWeight, fontWeight =>
            {
                if (fontWeight == null)
                    return;

                ShapeStyle.FontWeight = (NodeFontWeight)fontWeight;
            }),
        ];
    }

    private NodeShapeStyle ShapeStyle
    {
        get
        {
            _definition.Style ??= new NodeStyle();
            return _definition.Style.ShapeStyle;
        }
    }

    private NodeHeaderStyle HeaderStyle
    {
        get
        {
            _definition.Style ??= new NodeStyle();
            _definition.Style.HeaderStyle ??= new NodeHeaderStyle();
            return _definition.Style.HeaderStyle;
        }
    }

    #endregion Private Methods
}
