using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Layout;
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
    public string DisplayNameText => _definition.DisplayName ?? string.Empty;

    /// <summary>
    /// Gets or sets the optional header text override.
    /// </summary>
    public string HeaderText => _definition.HeaderText ?? string.Empty;

    /// <summary>
    /// Gets the effective header text, using the node name when no override is defined.
    /// </summary>
    public string ResolvedHeaderText => string.IsNullOrWhiteSpace(_definition.HeaderText)
        ? DisplayNameText
        : _definition.HeaderText;

    /// <summary>
    /// Gets or sets the application-defined domain type for the definition.
    /// </summary>
    public string DomainType => _definition.DomainType;

    /// <summary>
    /// Gets or sets the primitive editor shape used by the definition.
    /// </summary>
    public NodeShape NodeShape => _definition.NodeShape;

    /// <summary>
    /// Gets the intrinsic node size associated with the current definition.
    /// </summary>
    public NodeSize Size => _definition.Size;

    /// <summary>
    /// Gets or sets a value indicating whether the definition supports an overview mode.
    /// </summary>
    public bool HasOverview => _definition.HasOverview;

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

            NormalizePortRowsToEdges();

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
        var projection = NodeDragProjectionBuilder.Build(this);
        projection = NodeDragProjectionBuilder.InsertColumn(projection, columnIndex);
        var insertedColumnId = projection.Columns
            .OrderBy(column => column.Index)
            .ElementAt(Math.Clamp(columnIndex, 0, projection.Columns.Count - 1))
            .Id;
        ApplyDragProjection(projection, notifyDocumentChanged: false);
        NormalizePortRowsToEdges();
        if (SelectedComponent != null)
            SelectComponent(null);

        var newColumn = GetOrderedColumns().FirstOrDefault(column => column.Id == insertedColumnId);
        SetSelectedColumn(newColumn ?? GetOrderedColumns()[Math.Clamp(columnIndex, 0, GetOrderedColumns().Count - 1)]);
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

        var movedColumnId = orderedColumns[resolvedSourceIndex].Id;
        var targetSide = ResolveOppositeEdgePortSide(orderedColumns.Count, resolvedSourceIndex, resolvedTargetIndex);
        var projection = NodeDragProjectionBuilder.Build(this);
        projection = NodeDragProjectionBuilder.MoveColumn(projection, resolvedSourceIndex, resolvedTargetIndex);
        ApplyDragProjection(projection, notifyDocumentChanged: false);
        NormalizeMovedColumnPorts(movedColumnId, targetSide);
        NormalizePortRowsToEdges();

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
    /// Replaces the persisted row/component structure from a projected drag result.
    /// </summary>
    /// <param name="projection">The projected drag structure to apply.</param>
    /// <param name="notifyDocumentChanged"><see langword="true"/> to notify the owning document; otherwise, <see langword="false"/>.</param>
    internal void ApplyDragProjection(NodeDragProjection projection, bool notifyDocumentChanged = true)
    {
        ArgumentNullException.ThrowIfNull(projection);
        EnsureModelStructure();

        var orderedColumns = GetOrderedColumns();
        var columnsById = orderedColumns.ToDictionary(column => column.Id, StringComparer.Ordinal);
        var rewrittenColumns = projection.Columns
            .OrderBy(column => column.Index)
            .Select(projectedColumn =>
            {
                if (!columnsById.TryGetValue(projectedColumn.Id, out var existingColumn))
                {
                    existingColumn = new NodeColumn
                    {
                        Id = projectedColumn.Id,
                        WidthMode = NodeColumnWidthMode.Auto,
                        Rows = [],
                    };
                }

                existingColumn.XOrder = projectedColumn.Index;
                return existingColumn;
            })
            .ToList();
        _definition.Columns = rewrittenColumns;
        columnsById = rewrittenColumns.ToDictionary(column => column.Id, StringComparer.Ordinal);

        foreach (var projectedColumn in projection.Columns.OrderBy(column => column.Index))
        {
            if (!columnsById.TryGetValue(projectedColumn.Id, out var targetColumn))
                continue;

            var existingRowsById = targetColumn.Rows.ToDictionary(row => row.Id, StringComparer.Ordinal);
            var rewrittenRows = new List<NodeRow>();

            foreach (var projectedRow in projectedColumn.Rows.OrderBy(row => row.YOrder))
            {
                var targetRow = existingRowsById.TryGetValue(projectedRow.Id, out var existingRow)
                    ? existingRow
                    : new NodeRow
                    {
                        Id = projectedRow.Id,
                        ColumnSpan = 1,
                    };

                targetRow.Id = projectedRow.Id;
                targetRow.YOrder = rewrittenRows.Count;
                targetRow.ColumnSpan = Math.Max(1, targetRow.ColumnSpan);
                targetRow.Components = projectedRow.Components
                    .Select(component => component.Model)
                    .ToList();
                NormalizeRow(targetRow);
                rewrittenRows.Add(targetRow);
            }

            targetColumn.Rows = rewrittenRows;
            NormalizeRows(targetColumn);
        }

        NormalizeColumns();
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
            SetSelectedColumn(null);
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
            SetSelectedColumn(null);
        }

        NormalizePortRowsToEdges();
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
            NodeInput input when input.TypeId == InputTypeIds.StringSelection => new NodeDropdownComponentVM(input),
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
        NormalizePortRowsToEdges();
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

    private static NodePortSide? ResolveOppositeEdgePortSide(int columnCount, int sourceIndex, int targetIndex)
    {
        var lastColumnIndex = Math.Max(0, columnCount - 1);

        if (sourceIndex == 0 && targetIndex == lastColumnIndex)
            return NodePortSide.Right;

        if (sourceIndex == lastColumnIndex && targetIndex == 0)
            return NodePortSide.Left;

        return null;
    }

    private void NormalizeMovedColumnPorts(string movedColumnId, NodePortSide? targetSide)
    {
        if (string.IsNullOrWhiteSpace(movedColumnId) || targetSide == null)
            return;

        var targetColumn = _definition.Columns.FirstOrDefault(column => string.Equals(column.Id, movedColumnId, StringComparison.Ordinal));
        if (targetColumn == null)
            return;

        var componentVms = _nodeComponents.ToDictionary(componentVm => componentVm.Model);

        foreach (var port in targetColumn.Rows
                     .SelectMany(row => row.Components)
                     .OfType<NodePort>())
        {
            if (port.Side == targetSide.Value)
                continue;

            port.Side = targetSide.Value;

            if (componentVms.TryGetValue(port, out var componentVm)
                && componentVm is NodePortComponentVM portVm)
            {
                portVm.SetSide(targetSide.Value, notifyComponentChanged: false);
            }
        }
    }

    private void NormalizePortRowsToEdges()
    {
        var orderedColumns = GetOrderedColumns();
        if (orderedColumns.Count <= 1)
            return;

        var leftColumn = orderedColumns[0];
        var rightColumn = orderedColumns[^1];
        var leftRowsToMove = new List<NodeRow>();
        var rightRowsToMove = new List<NodeRow>();
        var componentVms = _nodeComponents.ToDictionary(componentVm => componentVm.Model);

        foreach (var column in orderedColumns)
        {
            foreach (var row in column.Rows.OrderBy(row => row.YOrder).ToList())
            {
                var portSide = GetOwnedPortSide(row);
                if (portSide == null)
                    continue;

                if (ReferenceEquals(column, leftColumn))
                {
                    SetPortSide(row, NodePortSide.Left, componentVms);
                    continue;
                }

                if (ReferenceEquals(column, rightColumn))
                {
                    SetPortSide(row, NodePortSide.Right, componentVms);
                    continue;
                }

                if (portSide == NodePortSide.Left)
                {
                    column.Rows.Remove(row);
                    leftRowsToMove.Add(row);
                }
                else if (portSide == NodePortSide.Right)
                {
                    column.Rows.Remove(row);
                    rightRowsToMove.Add(row);
                }
            }
        }

        if (leftRowsToMove.Count == 0 && rightRowsToMove.Count == 0)
            return;

        leftColumn.Rows.AddRange(leftRowsToMove);
        if (!ReferenceEquals(leftColumn, rightColumn))
            rightColumn.Rows.AddRange(rightRowsToMove);

        foreach (var row in leftRowsToMove)
            SetPortSide(row, NodePortSide.Left, componentVms);

        foreach (var row in rightRowsToMove)
            SetPortSide(row, NodePortSide.Right, componentVms);

        foreach (var column in orderedColumns)
            NormalizeRows(column);

        SynchronizeComponentPlacementFromModel();
    }

    private static NodePortSide? GetOwnedPortSide(NodeRow row)
    {
        var ports = row.Components.OfType<NodePort>().ToList();
        if (ports.Count == 0)
            return null;

        var side = ports[0].Side;
        return ports.All(port => port.Side == side)
            ? side
            : null;
    }

    private static void SetPortSide(NodeRow row, NodePortSide side, IReadOnlyDictionary<NodeComponent, NodeComponentVM> componentVms)
    {
        foreach (var port in row.Components.OfType<NodePort>())
        {
            if (port.Side == side)
                continue;

            port.Side = side;

            if (componentVms.TryGetValue(port, out var componentVm)
                && componentVm is NodePortComponentVM portVm)
            {
                portVm.SetSide(side, notifyComponentChanged: false);
            }
        }
    }

    private List<DataInputWrapperVM> CreateGeneralInputs()
    {
        return
        [
            new InputStringVM("Name:", _definition.DisplayName, (input, displayName) =>
            {
                var value = displayName ?? string.Empty;
                ApplyDefinitionChange(
                    "Change node name",
                    _definition.DisplayName,
                    value,
                    resolvedDisplayName =>
                    {
                        _definition.DisplayName = resolvedDisplayName;
                        input.UpdateValue(resolvedDisplayName);
                        DisplayName = resolvedDisplayName;
                        OnPropertyChanged(nameof(DisplayNameText));
                        OnPropertyChanged(nameof(ResolvedHeaderText));
                        OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
                    });
            }),

            new InputStringVM("Domain Type:", _definition.DomainType, (input, domainType) =>
            {
                var value = domainType ?? string.Empty;
                ApplyDefinitionChange(
                    "Change domain type",
                    _definition.DomainType,
                    value,
                    resolvedDomainType =>
                    {
                        _definition.DomainType = resolvedDomainType;
                        input.UpdateValue(resolvedDomainType);
                        OnPropertyChanged(nameof(DomainType));
                    });
            }),

            new InputEnumVM("Node Shape:", _definition.NodeShape, (input, shape) =>
            {
                if (shape is not NodeShape resolvedShape)
                    return;

                ApplyDefinitionChange(
                    "Change node shape",
                    _definition.NodeShape,
                    resolvedShape,
                    nodeShape =>
                    {
                        _definition.NodeShape = nodeShape;
                        _definition.Size = NodeSize.CreateDefault(nodeShape);
                        _definition.ShapeRules = ShapeRules.CreateDefault(nodeShape);
                        input.UpdateValue(nodeShape);
                        OnPropertyChanged(nameof(NodeShape));
                        OnPropertyChanged(nameof(Size));
                        OnPropertyChanged(nameof(ShapeRules));
                    });
            }),

            new InputBoolVM("Has Overview:", _definition.HasOverview, (input, hasOverview) =>
            {
                ApplyDefinitionChange(
                    "Change overview mode",
                    _definition.HasOverview,
                    hasOverview,
                    resolvedHasOverview =>
                    {
                        _definition.HasOverview = resolvedHasOverview;
                        input.UpdateValue(resolvedHasOverview);
                        OnPropertyChanged(nameof(HasOverview));
                    });
            }),
        ];
    }

    private List<DataInputWrapperVM> CreateColumnInputs()
    {
        if (SelectedColumn == null)
            return [];

        InputFloatVM? widthInput = null;
        widthInput = new InputFloatVM("Width (nu):", SelectedColumn.WidthNu, (input, widthNu) =>
        {
            var value = MathF.Max(0.0f, widthNu);
            ApplyDefinitionChange(
                "Change column width",
                SelectedColumn.WidthNu,
                value,
                resolvedWidth =>
                {
                    if (SelectedColumn == null)
                        return;

                    SelectedColumn.WidthNu = resolvedWidth;
                    input.UpdateValue(resolvedWidth);
                });
        })
        {
            Hide = SelectedColumn.WidthMode != NodeColumnWidthMode.Fixed,
        };

        return
        [
            new InputEnumVM("Width Mode:", SelectedColumn.WidthMode, (input, widthMode) =>
            {
                if (widthMode is not NodeColumnWidthMode resolvedWidthMode)
                    return;

                var previousWidthMode = SelectedColumn.WidthMode;
                ApplyDefinitionChange(
                    "Change column width mode",
                    previousWidthMode,
                    resolvedWidthMode,
                    widthModeValue =>
                    {
                        if (SelectedColumn == null)
                            return;

                        SelectedColumn.WidthMode = widthModeValue;
                        input.UpdateValue(widthModeValue);
                        if (widthInput != null)
                            widthInput.Hide = widthModeValue != NodeColumnWidthMode.Fixed;
                    });
            }),

            widthInput,
        ];
    }

    private List<DataInputWrapperVM> CreateHeaderInputs()
    {
        return
        [
            new InputStringVM("Header Text Override:", _definition.HeaderText, (input, headerText) =>
            {
                var value = string.IsNullOrWhiteSpace(headerText) ? null : headerText;
                ApplyDefinitionChange(
                    "Change header text",
                    _definition.HeaderText,
                    value,
                    resolvedHeaderText =>
                    {
                        _definition.HeaderText = resolvedHeaderText;
                        input.UpdateValue(resolvedHeaderText);
                        OnPropertyChanged(nameof(HeaderText));
                        OnPropertyChanged(nameof(ResolvedHeaderText));
                    });
            }),

            new InputColorVM("Background Color:", HeaderStyle.BackgroundColor.ToMediaColor(), (input, color) =>
            {
                ApplyDefinitionChange(
                    "Change header background color",
                    HeaderStyle.BackgroundColor.ToMediaColor(),
                    color,
                    resolvedColor =>
                    {
                        HeaderStyle.BackgroundColor = resolvedColor.ToNodeColor();
                        input.UpdateValue(resolvedColor);
                    });
            }),

            new InputEnumVM("Alignment:", HeaderStyle.Alignment, (input, alignment) =>
            {
                if (alignment is not NodeTextAlignment resolvedAlignment)
                    return;

                ApplyDefinitionChange(
                    "Change header alignment",
                    HeaderStyle.Alignment,
                    resolvedAlignment,
                    alignmentValue =>
                    {
                        HeaderStyle.Alignment = alignmentValue;
                        input.UpdateValue(alignmentValue);
                    });
            }),

            new InputEnumVM("Font Weight:", HeaderStyle.FontWeight, (input, fontWeight) =>
            {
                if (fontWeight is not NodeFontWeight resolvedFontWeight)
                    return;

                ApplyDefinitionChange(
                    "Change header font weight",
                    HeaderStyle.FontWeight,
                    resolvedFontWeight,
                    fontWeightValue =>
                    {
                        HeaderStyle.FontWeight = fontWeightValue;
                        input.UpdateValue(fontWeightValue);
                    });
            }),
        ];
    }

    private List<DataInputWrapperVM> CreateStyleInputs()
    {
        return
        [
            new InputColorVM("Background Color:", ShapeStyle.BackgroundColor.ToMediaColor(), (input, color) =>
            {
                ApplyDefinitionChange(
                    "Change node background color",
                    ShapeStyle.BackgroundColor.ToMediaColor(),
                    color,
                    resolvedColor =>
                    {
                        ShapeStyle.BackgroundColor = resolvedColor.ToNodeColor();
                        input.UpdateValue(resolvedColor);
                    });
            }),

            new InputColorVM("Border Color:", ShapeStyle.BorderColor.ToMediaColor(), (input, color) =>
            {
                ApplyDefinitionChange(
                    "Change node border color",
                    ShapeStyle.BorderColor.ToMediaColor(),
                    color,
                    resolvedColor =>
                    {
                        ShapeStyle.BorderColor = resolvedColor.ToNodeColor();
                        input.UpdateValue(resolvedColor);
                    });
            }),

            new InputFloatVM("Border Thickness:", ShapeStyle.BorderThickness, (input, thickness) =>
            {
                ApplyDefinitionChange(
                    "Change border thickness",
                    ShapeStyle.BorderThickness,
                    thickness,
                    resolvedThickness =>
                    {
                        ShapeStyle.BorderThickness = resolvedThickness;
                        input.UpdateValue(resolvedThickness);
                    });
            }),

            new InputEnumVM("Font Weight:", ShapeStyle.FontWeight, (input, fontWeight) =>
            {
                if (fontWeight is not NodeFontWeight resolvedFontWeight)
                    return;

                ApplyDefinitionChange(
                    "Change node font weight",
                    ShapeStyle.FontWeight,
                    resolvedFontWeight,
                    fontWeightValue =>
                    {
                        ShapeStyle.FontWeight = fontWeightValue;
                        input.UpdateValue(fontWeightValue);
                    });
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
