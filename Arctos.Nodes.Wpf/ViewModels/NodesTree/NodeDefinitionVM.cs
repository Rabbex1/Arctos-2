using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Wpf;
using Arctos.Wpf.Inputs;
using Arctos.Wpf.Utilities;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Editable tree view model wrapper over a <see cref="NodeDefinition"/>.
/// </summary>
public sealed class NodeDefinitionVM : NodeTreeItemVM<INodeTreeItemVM>, IEditTabs, IModelHost<NodeDefinition>, ICanvasViewStateHost
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly NodeDefinition _definition;
    private readonly ObservableCollection<NodeComponentVM> _nodeComponents = [];
    private readonly InputFormVM _generalForm = new();
    private readonly InputFormVM _headerForm = new();
    private readonly InputFormVM _styleForm = new();
    private readonly IReadOnlyList<EditTabVM> _editTabs;
    private ICommand? _deleteCommand;
    private NodeComponentVM? _selectedComponent;
    private NodePoint _canvasPan;
    private float _canvasZoom;

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
            if (string.Equals(_definition.DisplayName, value, StringComparison.Ordinal))
                return;

            _definition.DisplayName = value;
            DisplayName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ResolvedHeaderText));
            OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
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

            if (string.Equals(_definition.HeaderText, headerText, StringComparison.Ordinal))
                return;

            _definition.HeaderText = headerText;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ResolvedHeaderText));
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
            if (string.Equals(_definition.DomainType, value, StringComparison.Ordinal))
                return;

            _definition.DomainType = value;
            OnPropertyChanged();
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
            if (_definition.NodeShape == value)
                return;

            _definition.NodeShape = value;
            _definition.Size = NodeSize.CreateDefault(value);
            _definition.ShapeRules = ShapeRules.CreateDefault(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(ShapeRules));
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
            if (_definition.HasOverview == value)
                return;

            _definition.HasOverview = value;
            OnPropertyChanged();
        }
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
    public IReadOnlyList<EditTabVM> EditTabs => SelectedComponent?.EditTabs ?? _editTabs;

    /// <summary>
    /// Gets the currently selected component inside the detailed node editor.
    /// </summary>
    public NodeComponentVM? SelectedComponent
    {
        get => _selectedComponent;
        private set => SetProperty(ref _selectedComponent, value);
    }

    /// <summary>
    /// Gets the display name for the active edit target.
    /// </summary>
    public string CurrentEditTargetDisplayName => SelectedComponent?.DisplayName ?? DisplayName ?? string.Empty;

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
        _editTabs =
        [
            new EditTabVM("General", _generalForm),
            new EditTabVM("Header", _headerForm),
            new EditTabVM("Style", _styleForm),
        ];
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
    /// Adds a temporary component instance to the current definition.
    /// </summary>
    /// <param name="component">The component instance to add.</param>
    public void AddComponent(NodeComponentVM component)
    {
        ArgumentNullException.ThrowIfNull(component);

        component.SetComponentChangedCallback(NotifyDocumentChanged);
        _nodeComponents.Add(component);
        _definition.Components.Add(component.Model);
        SelectComponent(component);
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
        if (ReferenceEquals(SelectedComponent, component))
            return false;

        if (SelectedComponent != null)
        {
            SelectedComponent.IsSelected = false;
            SelectedComponent.PropertyChanged -= OnSelectedComponentPropertyChanged;
        }

        SelectedComponent = component;

        if (SelectedComponent != null)
        {
            SelectedComponent.IsSelected = true;
            SelectedComponent.PropertyChanged += OnSelectedComponentPropertyChanged;
        }

        OnPropertyChanged(nameof(EditTabs));
        OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
        return true;
    }

    /// <summary>
    /// Clears the current component selection.
    /// </summary>
    /// <returns><see langword="true"/> when the selection changed; otherwise, <see langword="false"/>.</returns>
    public bool ClearComponentSelection()
    {
        return SelectComponent(null);
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

        if (ReferenceEquals(SelectedComponent, component))
            ClearComponentSelection();

        component.SetComponentChangedCallback(null);
        _nodeComponents.Remove(component);
        _definition.Components.Remove(component.Model);
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
    }

    private void LoadComponentsFromModel()
    {
        _nodeComponents.Clear();

        foreach (var component in _definition.Components)
        {
            var componentVm = CreateComponentVm(component);
            componentVm.SetComponentChangedCallback(NotifyDocumentChanged);
            _nodeComponents.Add(componentVm);
        }
    }

    private static NodeComponentVM CreateComponentVm(NodeComponent component)
    {
        return component switch
        {
            NodeText text => new NodeLabelComponentVM(text),
            NodeInput input => new NodeInputComponentVM(input),
            NodeButton button => new NodeButtonComponentVM(button),
            NodeSeparator separator => new NodeSeparatorComponentVM(separator),
            NodeSpacer spacer => new NodeSpacerComponentVM(spacer),
            NodePort port => new NodePortComponentVM(port),
            _ => throw new NotSupportedException($"The node component type '{component.GetType().Name}' is not supported."),
        };
    }

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
