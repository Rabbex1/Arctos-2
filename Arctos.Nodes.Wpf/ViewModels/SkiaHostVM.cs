using Arctos;
using Arctos.History;
using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Builders;
using Arctos.Nodes.Wpf.DragImage;
using Arctos.Nodes.Wpf.Layout;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels.Dialogs;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Nodes.Wpf.Views.Dialogs;
using Arctos.Nodes.Wpf.Visuals;
using Arctos.Wpf.DragDrop;
using Arctos.Wpf;
using Arctos.Wpf.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows.Input;
using System.Windows;
using DragDropEffects = System.Windows.DragDropEffects;
using Point = System.Windows.Point;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Hosts a Skia rendering surface for the current node-space view.
/// </summary>
public sealed class SkiaHostVM : BaseVM, IDropable
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private const byte GridAlphaCutoff = 16;
    private const float GridPadding = 2.0f;
    private const float FitPaddingFactor = 0.75f;
    private const float MinorGridBaseOpacity = 0.09f;
    private const float MainGridBaseOpacity = 0.30f;
    private const float MajorGridBaseOpacity = 0.40f;
    private const float PreviewOpacity = 0.60f;
    private const float PreviewIndicatorOpacity = 0.85f;
    private const float PreviewGhostValidOpacity = 0.50f;
    private const float PreviewGhostInvalidOpacity = 0.30f;
    private static readonly DragService SharedDragService = DragService.Shared;
    private bool _isPanning;
    private bool _isLeftButtonDown;
    private bool _isDraggingSelectedComponent;
    private bool _isDraggingSelectedColumn;
    private SKPoint _lastPanScreenPosition;
    private SKPoint _leftButtonDownPosition;
    private ILayoutEngine _layoutEngine = new NodeLayoutEngine();
    private ILayoutSnapshot? _layoutSnapshot;
    private NodeComponentVM? _dragSourceComponent;
    private int? _pressedColumnIndex;
    private int? _dragSourceColumnIndex;
    private IDragFeedbackState? _activeDragFeedbackState;
    private NodeComponentVM? _hoveredComponent;
    private int? _hoveredColumnIndex;
    private NodeComponentVM? _pressedComponent;
    private NodeComponentVM? _previewComponent;
    private int? _selectedColumnIndex;
    private SKPoint _previewPointWorld = new(0.0f, 0.0f);
    private SKRect _previewPlacementBounds = SKRect.Empty;
    private float? _previewInsertionY;
    private float? _previewInsertionX;
    private SKRect _previewInlineBounds = SKRect.Empty;
    private float _previewDropYNu;
    private int _previewDropRowIndex;
    private int _previewDropColumnIndex;
    private bool _previewColumnInsert;
    private int _previewDropXOrder;
    private bool _previewInlineWithRow;
    private bool _previewDropValid;
    private ICommand? _deleteSelectedComponentCommand;
    private ICommand? _undoCommand;
    private ICommand? _redoCommand;
    private ICommand? _fitToViewCommand;
    private float _viewportHeight;
    private float _viewportWidth;
    private SKBitmap? _hitTestBitmap;
    private readonly Dictionary<int, HitTestResult> _hitEntries = [];
    private bool _hitTestLayerDirty = true;
    private bool _pendingHostViewState = true;
    private readonly DragImageRenderer _dragImageRenderer = new();

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    #region Host
    private IModelHost? _host;
    /// <summary>
    /// Gets or sets the current navigation host shown on the canvas.
    /// </summary>
    public IModelHost? Host
    {
        get => _host;
        set
        {
            SaveHostViewState();

            if (!SetProperty(ref _host, value))
                return;

            _hoveredColumnIndex = null;
            _selectedColumnIndex = null;
            _pendingHostViewState = true;
            OnPropertyChanged(nameof(Model));
            RebuildVisualTree();
        }
    }
    #endregion

    /// <summary>
    /// Gets the raw current model being shown by the canvas.
    /// </summary>
    public object? Model => _host?.Model;

    #region HistoryContext
    private IHistoryContext? _historyContext;
    /// <summary>
    /// Gets or sets the active document history context used by the canvas toolbar.
    /// </summary>
    public IHistoryContext? HistoryContext
    {
        get => _historyContext;
        set
        {
            if (ReferenceEquals(_historyContext, value))
                return;

            if (_historyContext != null)
                _historyContext.StateChanged -= OnHistoryStateChanged;

            _historyContext = value;

            if (_historyContext != null)
                _historyContext.StateChanged += OnHistoryStateChanged;

            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            CommandManager.InvalidateRequerySuggested();
        }
    }
    #endregion

    /// <summary>
    /// Gets a value indicating whether the active document can currently undo.
    /// </summary>
    public bool CanUndo => HistoryContext?.CanUndo == true;

    /// <summary>
    /// Gets a value indicating whether the active document can currently redo.
    /// </summary>
    public bool CanRedo => HistoryContext?.CanRedo == true;

    #region FontSize
    private float _fontSize = 12.0f;
    /// <summary>
    /// Gets or sets the document typography size used to derive the canvas grid spacing.
    /// </summary>
    public float FontSize
    {
        get => _fontSize;
        set
        {
            var fontSize = value <= 0.0f ? 12.0f : value;

            if (!SetProperty(ref _fontSize, fontSize))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region FontColor
    private NodeColor _fontColor = NodeColor.Black;
    /// <summary>
    /// Gets or sets the effective document text color used for headers, text components, and component labels.
    /// </summary>
    public NodeColor FontColor
    {
        get => _fontColor;
        set
        {
            if (!SetProperty(ref _fontColor, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region FontFamily
    private string? _fontFamily;
    /// <summary>
    /// Gets or sets the document typography family used to derive the canvas grid spacing.
    /// </summary>
    public string? FontFamily
    {
        get => _fontFamily;
        set
        {
            if (!SetProperty(ref _fontFamily, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region HeaderFontSize
    private float _headerFontSize = 14.0f;
    /// <summary>
    /// Gets or sets the document header typography size used for node header layout and rendering.
    /// </summary>
    public float HeaderFontSize
    {
        get => _headerFontSize;
        set
        {
            var headerFontSize = value <= 0.0f ? 14.0f : value;

            if (!SetProperty(ref _headerFontSize, headerFontSize))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region LayoutPadding
    private float _layoutPadding = 1.0f;
    /// <summary>
    /// Gets or sets the effective detailed node padding, expressed in <c>nu</c>.
    /// </summary>
    public float LayoutPadding
    {
        get => _layoutPadding;
        set
        {
            var padding = MathF.Max(0.0f, value);

            if (!SetProperty(ref _layoutPadding, padding))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region RowSpacing
    private float _rowSpacing;
    /// <summary>
    /// Gets or sets the effective detailed node row spacing, expressed in <c>nu</c>.
    /// </summary>
    public float RowSpacing
    {
        get => _rowSpacing;
        set
        {
            var rowSpacing = MathF.Max(0.0f, value);

            if (!SetProperty(ref _rowSpacing, rowSpacing))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region ColumnSpacing
    private float _columnSpacing = 1.0f;
    /// <summary>
    /// Gets or sets the effective detailed node column spacing, expressed in <c>nu</c>.
    /// </summary>
    public float ColumnSpacing
    {
        get => _columnSpacing;
        set
        {
            var columnSpacing = MathF.Max(0.0f, value);

            if (!SetProperty(ref _columnSpacing, columnSpacing))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region RoundedCornerRadius
    private float _roundedCornerRadius = 2.0f;
    /// <summary>
    /// Gets or sets the rounded rectangle corner radius, expressed in <c>nu</c>.
    /// </summary>
    public float RoundedCornerRadius
    {
        get => _roundedCornerRadius;
        set
        {
            var radius = MathF.Max(0.0f, value);

            if (!SetProperty(ref _roundedCornerRadius, radius))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region ControlCornerRadius
    private float _controlCornerRadius = 1.0f;
    /// <summary>
    /// Gets or sets the shared control corner radius, expressed in <c>nu</c>.
    /// </summary>
    public float ControlCornerRadius
    {
        get => _controlCornerRadius;
        set
        {
            var radius = MathF.Max(0.0f, value);

            if (!SetProperty(ref _controlCornerRadius, radius))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region MinimumColumnWidth
    private float _minimumColumnWidth = 24.0f;
    /// <summary>
    /// Gets or sets the effective minimum width used by a single detailed-layout content column, expressed in <c>nu</c>.
    /// </summary>
    public float MinimumColumnWidth
    {
        get => _minimumColumnWidth;
        set
        {
            var minimumWidth = MathF.Max(0.0f, value);

            if (!SetProperty(ref _minimumColumnWidth, minimumWidth))
                return;

            RebuildVisualTree();
        }
    }

    #region PortSize
    private float _portSize = 1.5f;
    public float PortSize
    {
        get => _portSize;
        set
        {
            if (!SetProperty(ref _portSize, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region PortPlacement
    private NodePortPlacement _portPlacement = NodePortPlacement.Over;
    public NodePortPlacement PortPlacement
    {
        get => _portPlacement;
        set
        {
            if (!SetProperty(ref _portPlacement, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion
    #endregion

    #region ShowGrid
    private bool _showGrid = true;
    /// <summary>
    /// Gets or sets a value indicating whether the node-space grid should be rendered.
    /// </summary>
    public bool ShowGrid
    {
        get => _showGrid;
        set
        {
            if (!SetProperty(ref _showGrid, value))
                return;

            InvalidateCanvas();
        }
    }
    #endregion

    #region ShowDebugBounds
    private bool _showDebugBounds;
    /// <summary>
    /// Gets or sets a value indicating whether component debug bounds should be rendered.
    /// </summary>
    public bool ShowDebugBounds
    {
        get => _showDebugBounds;
        set
        {
            if (!SetProperty(ref _showDebugBounds, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region Pan
    private NodePoint _pan = new(0.0f, 0.0f);
    /// <summary>
    /// Gets or sets the current node-space pan offset.
    /// </summary>
    public NodePoint Pan
    {
        get => _pan;
        set
        {
            if (!SetProperty(ref _pan, value))
                return;

            SaveHostViewState();
            MarkHitTestLayerDirty();
            InvalidateCanvas();
        }
    }
    #endregion

    #region Zoom
    private float _zoom = 1.0f;
    /// <summary>
    /// Gets or sets the current node-space zoom factor.
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set
        {
            var zoom = value <= 0.0f ? 0.01f : value;

            if (!SetProperty(ref _zoom, zoom))
                return;

            SaveHostViewState();
            OnPropertyChanged(nameof(ZoomText));
            MarkHitTestLayerDirty();
            InvalidateCanvas();
        }
    }
    #endregion

    /// <summary>
    /// Gets the current zoom formatted as a percentage for the status strip.
    /// </summary>
    public string ZoomText => $"{MathF.Round(Zoom * 100.0f):0}%";

    #region BackgroundColor
    private NodeColor _backgroundColor = NodeColor.Transparent;
    /// <summary>
    /// Gets or sets the effective root canvas background color.
    /// </summary>
    public NodeColor BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (!SetProperty(ref _backgroundColor, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region VisualRoot
    private IVisual? _visualRoot;
    /// <summary>
    /// Gets or sets the root visual for the current canvas scene.
    /// </summary>
    public IVisual? VisualRoot
    {
        get => _visualRoot;
        set => SetVisualRoot(value);
    }
    #endregion

    #region Overview
    private bool _overview;
    /// <summary>
    /// Gets or sets a value indicating whether the current host should be shown in overview mode.
    /// </summary>
    public bool Overview
    {
        get => _overview;
        set
        {
            if (!SetProperty(ref _overview, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region VisualTreeBuilder
    private VisualTreeBuilder _visualTreeBuilder = new();
    /// <summary>
    /// Gets or sets the builder used to create the current visual tree.
    /// </summary>
    public VisualTreeBuilder VisualTreeBuilder
    {
        get => _visualTreeBuilder;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!SetProperty(ref _visualTreeBuilder, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region LayoutEngine
    /// <summary>
    /// Gets or sets the layout engine used to resolve the current host layout before building the visual tree.
    /// </summary>
    public ILayoutEngine LayoutEngine
    {
        get => _layoutEngine;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!SetProperty(ref _layoutEngine, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    /// <summary>
    /// Gets the current transient layout snapshot for the active host.
    /// </summary>
    public ILayoutSnapshot? LayoutSnapshot
    {
        get => _layoutSnapshot;
        private set => SetProperty(ref _layoutSnapshot, value);
    }

    /// <summary>
    /// Gets a value indicating whether the draw mode toggle should be shown for this host.
    /// </summary>
    public bool ShowDrawModeToggle => true;

    /// <summary>
    /// Gets the command that fits the current visual content into the visible canvas area.
    /// </summary>
    public ICommand FitToViewCommand => _fitToViewCommand ??= new RelayCommand(_ => FitToView(), _ => CanFitToView());

    /// <summary>
    /// Gets the command that undoes the latest history entry for the active document.
    /// </summary>
    public ICommand UndoCommand => _undoCommand ??= new RelayCommand(
        _ => HistoryContext?.Undo(),
        _ => HistoryContext?.CanUndo == true);

    /// <summary>
    /// Gets the command that redoes the latest reverted history entry for the active document.
    /// </summary>
    public ICommand RedoCommand => _redoCommand ??= new RelayCommand(
        _ => HistoryContext?.Redo(),
        _ => HistoryContext?.CanRedo == true);

    /// <summary>
    /// Gets the command that deletes the currently selected component from the active node definition.
    /// </summary>
    public ICommand DeleteSelectedComponentCommand => _deleteSelectedComponentCommand ??= new RelayCommand(
        _ => DeleteSelectedComponent(),
        _ => CanDeleteSelectedComponent());

    /// <summary>
    /// Gets the drag/drop payload types that can be dropped onto the current canvas.
    /// </summary>
    public List<Type> DataTypes { get; } =
    [
        typeof(PaletteItemVM),
    ];

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events

    /// <summary>
    /// Raised when the hosted Skia surface should be invalidated and repainted.
    /// </summary>
    public event Action? CanvasInvalidated;

    /// <summary>
    /// Raised when the transient overlay surface should be invalidated and repainted.
    /// </summary>
    public event Action? OverlayInvalidated;

    #endregion Events

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new Skia host view model.
    /// </summary>
    public SkiaHostVM()
    {
        DisplayName = "Skia Host";
        RebuildVisualTree();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Handles a Skia paint request from the host view.
    /// </summary>
    /// <param name="e">The paint event arguments.</param>
    public void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var canvas = e.Surface.Canvas;
        var nodeSpace = new NodeSpace(e.Info.Width, e.Info.Height);
        var viewportChanged = !AreClose(_viewportWidth, e.Info.Width) || !AreClose(_viewportHeight, e.Info.Height);
        _viewportWidth = e.Info.Width;
        _viewportHeight = e.Info.Height;

        if (viewportChanged)
            MarkHitTestLayerDirty(resetBitmap: true);

        if (VisualRoot == null)
            throw new InvalidOperationException("The visual root was not built before the Skia surface requested a paint.");

        ApplyHostViewStateIfNeeded();

        VisualRoot.Bounds = nodeSpace.GetVisibleBounds(Pan, Zoom);
        var viewMatrix = nodeSpace.CreateViewMatrix(Pan, Zoom);

        canvas.Save();
        canvas.SetMatrix(viewMatrix);
        RenderVisualTree(canvas, VisualRoot);
        canvas.Restore();
    }

    /// <summary>
    /// Handles a Skia overlay paint request from the host view.
    /// </summary>
    /// <param name="e">The paint event arguments.</param>
    public void OnOverlayPaintSurface(SKPaintSurfaceEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (Host is not NodeDefinitionVM definitionHost || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        var nodeSpace = new NodeSpace(e.Info.Width, e.Info.Height);
        var viewMatrix = nodeSpace.CreateViewMatrix(Pan, Zoom);
        canvas.Save();
        canvas.SetMatrix(viewMatrix);
        RenderColumnOverlay(canvas, definitionHost, layoutSnapshot);
        if (_previewComponent != null || _previewColumnInsert)
            RenderPreviewOverlay(canvas, definitionHost, layoutSnapshot);
        canvas.Restore();
    }

    /// <summary>
    /// Invalidates the hosted Skia surface so it will repaint.
    /// </summary>
    public void InvalidateCanvas()
    {
        CanvasInvalidated?.Invoke();
        OverlayInvalidated?.Invoke();
    }

    /// <summary>
    /// Rebuilds the current visual tree from the active host context.
    /// </summary>
    public void RebuildVisualTree()
    {
        var canvasStyle = CreateCanvasStyleContext();
        LayoutSnapshot = LayoutEngine.Rebuild(Host, Overview, GetMainGridSize(), GetMinorGridSize(), canvasStyle);

        if (LayoutSnapshot is NodeLayoutSnapshot nodeLayoutSnapshot)
        {
            if (_hoveredColumnIndex is int hoveredColumnIndex
                && !nodeLayoutSnapshot.Columns.Any(column => column.Index == hoveredColumnIndex))
            {
                _hoveredColumnIndex = null;
            }

            if (_selectedColumnIndex is int selectedColumnIndex
                && !nodeLayoutSnapshot.Columns.Any(column => column.Index == selectedColumnIndex))
            {
                _selectedColumnIndex = null;
            }
        }

        MarkHitTestLayerDirty();
        SetVisualRoot(VisualTreeBuilder.Build(
            Host,
            Overview,
            BackgroundColor,
            GetMinorGridSize(),
            canvasStyle,
            LayoutSnapshot,
            ghostedComponents: GetGhostedComponents(),
            showDebugBounds: ShowDebugBounds));

        if (_viewportWidth > 0.0f && _viewportHeight > 0.0f)
            ApplyHostViewStateIfNeeded();
    }

    /// <summary>
    /// Handles pointer input forwarded from the transparent host overlay.
    /// </summary>
    /// <param name="pointerInput">The centered node-space pointer input event.</param>
    public void OnPointerInput(PointerInput pointerInput)
    {
        switch (pointerInput.InputType)
        {
            case PointerInputType.LeftButtonDown:
                HandleLeftButtonDown(pointerInput.Position, pointerInput.LocalPosition);
                break;

            case PointerInputType.LeftButtonUp:
                HandleLeftButtonUp();
                break;

            case PointerInputType.MiddleButtonDown:
                _isPanning = true;
                _lastPanScreenPosition = pointerInput.ScreenPosition;
                break;

            case PointerInputType.MiddleButtonUp:
            case PointerInputType.Leave:
                _isPanning = false;
                if (pointerInput.InputType == PointerInputType.Leave && !_isLeftButtonDown)
                {
                    SetHoveredComponent(null);
                    SetHoveredColumnIndex(null);
                    ClearPreview();
                }
                break;

            case PointerInputType.Move:
                if (HandleColumnDragMove(pointerInput.Position, pointerInput.LocalPosition, pointerInput.ScreenPosition))
                    break;

                if (HandleComponentDragMove(pointerInput.Position, pointerInput.LocalPosition, pointerInput.ScreenPosition))
                    break;

                UpdateHoveredComponent(pointerInput.Position, pointerInput.LocalPosition);

                if (_isPanning)
                {
                    var deltaX = pointerInput.ScreenPosition.X - _lastPanScreenPosition.X;
                    var deltaY = pointerInput.ScreenPosition.Y - _lastPanScreenPosition.Y;
                    _lastPanScreenPosition = pointerInput.ScreenPosition;

                    if (deltaX == 0.0f && deltaY == 0.0f)
                        return;

                    Pan = new NodePoint(Pan.X + (deltaX / Zoom), Pan.Y + (deltaY / Zoom));
                }
                break;

            case PointerInputType.Wheel when pointerInput.WheelDelta != 0:
                var zoomFactor = MathF.Pow(1.1f, pointerInput.WheelDelta / 120.0f);
                Zoom *= zoomFactor;
                break;
        }
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region IDropable Members

    /// <summary>
    /// Handles the start of a compatible drag operation over the canvas.
    /// </summary>
    /// <param name="data">The dragged data payload.</param>
    /// <param name="location">The pointer location relative to the canvas.</param>
    public void OnDragEnter(object data, Point location)
    {
        if (!CanDrop(data) || data is not PaletteItemVM paletteItem)
            return;

        _activeDragFeedbackState = paletteItem;
        if (paletteItem.Kind == PaletteItemKind.Column)
        {
            _previewComponent = null;
            _previewColumnInsert = true;
            UpdateColumnPreview(location);
        }
        else
        {
            _previewColumnInsert = false;
            _previewComponent = paletteItem.CreateComponent();
            UpdatePreview(location, new SKPoint((float)location.X, (float)location.Y));
        }
        SetDragFeedbackEffectOverride(paletteItem, _previewDropValid ? DragDropEffects.Copy : DragDropEffects.None);
    }

    /// <summary>
    /// Handles a compatible drag payload moving over the canvas.
    /// </summary>
    /// <param name="data">The dragged data payload.</param>
    /// <param name="location">The pointer location relative to the canvas.</param>
    public void OnDragOver(object data, Point location)
    {
        if (!CanDrop(data) || data is not PaletteItemVM paletteItem)
            return;

        if (paletteItem.Kind == PaletteItemKind.Column)
        {
            if (!_previewColumnInsert)
                return;

            UpdateColumnPreview(location);
        }
        else
        {
            if (_previewComponent == null)
                return;

            UpdatePreview(location, new SKPoint((float)location.X, (float)location.Y));
        }

        if (data is IDragFeedbackState dragFeedbackState)
            SetDragFeedbackEffectOverride(dragFeedbackState, _previewDropValid ? DragDropEffects.Copy : DragDropEffects.None);
    }

    /// <summary>
    /// Handles the current drag payload leaving the canvas.
    /// </summary>
    public void OnDragLeave()
    {
        if (_previewComponent == null && !_previewColumnInsert)
            return;

        SetDragFeedbackEffectOverride(_activeDragFeedbackState, DragDropEffects.None);

        ClearPreview();
    }

    /// <summary>
    /// Drops the supplied palette item onto the current host at the specified pointer location.
    /// </summary>
    /// <param name="data">The dragged data payload.</param>
    /// <param name="location">The drop location relative to the Skia host.</param>
    /// <param name="index">Unused for canvas drops.</param>
    public void Drop(object data, Point location, int index = -1)
    {
        _ = index;

        if (data is not PaletteItemVM paletteItem || Host is not NodeDefinitionVM definitionHost)
            return;

        if (!_previewDropValid)
        {
            SetDragFeedbackEffectOverride(paletteItem, DragDropEffects.None);
            return;
        }

        if (paletteItem.Kind == PaletteItemKind.Column)
        {
            definitionHost.InsertColumn(_previewDropColumnIndex, notifyDocumentChanged: true);
            SetSelectedColumnIndex(Math.Clamp(_previewDropColumnIndex, 0, definitionHost.ColumnCount - 1));
            SetDragFeedbackEffectOverride(paletteItem, null);
            ClearPreview(false);
            return;
        }

        var component = _previewComponent ?? paletteItem.CreateComponent();
        if (_previewComponent != null)
        {
            component.SetColumn(_previewComponent.Column, notifyComponentChanged: false);
            component.SetParentId(_previewComponent.ParentId, notifyComponentChanged: false);
            component.SetYOrder(_previewComponent.YOrder, notifyComponentChanged: false);
            component.Position = _previewComponent.Position;
        }
        else
        {
            component.Position = new NodePoint(component.Position.X, _previewDropYNu);
        }

        CommitPreviewDrop(definitionHost, component, isNewComponent: true);
        SetSelectedColumnIndex(null);
        SetDragFeedbackEffectOverride(paletteItem, null);
        ClearPreview(false);
    }

    /// <summary>
    /// Returns the required drag/drop effect for the supplied object.
    /// </summary>
    /// <param name="dropObject">The dragged object.</param>
    /// <returns>The required drag/drop effect.</returns>
    public DragDropEffects GetRequiredEffect(object dropObject)
    {
        if (dropObject is PaletteItemVM paletteItem && (_previewComponent != null || paletteItem.Kind == PaletteItemKind.Column))
            return _previewDropValid ? DragDropEffects.Copy : DragDropEffects.None;

        return CanDrop(dropObject)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    /// <summary>
    /// Returns a value indicating whether the supplied object can be dropped onto the current host.
    /// </summary>
    /// <param name="dropObject">The dragged object.</param>
    /// <returns><see langword="true"/> when the object may be dropped; otherwise, <see langword="false"/>.</returns>
    public bool CanDrop(object dropObject)
    {
        return dropObject is PaletteItemVM && Host is NodeDefinitionVM;
    }

    #endregion IDropable Members

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private static SKRect ComputeContentBounds(IVisual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);

        if (visual is CanvasVisual canvasVisual)
        {
            var childBounds = SKRect.Empty;

            foreach (var child in canvasVisual.Children)
                childBounds = UnionBounds(childBounds, ComputeContentBounds(child));

            return childBounds;
        }

        var bounds = visual.Bounds;

        foreach (var child in visual.Children)
            bounds = UnionBounds(bounds, ComputeContentBounds(child));

        return bounds;
    }

    private bool CanFitToView()
    {
        if (_viewportWidth <= 0.0f || _viewportHeight <= 0.0f || VisualRoot == null)
            return false;

        var contentBounds = ComputeContentBounds(VisualRoot);
        return !contentBounds.IsEmpty && contentBounds.Width > 0.0f && contentBounds.Height > 0.0f;
    }

    private void DrawGrid(SKCanvas canvas, SKRect bounds)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        var mainGridSize = GetMainGridSize();
        var minorGridSize = GetMinorGridSize();
        var gridBaseColor = GetGridBaseColor();

        using var minorGridPaint = new SKPaint
        {
            IsAntialias = false,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
        };
        using var mainGridPaint = new SKPaint
        {
            IsAntialias = false,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
        };
        using var majorGridPaint = new SKPaint
        {
            IsAntialias = false,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
        };

        var startX = MathF.Floor(bounds.Left / minorGridSize) * minorGridSize;
        var endX = MathF.Ceiling(bounds.Right / minorGridSize) * minorGridSize;
        var startY = MathF.Floor(bounds.Top / minorGridSize) * minorGridSize;
        var endY = MathF.Ceiling(bounds.Bottom / minorGridSize) * minorGridSize;

        for (var x = startX; x <= endX; x += minorGridSize)
        {
            var lineIndex = (int)MathF.Round(x / minorGridSize);
            var gridPaint = GetGridPaintForLine(lineIndex, minorGridPaint, mainGridPaint, majorGridPaint, gridBaseColor);

            if (gridPaint == null)
                continue;

            canvas.DrawLine(x, bounds.Top, x, bounds.Bottom, gridPaint);
        }

        for (var y = startY; y <= endY; y += minorGridSize)
        {
            var lineIndex = (int)MathF.Round(y / minorGridSize);
            var gridPaint = GetGridPaintForLine(lineIndex, minorGridPaint, mainGridPaint, majorGridPaint, gridBaseColor);

            if (gridPaint == null)
                continue;

            canvas.DrawLine(bounds.Left, y, bounds.Right, y, gridPaint);
        }
    }

    private SKPaint? GetGridPaintForLine(int lineIndex, SKPaint minorGridPaint, SKPaint mainGridPaint, SKPaint majorGridPaint, SKColor gridBaseColor)
    {
        ArgumentNullException.ThrowIfNull(minorGridPaint);
        ArgumentNullException.ThrowIfNull(mainGridPaint);
        ArgumentNullException.ThrowIfNull(majorGridPaint);

        if (lineIndex % 16 == 0)
        {
            var alpha = GetGridAlpha(MajorGridBaseOpacity, -0.04f, 0.18f, 0.50f);

            if (alpha <= GridAlphaCutoff)
                return null;

            majorGridPaint.Color = gridBaseColor.WithAlpha(alpha);
            return majorGridPaint;
        }

        if (lineIndex % 4 == 0)
        {
            var alpha = GetGridAlpha(MainGridBaseOpacity, 0.18f, 0.0f, 0.40f);

            if (alpha <= GridAlphaCutoff)
                return null;

            mainGridPaint.Color = gridBaseColor.WithAlpha(alpha);
            return mainGridPaint;
        }

        var minorAlpha = GetGridAlpha(MinorGridBaseOpacity, 0.18f, 0.0f, 0.25f);

        if (minorAlpha <= GridAlphaCutoff)
            return null;

        minorGridPaint.Color = gridBaseColor.WithAlpha(minorAlpha);
        return minorGridPaint;
    }

    private float GetMainGridSize()
    {
        using var typeface = string.IsNullOrWhiteSpace(FontFamily)
            ? null
            : SKTypeface.FromFamilyName(FontFamily);

        using var textPaint = new SKPaint
        {
            Typeface = typeface,
            TextSize = FontSize,
            IsAntialias = true,
        };

        var textBounds = SKRect.Empty;
        textPaint.MeasureText("Mj", ref textBounds);
        var textHeight = MathF.Abs(textBounds.Height);

        if (textHeight <= 0.0f)
            textHeight = FontSize;

        return MathF.Ceiling(textHeight + (GridPadding * 2.0f));
    }

    private float GetMinorGridSize()
    {
        return GetMainGridSize() / 4.0f;
    }

    private byte GetGridAlpha(float baseOpacity, float zoomSlope, float minimumOpacity, float maximumOpacity)
    {
        var zoomLevel = MathF.Log2(MathF.Max(Zoom, 0.01f));
        var opacity = baseOpacity + (zoomLevel * zoomSlope);
        opacity = Math.Clamp(opacity, minimumOpacity, maximumOpacity);
        return (byte)Math.Clamp(MathF.Round(opacity * 255.0f), 0.0f, 255.0f);
    }

    private SKColor GetGridBaseColor()
    {
        var grayscale = ((BackgroundColor.Red * 0.299f) + (BackgroundColor.Green * 0.587f) + (BackgroundColor.Blue * 0.114f));

        if (grayscale <= 64.0f || grayscale >= 191.0f)
            return new SKColor(128, 128, 128);

        if (grayscale <= 127.0f)
            return new SKColor(192, 192, 192);

        return new SKColor(72, 72, 72);
    }

    private void FitToView()
    {
        if (!CanFitToView() || VisualRoot == null)
            return;

        var contentBounds = ComputeContentBounds(VisualRoot);
        var widthZoom = (_viewportWidth * FitPaddingFactor) / contentBounds.Width;
        var heightZoom = (_viewportHeight * FitPaddingFactor) / contentBounds.Height;
        var fitZoom = MathF.Max(0.01f, MathF.Min(widthZoom, heightZoom));
        var centerX = contentBounds.MidX;
        var centerY = contentBounds.MidY;

        SetViewportState(new NodePoint(-centerX, -centerY), fitZoom, persistToHost: true);
    }

    private bool CanDeleteSelectedComponent()
    {
        if (Host is not NodeDefinitionVM definitionHost)
            return false;

        if (definitionHost.SelectedComponentCount > 0)
            return true;

        var selectedColumnIndex = definitionHost.SelectedColumnIndex;
        return selectedColumnIndex.HasValue
            && (definitionHost.CanDeleteColumn(selectedColumnIndex.Value, NodeColumnDeleteRule.Delete)
                || definitionHost.CanDeleteColumn(selectedColumnIndex.Value, NodeColumnDeleteRule.MoveLeft)
                || definitionHost.CanDeleteColumn(selectedColumnIndex.Value, NodeColumnDeleteRule.MoveRight));
    }

    private void DeleteSelectedComponent()
    {
        if (Host is not NodeDefinitionVM definitionHost)
            return;

        if (definitionHost.SelectedComponentCount > 0)
        {
            var selectedComponents = definitionHost.SelectedComponents.ToHashSet();

            if (!definitionHost.RemoveSelectedComponents())
                return;

            if (_hoveredComponent != null && selectedComponents.Contains(_hoveredComponent))
                SetHoveredComponent(null);

            RebuildVisualTree();
            return;
        }

        var selectedColumnIndex = definitionHost.SelectedColumnIndex;
        if (!selectedColumnIndex.HasValue)
            return;

        if (!TryDeleteSelectedColumn(definitionHost, selectedColumnIndex.Value))
            return;

        SyncSelectedColumnOverlay(definitionHost);
        RebuildVisualTree();
    }

    private bool TryDeleteSelectedColumn(NodeDefinitionVM definitionHost, int columnIndex)
    {
        if (!definitionHost.ColumnHasContent(columnIndex))
            return definitionHost.DeleteColumn(columnIndex, NodeColumnDeleteRule.Delete);

        var rule = ShowDeleteColumnDialog(definitionHost, columnIndex);
        return definitionHost.DeleteColumn(columnIndex, rule);
    }

    private NodeColumnDeleteRule ShowDeleteColumnDialog(NodeDefinitionVM definitionHost, int columnIndex)
    {
        var dialogVm = new DeleteColumnDialogVM(
            definitionHost.CanDeleteColumn(columnIndex, NodeColumnDeleteRule.Delete),
            definitionHost.CanDeleteColumn(columnIndex, NodeColumnDeleteRule.MoveLeft),
            definitionHost.CanDeleteColumn(columnIndex, NodeColumnDeleteRule.MoveRight));
        var dialogView = new DeleteColumnDialogView
        {
            DataContext = dialogVm,
        };
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        var dialogWindow = new Window
        {
            Title = "Column Has Content",
            Content = dialogView,
            Owner = owner,
            WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.ToolWindow,
        };

        dialogVm.CloseRequested += (_, _) => dialogWindow.Close();
        dialogWindow.ShowDialog();
        return dialogVm.Result;
    }

    private void SyncSelectedColumnOverlay(NodeDefinitionVM definitionHost)
    {
        SetSelectedColumnIndex(definitionHost.SelectedColumnIndex);
    }

    private void ApplyHostViewStateIfNeeded()
    {
        if (!_pendingHostViewState)
            return;

        _pendingHostViewState = false;

        if (Host is not ICanvasViewStateHost viewStateHost)
            return;

        if (viewStateHost.CanvasZoom > 0.0f)
        {
            SetViewportState(viewStateHost.CanvasPan, viewStateHost.CanvasZoom, persistToHost: false);
            return;
        }

        if (!CanFitToView())
        {
            _pendingHostViewState = true;
            return;
        }

        FitToView();
    }

    private void SaveHostViewState()
    {
        if (Host is not ICanvasViewStateHost viewStateHost)
            return;

        viewStateHost.CanvasPan = _pan;
        viewStateHost.CanvasZoom = _zoom;
    }

    private void SetViewportState(NodePoint pan, float zoom, bool persistToHost)
    {
        var normalizedZoom = zoom <= 0.0f ? 0.01f : zoom;
        var panChanged = _pan != pan;
        var zoomChanged = !AreClose(_zoom, normalizedZoom);

        _pan = pan;
        _zoom = normalizedZoom;

        if (persistToHost)
            SaveHostViewState();

        if (!panChanged && !zoomChanged)
            return;

        OnPropertyChanged(nameof(Pan));
        OnPropertyChanged(nameof(Zoom));
        OnPropertyChanged(nameof(ZoomText));
        MarkHitTestLayerDirty();
        InvalidateCanvas();
    }

    private void HandleLeftButtonDown(SKPoint position, SKPoint localPosition)
    {
        if (Host is not NodeDefinitionVM definitionHost || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        _isLeftButtonDown = true;
        _leftButtonDownPosition = position;

        if (TryGetColumnHandleIndex(layoutSnapshot, position, out var columnIndex))
        {
            _pressedComponent = null;
            _pressedColumnIndex = columnIndex;
            SetHoveredComponent(null);
            SetSelectedColumnIndex(columnIndex);
            var columnSelectionChanged = definitionHost.ClearComponentSelection();
            columnSelectionChanged |= definitionHost.SelectColumn(columnIndex);
            if (columnSelectionChanged)
                RebuildVisualTree();
            return;
        }

        _pressedColumnIndex = null;
        _pressedComponent = HitTestComponent(layoutSnapshot, position, localPosition);
        SetSelectedColumnIndex(null);

        var isToggleSelection = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        var selectionChanged = false;
        if (_pressedComponent != null)
        {
            selectionChanged |= isToggleSelection
                ? definitionHost.ToggleComponentSelection(_pressedComponent)
                : definitionHost.SelectComponent(_pressedComponent);
        }
        else
        {
            selectionChanged |= definitionHost.ClearComponentSelection();
            selectionChanged |= definitionHost.ClearColumnSelection();
        }

        if (isToggleSelection)
            _pressedComponent = null;

        if (selectionChanged)
            RebuildVisualTree();
    }

    private void HandleLeftButtonUp()
    {
        _isLeftButtonDown = false;

        if (_isDraggingSelectedColumn && _dragSourceColumnIndex.HasValue)
        {
            if (_previewDropValid && Host is NodeDefinitionVM definitionHost)
            {
                definitionHost.MoveColumn(_dragSourceColumnIndex.Value, _previewDropColumnIndex, notifyDocumentChanged: true);
                SyncSelectedColumnOverlay(definitionHost);
            }

            ClearSelectedColumnDrag();
            return;
        }

        if (_isDraggingSelectedComponent && _dragSourceComponent != null)
        {
            if (_previewDropValid)
            {
                var previewComponent = _previewComponent;
                if (previewComponent != null)
                {
                    var resolvedX = IsButtonLikeComponent(previewComponent)
                        ? previewComponent.Position.X
                        : _dragSourceComponent.Position.X;
                    _dragSourceComponent.Position = new NodePoint(resolvedX, _previewDropYNu);
                }

                if (Host is NodeDefinitionVM definitionHost)
                    CommitPreviewDrop(definitionHost, _dragSourceComponent, isNewComponent: false);
            }

            ClearSelectedComponentDrag();
            return;
        }

        _pressedColumnIndex = null;
        _pressedComponent = null;
    }

    private bool HandleColumnDragMove(SKPoint position, SKPoint localPosition, SKPoint screenPosition)
    {
        if (!_isLeftButtonDown || Host is not NodeDefinitionVM definitionHost || LayoutSnapshot is not NodeLayoutSnapshot)
            return false;

        if (!_isDraggingSelectedColumn)
        {
            if (!_pressedColumnIndex.HasValue)
                return false;

            var deltaX = position.X - _leftButtonDownPosition.X;
            var deltaY = position.Y - _leftButtonDownPosition.Y;
            if (AreClose(deltaX, 0.0f) && AreClose(deltaY, 0.0f))
                return true;

            _isDraggingSelectedColumn = true;
            _dragSourceColumnIndex = _pressedColumnIndex;
            _previewComponent = null;
            _previewColumnInsert = true;
            SetHoveredComponent(null);
            SetHoveredColumnIndex(null);
            RebuildVisualTree();
            BeginColumnDragImage(definitionHost, screenPosition);
        }

        UpdateColumnMovePreview(localPosition);
        UpdateEditorDragFeedback(screenPosition);
        return true;
    }

    private bool HandleComponentDragMove(SKPoint position, SKPoint localPosition, SKPoint screenPosition)
    {
        if (!_isLeftButtonDown || Host is not NodeDefinitionVM definitionHost || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return false;

        if (definitionHost.SelectedComponentCount != 1)
            return false;

        if (!_isDraggingSelectedComponent)
        {
            if (_pressedComponent == null)
                return false;

            var deltaX = position.X - _leftButtonDownPosition.X;
            var deltaY = position.Y - _leftButtonDownPosition.Y;
            if (AreClose(deltaX, 0.0f) && AreClose(deltaY, 0.0f))
                return true;

            _isDraggingSelectedComponent = true;
            _dragSourceComponent = _pressedComponent;
            SetHoveredComponent(null);
            _previewComponent = CreatePreviewComponent(_pressedComponent);
            RebuildVisualTree();
            BeginEditorDragImage(definitionHost, screenPosition);
        }

        UpdatePreview(position, localPosition, layoutSnapshot);
        UpdateEditorDragFeedback(screenPosition);
        return true;
    }

    private void RenderPreviewOverlay(SKCanvas canvas, NodeDefinitionVM definitionHost, NodeLayoutSnapshot layoutSnapshot)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(definitionHost);

        var previewBaseColor = GetPreviewBaseColor(definitionHost, _previewDropValid);

        if (!_previewDropValid)
            return;

        if (_previewInsertionY.HasValue)
        {
            DrawInsertionIndicator(canvas, previewBaseColor, _previewInsertionY.Value);
        }
        else if (_previewInsertionX.HasValue && !_previewInlineBounds.IsEmpty)
        {
            DrawVerticalInsertionIndicator(canvas, previewBaseColor, _previewInsertionX.Value, _previewInlineBounds);
        }
        else if (!_previewPlacementBounds.IsEmpty)
        {
            using var placementPaint = new SKPaint
            {
                Color = previewBaseColor.WithAlpha((byte)Math.Clamp(MathF.Round(PreviewIndicatorOpacity * 255.0f), 0.0f, 255.0f)),
                IsAntialias = false,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.0f / MathF.Max(Zoom, 0.01f),
            };

            canvas.DrawRect(_previewPlacementBounds, placementPaint);
        }

        if (_previewComponent is NodePortComponentVM previewPort
            && TryGetPreviewPortGhostBounds(definitionHost, out var portGhostBounds))
        {
            var fontWeight = definitionHost.Model.Style?.ShapeStyle.FontWeight ?? NodeFontWeight.Normal;
            var nodeStyle = definitionHost.Model.Style?.ShapeStyle ?? new NodeStyle().ShapeStyle;
            var portVisual = new NodePortComponentVisual(
                previewPort.Label,
                previewPort.Side,
                previewPort.Shape,
                previewPort.Shape == NodePortShape.Hidden ? NodePortPlacement.Over : PortPlacement,
                PortSize,
                FontFamily,
                FontSize,
                FontColor,
                fontWeight,
                previewPort.FillColor,
                nodeStyle.BorderColor,
                nodeStyle.BorderThickness,
                GetMinorGridSize(),
                GetMinorGridSize() / 4.0f,
                layoutSnapshot.NodeBounds.Left,
                layoutSnapshot.NodeBounds.Right,
                0.28f)
            {
                Bounds = portGhostBounds,
            };

            portVisual.Render(canvas);
        }
    }

    private void RenderVisualTree(SKCanvas canvas, IVisual visualRoot)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(visualRoot);

        if (visualRoot is not CanvasVisual canvasVisual)
        {
            visualRoot.Render(canvas);
            return;
        }

        canvas.Clear(canvasVisual.BackgroundColor.ToSkColor());

        if (ShowGrid)
            DrawGrid(canvas, canvasVisual.Bounds);

        foreach (var child in canvasVisual.Children)
            child.Render(canvas);
    }

    private void SetVisualRoot(IVisual? visualRoot)
    {
        if (!SetProperty(ref _visualRoot, visualRoot, nameof(VisualRoot)))
            return;

        if (_visualRoot is CanvasVisual canvasVisual)
            canvasVisual.BackgroundColor = BackgroundColor;

        CommandManager.InvalidateRequerySuggested();
        InvalidateCanvas();
    }

    private static SKRect UnionBounds(SKRect current, SKRect next)
    {
        if (current.IsEmpty)
            return next;

        if (next.IsEmpty)
            return current;

        return new SKRect(
            MathF.Min(current.Left, next.Left),
            MathF.Min(current.Top, next.Top),
            MathF.Max(current.Right, next.Right),
            MathF.Max(current.Bottom, next.Bottom));
    }

    private NodePoint ToNodeUnitPoint(Point location)
    {
        var nodeSpace = new NodeSpace(_viewportWidth, _viewportHeight);
        var point = nodeSpace.ToNodePoint((float)location.X, (float)location.Y, Pan, Zoom);
        var minorGridSize = GetMinorGridSize();

        return new NodePoint(
            MathF.Round(point.X / minorGridSize),
            MathF.Round(point.Y / minorGridSize));
    }

    private NodePoint ToNodeUnitPoint(Point location, bool snap)
    {
        var nodeSpace = new NodeSpace(_viewportWidth, _viewportHeight);
        var point = nodeSpace.ToNodePoint((float)location.X, (float)location.Y, Pan, Zoom);
        var minorGridSize = GetMinorGridSize();
        var x = point.X / minorGridSize;
        var y = point.Y / minorGridSize;

        return snap
            ? new NodePoint(MathF.Round(x), MathF.Round(y))
            : new NodePoint(x, y);
    }

    private void UpdatePreview(Point location, SKPoint screenPoint)
    {
        if (_previewComponent == null || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        var nodeSpace = new NodeSpace(_viewportWidth, _viewportHeight);
        _previewPointWorld = nodeSpace.ToNodePoint((float)location.X, (float)location.Y, Pan, Zoom);
        UpdatePreview(_previewPointWorld, screenPoint, layoutSnapshot);
    }

    private void UpdatePreview(SKPoint nodePoint, SKPoint localPoint, NodeLayoutSnapshot layoutSnapshot)
    {
        if (_previewComponent == null)
            return;

        _previewPointWorld = nodePoint;

        if (!layoutSnapshot.NodeBounds.Contains(_previewPointWorld.X, _previewPointWorld.Y))
        {
            _previewDropValid = false;
            _previewInsertionX = null;
            _previewInlineBounds = SKRect.Empty;
            _previewPlacementBounds = SKRect.Empty;
            _previewInsertionY = null;
            OverlayInvalidated?.Invoke();
            return;
        }

        _previewDropValid = true;
        var minorGridSize = GetMinorGridSize();
        _previewComponent.Position = new NodePoint(_previewPointWorld.X / minorGridSize, _previewPointWorld.Y / minorGridSize);
        var hitResult = GetPreviewHitResult(layoutSnapshot, nodePoint, localPoint);
        ApplyPreviewComponentPlacement(_previewComponent, layoutSnapshot, hitResult);
        UpdateProjectedPlacement(layoutSnapshot, hitResult);
        OverlayInvalidated?.Invoke();
    }

    private void UpdateColumnPreview(Point location)
    {
        UpdateColumnPreviewCore(new SKPoint((float)location.X, (float)location.Y), null);
    }

    private void UpdateColumnMovePreview(SKPoint localPosition)
    {
        UpdateColumnPreviewCore(localPosition, _dragSourceColumnIndex);
    }

    private void UpdateColumnPreviewCore(SKPoint localPosition, int? draggedColumnIndex)
    {
        if (!_previewColumnInsert || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        var nodeSpace = new NodeSpace(_viewportWidth, _viewportHeight);
        _previewPointWorld = nodeSpace.ToNodePoint(localPosition.X, localPosition.Y, Pan, Zoom);

        if (!layoutSnapshot.NodeBounds.Contains(_previewPointWorld.X, _previewPointWorld.Y))
        {
            _previewDropValid = false;
            _previewInsertionX = null;
            _previewInlineBounds = SKRect.Empty;
            _previewPlacementBounds = SKRect.Empty;
            _previewInsertionY = null;
            OverlayInvalidated?.Invoke();
            return;
        }

        var orderedColumns = layoutSnapshot.Columns
            .OrderBy(column => column.Index)
            .ToList();
        if (orderedColumns.Count == 0)
        {
            _previewDropValid = false;
            OverlayInvalidated?.Invoke();
            return;
        }

        var insertionSlots = new List<(int Index, float X)>
        {
            (0, orderedColumns[0].GuideBounds.Left)
        };

        for (var index = 1; index < orderedColumns.Count; index++)
        {
            insertionSlots.Add((index, (orderedColumns[index - 1].GuideBounds.Right + orderedColumns[index].GuideBounds.Left) * 0.5f));
        }

        insertionSlots.Add((orderedColumns.Count, orderedColumns[^1].GuideBounds.Right));

        var insertionSlot = insertionSlots
            .OrderBy(slot => MathF.Abs(slot.X - _previewPointWorld.X))
            .First();

        var resolvedDropColumnIndex = draggedColumnIndex.HasValue
            ? ResolveColumnMoveTargetIndex(draggedColumnIndex.Value, insertionSlot.Index, orderedColumns.Count)
            : insertionSlot.Index;

        var isNoOpMove = draggedColumnIndex.HasValue && resolvedDropColumnIndex == draggedColumnIndex.Value;
        _previewDropValid = !isNoOpMove;
        _previewInsertionY = null;
        _previewPlacementBounds = SKRect.Empty;
        _previewInsertionX = _previewDropValid ? insertionSlot.X : null;
        _previewInlineBounds = _previewDropValid
            ? new SKRect(
                layoutSnapshot.ContentBounds.Left,
                orderedColumns[0].GuideBounds.Top,
                layoutSnapshot.ContentBounds.Right,
                orderedColumns[0].GuideBounds.Bottom)
            : SKRect.Empty;
        _previewDropColumnIndex = resolvedDropColumnIndex;
        _previewDropRowIndex = 0;
        _previewDropXOrder = 0;
        _previewInlineWithRow = false;
        OverlayInvalidated?.Invoke();
    }

    private void UpdateProjectedPlacement(NodeLayoutSnapshot layoutSnapshot, HitTestResult hitResult)
    {
        if (Host is not NodeDefinitionVM definitionHost)
            return;

        var lineHeight = layoutSnapshot.LineHeight;
        var dragProjection = BuildDragProjection(definitionHost);
        var previewColumn = GetPreviewColumnLayout(layoutSnapshot);
        var projectedColumn = dragProjection.GetColumn(previewColumn.Index);
        _previewDropColumnIndex = previewColumn.Index;
        _previewDropXOrder = 0;
        _previewInlineWithRow = false;
        var rows = BuildPreviewRows(projectedColumn, layoutSnapshot);
        var minorGridSize = GetMinorGridSize();

        if (rows.Count == 0)
        {
            _previewInsertionY = null;
            _previewInsertionX = null;
            _previewInlineBounds = SKRect.Empty;
            _previewPlacementBounds = new SKRect(
                previewColumn.ContentBounds.Left,
                previewColumn.ContentBounds.Top,
                previewColumn.ContentBounds.Right,
                previewColumn.ContentBounds.Top + lineHeight);
            _previewDropYNu = previewColumn.ContentBounds.Top / minorGridSize;
            _previewDropRowIndex = 0;
            _previewDropXOrder = 0;
            return;
        }

        if (hitResult.Component != null)
        {
            var hitRow = rows.FirstOrDefault(row => row.Components.Any(component => ReferenceEquals(component.Component, hitResult.Component)));
            var hitLayout = hitRow?.Components.FirstOrDefault(component => ReferenceEquals(component.Component, hitResult.Component));
            if (hitRow != null && hitLayout != null)
            {
                if (IsButtonLikeComponent(_previewComponent) && IsButtonLikeComponent(hitLayout.Component))
                {
                    var inlineBefore = hitResult.Flags.HasFlag(HitRegionFlags.Middle)
                        && hitResult.Flags.HasFlag(HitRegionFlags.Left)
                        && !hitResult.Flags.HasFlag(HitRegionFlags.Right);
                    var inlineAfter = hitResult.Flags.HasFlag(HitRegionFlags.Middle)
                        && hitResult.Flags.HasFlag(HitRegionFlags.Right)
                        && !hitResult.Flags.HasFlag(HitRegionFlags.Left);

                    if (inlineBefore || inlineAfter)
                    {
                        var previewComponent = _previewComponent;
                        _previewInsertionY = null;
                        _previewInsertionX = TryGetButtonInsertionIndicatorX(layoutSnapshot, hitRow, hitLayout, inlineBefore, out var insertionX)
                            ? insertionX
                            : inlineBefore
                                ? hitLayout.Bounds.Left
                                : hitLayout.Bounds.Right;
                        _previewInlineBounds = hitLayout.Bounds;
                        _previewPlacementBounds = SKRect.Empty;
                        _previewDropYNu = hitLayout.PositionNu.Y;
                        _previewDropRowIndex = hitRow.RowIndex;
                        _previewDropXOrder = GetInlineDropXOrder(hitRow, hitLayout, inlineBefore);
                        _previewInlineWithRow = true;
                        previewComponent!.Position = new NodePoint(hitLayout.PositionNu.X + (inlineBefore ? -0.5f : 0.5f), hitLayout.PositionNu.Y);
                        return;
                    }
                }

                var insertAbove = hitResult.Flags.HasFlag(HitRegionFlags.Top) && !hitResult.Flags.HasFlag(HitRegionFlags.Bottom);
                var insertBelow = hitResult.Flags.HasFlag(HitRegionFlags.Bottom) && !hitResult.Flags.HasFlag(HitRegionFlags.Top);

                if ((insertAbove || insertBelow) && hitRow != null)
                {
                    _previewInsertionX = null;
                    _previewInlineBounds = SKRect.Empty;
                    _previewInsertionY = GetVerticalInsertionIndicatorY(hitRow.PrimaryComponent, lineHeight, minorGridSize, insertAbove);
                    _previewPlacementBounds = _dragSourceComponent == null && insertBelow && hitRow.RowIndex == rows.Count - 1
                        ? new SKRect(
                            previewColumn.ContentBounds.Left,
                            hitRow.Bounds.Bottom,
                            previewColumn.ContentBounds.Right,
                            hitRow.Bounds.Bottom + lineHeight)
                        : SKRect.Empty;
                    _previewDropYNu = hitRow.PrimaryComponent.PositionNu.Y + (insertAbove ? -0.5f : 0.5f);
                    _previewDropRowIndex = hitRow.RowIndex + (insertAbove ? 0 : 1);
                    _previewDropXOrder = 0;
                    SuppressNoOpInsertion(layoutSnapshot);
                    return;
                }
            }
        }

        foreach (var row in rows)
        {
            if (_previewPointWorld.Y > row.Bounds.Bottom)
                continue;

            if (_previewPointWorld.Y <= row.Bounds.MidY)
            {
                _previewInsertionX = null;
                _previewInlineBounds = SKRect.Empty;
                _previewInsertionY = GetVerticalInsertionIndicatorY(row.PrimaryComponent, lineHeight, minorGridSize, insertAbove: true);
                _previewPlacementBounds = SKRect.Empty;
                _previewDropYNu = row.PrimaryComponent.PositionNu.Y - 0.5f;
                _previewDropRowIndex = row.RowIndex;
                _previewDropXOrder = 0;
                SuppressNoOpInsertion(layoutSnapshot);
                return;
            }

            if (row.RowIndex == rows.Count - 1)
            {
                _previewInsertionX = null;
                _previewInlineBounds = SKRect.Empty;
                _previewInsertionY = GetVerticalInsertionIndicatorY(row.PrimaryComponent, lineHeight, minorGridSize, insertAbove: false);
                _previewPlacementBounds = _dragSourceComponent == null
                    ? new SKRect(
                        previewColumn.ContentBounds.Left,
                        row.Bounds.Bottom,
                        previewColumn.ContentBounds.Right,
                        row.Bounds.Bottom + lineHeight)
                    : SKRect.Empty;
                _previewDropYNu = row.PrimaryComponent.PositionNu.Y + 0.5f;
                _previewDropRowIndex = row.RowIndex + 1;
                _previewDropXOrder = 0;
                SuppressNoOpInsertion(layoutSnapshot);
                return;
            }

            _previewInsertionY = GetVerticalInsertionIndicatorY(row.PrimaryComponent, lineHeight, minorGridSize, insertAbove: false);
            _previewInsertionX = null;
            _previewInlineBounds = SKRect.Empty;
            _previewPlacementBounds = SKRect.Empty;
            _previewDropYNu = row.PrimaryComponent.PositionNu.Y + 0.5f;
            _previewDropRowIndex = row.RowIndex + 1;
            _previewDropXOrder = 0;
            SuppressNoOpInsertion(layoutSnapshot);
            return;
        }

        var last = rows[^1];
        _previewInsertionX = null;
        _previewInlineBounds = SKRect.Empty;
        _previewInsertionY = GetVerticalInsertionIndicatorY(last.PrimaryComponent, lineHeight, minorGridSize, insertAbove: false);
        _previewPlacementBounds = _dragSourceComponent == null
            ? new SKRect(
                previewColumn.ContentBounds.Left,
                last.Bounds.Bottom,
                previewColumn.ContentBounds.Right,
                last.Bounds.Bottom + lineHeight)
            : SKRect.Empty;
        _previewDropYNu = last.PrimaryComponent.PositionNu.Y + 0.5f;
        _previewDropRowIndex = last.RowIndex + 1;
        _previewDropXOrder = 0;
        SuppressNoOpInsertion(layoutSnapshot);
    }

    private static float GetVerticalInsertionIndicatorY(
        NodeComponentLayoutSnapshot componentLayout,
        float lineHeight,
        float minorGridSize,
        bool insertAbove)
    {
        if (!IsButtonLikeComponent(componentLayout.Component))
            return insertAbove ? componentLayout.Bounds.Top : componentLayout.Bounds.Bottom;

        var rowTop = componentLayout.PositionNu.Y * minorGridSize;
        return insertAbove ? rowTop : rowTop + lineHeight;
    }

    private void DrawInsertionIndicator(SKCanvas canvas, SKColor indicatorColor, float insertionY)
    {
        if (LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        var previewColumn = GetPreviewColumnLayout(layoutSnapshot);
        var left = previewColumn.ContentBounds.Left;
        var right = previewColumn.ContentBounds.Right;
        var arrowSize = MathF.Max(3.0f, GetMinorGridSize() * 0.5f);
        var strokeWidth = 0.0f;
        var previewPort = _previewComponent as NodePortComponentVM;
        var isLeftPort = previewPort?.Side == NodePortSide.Left;
        var isRightPort = previewPort?.Side == NodePortSide.Right;
        var portRadius = previewPort != null
            ? MathF.Max(1.0f, (PortSize * GetMinorGridSize()) * 0.25f)
            : 0.0f;
        var alphaColor = indicatorColor.WithAlpha((byte)Math.Clamp(MathF.Round(PreviewIndicatorOpacity * 255.0f), 0.0f, 255.0f));

        using var linePaint = new SKPaint
        {
            Color = alphaColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
        };
        using var arrowPaint = new SKPaint
        {
            Color = alphaColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        using var portPaint = new SKPaint
        {
            Color = alphaColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        canvas.DrawLine(left, insertionY, right, insertionY, linePaint);

        if (isLeftPort)
            canvas.DrawCircle(left, insertionY, portRadius, portPaint);
        else
        {
            using var leftArrow = new SKPath();
            leftArrow.MoveTo(left, insertionY - (arrowSize * 0.5f));
            leftArrow.LineTo(left, insertionY + (arrowSize * 0.5f));
            leftArrow.LineTo(left + arrowSize, insertionY);
            leftArrow.Close();
            canvas.DrawPath(leftArrow, arrowPaint);
        }

        if (isRightPort)
            canvas.DrawCircle(right, insertionY, portRadius, portPaint);
        else
        {
            using var rightArrow = new SKPath();
            rightArrow.MoveTo(right, insertionY - (arrowSize * 0.5f));
            rightArrow.LineTo(right, insertionY + (arrowSize * 0.5f));
            rightArrow.LineTo(right - arrowSize, insertionY);
            rightArrow.Close();
            canvas.DrawPath(rightArrow, arrowPaint);
        }
    }

    private void DrawVerticalInsertionIndicator(SKCanvas canvas, SKColor indicatorColor, float insertionX, SKRect rowBounds)
    {
        var arrowSize = MathF.Max(3.0f, GetMinorGridSize() * 0.5f);
        var strokeWidth = 2.0f / MathF.Max(Zoom, 0.01f);
        var alphaColor = indicatorColor.WithAlpha((byte)Math.Clamp(MathF.Round(PreviewIndicatorOpacity * 255.0f), 0.0f, 255.0f));

        using var linePaint = new SKPaint
        {
            Color = alphaColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeWidth,
        };
        using var arrowPaint = new SKPaint
        {
            Color = alphaColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        canvas.DrawLine(insertionX, rowBounds.Top, insertionX, rowBounds.Bottom, linePaint);

        using var topArrow = new SKPath();
        topArrow.MoveTo(insertionX - (arrowSize * 0.5f), rowBounds.Top);
        topArrow.LineTo(insertionX + (arrowSize * 0.5f), rowBounds.Top);
        topArrow.LineTo(insertionX, rowBounds.Top + arrowSize);
        topArrow.Close();
        canvas.DrawPath(topArrow, arrowPaint);

        using var bottomArrow = new SKPath();
        bottomArrow.MoveTo(insertionX - (arrowSize * 0.5f), rowBounds.Bottom);
        bottomArrow.LineTo(insertionX + (arrowSize * 0.5f), rowBounds.Bottom);
        bottomArrow.LineTo(insertionX, rowBounds.Bottom - arrowSize);
        bottomArrow.Close();
        canvas.DrawPath(bottomArrow, arrowPaint);
    }

    private SKColor GetPreviewBaseColor(NodeDefinitionVM definitionHost, bool isValidDrop)
    {
        if (!isValidDrop)
            return SKColors.Red;

        var backgroundColor = definitionHost.Model.Style?.ShapeStyle.BackgroundColor ?? BackgroundColor;
        var grayscale = ((backgroundColor.Red * 0.299f) + (backgroundColor.Green * 0.587f) + (backgroundColor.Blue * 0.114f));

        if (grayscale <= 64.0f || grayscale >= 191.0f)
            return new SKColor(128, 128, 128);

        if (grayscale <= 127.0f)
            return new SKColor(192, 192, 192);

        return new SKColor(72, 72, 72);
    }

    private void SetDragFeedbackEffectOverride(IDragFeedbackState? dragFeedbackState, DragDropEffects? effect)
    {
        if (dragFeedbackState == null)
            return;

        dragFeedbackState.DragFeedbackEffectOverride = effect;

        if (effect == null || effect == DragDropEffects.None)
            _activeDragFeedbackState = null;
        else
            _activeDragFeedbackState = dragFeedbackState;
    }

    private void ClearPreview(bool invalidateOverlay = true)
    {
        _previewComponent = null;
        _previewColumnInsert = false;
        _previewDropValid = false;
        _previewInsertionY = null;
        _previewInsertionX = null;
        _previewInlineBounds = SKRect.Empty;
        _previewPlacementBounds = SKRect.Empty;
        _previewDropYNu = 0.0f;
        _previewDropRowIndex = 0;
        _previewDropColumnIndex = 0;
        _previewDropXOrder = 0;
        _previewInlineWithRow = false;
        _dragSourceColumnIndex = null;
        _dragSourceComponent = null;
        _activeDragFeedbackState = null;

        if (invalidateOverlay)
            OverlayInvalidated?.Invoke();
    }

    private void ClearSelectedComponentDrag()
    {
        _pressedComponent = null;
        _isDraggingSelectedComponent = false;
        EndEditorDragImage();
        ClearPreview();
        RebuildVisualTree();
    }

    private void ClearSelectedColumnDrag()
    {
        _pressedColumnIndex = null;
        _isDraggingSelectedColumn = false;
        EndEditorDragImage();
        ClearPreview();
        RebuildVisualTree();
    }

    private void BeginEditorDragImage(NodeDefinitionVM definitionHost, SKPoint screenPosition)
    {
        if (_previewComponent == null)
            return;

        definitionHost.DragImage = _dragImageRenderer.RenderComponent(
            _previewComponent,
            CreateCanvasStyleContext(),
            definitionHost.Model.Style ?? new NodeStyle());

        SharedDragService.Begin(
            definitionHost.DragImage,
            DragDropEffects.None,
            screenPosition.X,
            screenPosition.Y);
    }

    private void BeginColumnDragImage(NodeDefinitionVM definitionHost, SKPoint screenPosition)
    {
        var previewColumnLabel = new NodeLabelComponentVM(new NodeText { Label = "Column" });
        definitionHost.DragImage = _dragImageRenderer.RenderComponent(
            previewColumnLabel,
            CreateCanvasStyleContext(),
            definitionHost.Model.Style ?? new NodeStyle());

        SharedDragService.Begin(
            definitionHost.DragImage,
            DragDropEffects.None,
            screenPosition.X,
            screenPosition.Y);
    }

    private void UpdateEditorDragFeedback(SKPoint screenPosition)
    {
        if (!_isDraggingSelectedComponent && !_isDraggingSelectedColumn)
            return;

        SharedDragService.UpdatePointerPosition(screenPosition.X, screenPosition.Y);
        SharedDragService.SetEffectOverride(_previewDropValid ? DragDropEffects.Move : DragDropEffects.None);
        Mouse.SetCursor(_previewDropValid ? Cursors.Arrow : Cursors.No);
    }

    private static void EndEditorDragImage()
    {
        SharedDragService.End();
        Mouse.SetCursor(null);
    }

    private void UpdateHoveredComponent(SKPoint position, SKPoint localPosition)
    {
        if (_isLeftButtonDown || Host is not NodeDefinitionVM || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
        {
            SetHoveredComponent(null);
            SetHoveredColumnIndex(null);
            return;
        }

        if (TryGetColumnHandleIndex(layoutSnapshot, position, out var columnIndex))
        {
            SetHoveredComponent(null);
            SetHoveredColumnIndex(columnIndex);
            return;
        }

        SetHoveredColumnIndex(null);
        SetHoveredComponent(HitTestComponent(layoutSnapshot, position, localPosition));
    }

    private void SetHoveredComponent(NodeComponentVM? hoveredComponent)
    {
        if (ReferenceEquals(_hoveredComponent, hoveredComponent))
            return;

        if (_hoveredComponent != null)
            _hoveredComponent.IsHovered = false;

        _hoveredComponent = hoveredComponent;

        if (_hoveredComponent != null)
            _hoveredComponent.IsHovered = true;

        RebuildVisualTree();
    }

    private void SetHoveredColumnIndex(int? hoveredColumnIndex)
    {
        if (_hoveredColumnIndex == hoveredColumnIndex)
            return;

        _hoveredColumnIndex = hoveredColumnIndex;
        OverlayInvalidated?.Invoke();
    }

    private void SetSelectedColumnIndex(int? selectedColumnIndex)
    {
        if (_selectedColumnIndex == selectedColumnIndex)
            return;

        _selectedColumnIndex = selectedColumnIndex;
        OverlayInvalidated?.Invoke();
    }

    private void RenderColumnOverlay(SKCanvas canvas, NodeDefinitionVM definitionHost, NodeLayoutSnapshot layoutSnapshot)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(definitionHost);

        if (_hoveredColumnIndex is null && _selectedColumnIndex is null)
            return;

        var strokeColor = NodeVisualDrawing.GetNeutralColor(definitionHost.Model.Style?.ShapeStyle.BackgroundColor ?? new NodeStyle().ShapeStyle.BackgroundColor);
        var strokeWidth = 0.0f;
        var dashLength = NodeVisualDrawing.GetSelectionDashLength(GetMinorGridSize());

        if (_hoveredColumnIndex is int hoveredIndex && hoveredIndex != _selectedColumnIndex)
        {
            var hoveredColumn = layoutSnapshot.Columns.FirstOrDefault(column => column.Index == hoveredIndex);
            if (hoveredColumn != null)
                NodeVisualDrawing.DrawOutlineSelector(canvas, NodeVisualDrawing.OutlineSelectorMode.Dashed, hoveredColumn.GuideBounds, strokeColor, dashLength, strokeWidth);
        }

        if (_selectedColumnIndex is int selectedIndex)
        {
            var selectedColumn = layoutSnapshot.Columns.FirstOrDefault(column => column.Index == selectedIndex);
            if (selectedColumn != null)
            {
                NodeVisualDrawing.DrawOutlineSelector(canvas, NodeVisualDrawing.OutlineSelectorMode.Solid, selectedColumn.GuideBounds, strokeColor, dashLength, strokeWidth);
                DrawSelectedColumnHandle(canvas, layoutSnapshot, selectedColumn, strokeColor);
            }
        }
    }

    private float GetColumnHandleWidth()
    {
        return GetMinorGridSize() * 4.0f;
    }

    private float GetColumnHandleHeight()
    {
        return GetMinorGridSize();
    }

    private static float GetColumnHandleTopEdgeY(NodeLayoutSnapshot layoutSnapshot)
    {
        return layoutSnapshot.HeaderBounds?.Bottom ?? layoutSnapshot.NodeBounds.Top;
    }

    private void DrawSelectedColumnHandle(SKCanvas canvas, NodeLayoutSnapshot layoutSnapshot, NodeColumnLayoutSnapshot columnLayout, SKColor fillColor)
    {
        var handleBounds = GetColumnHandleBounds(layoutSnapshot, columnLayout, GetColumnHandleWidth(), GetColumnHandleHeight());
        var radius = handleBounds.Height * 0.5f;

        using var fillPaint = new SKPaint
        {
            Color = fillColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        canvas.DrawRoundRect(handleBounds, radius, radius, fillPaint);
    }

    private bool TryGetColumnHandleIndex(NodeLayoutSnapshot layoutSnapshot, SKPoint nodePoint, out int columnIndex)
    {
        var handleHeight = GetColumnHandleHeight();
        var handleWidth = GetColumnHandleWidth();

        foreach (var column in layoutSnapshot.Columns)
        {
            var handleBounds = GetColumnHandleBounds(layoutSnapshot, column, handleWidth, handleHeight);

            if (!handleBounds.Contains(nodePoint.X, nodePoint.Y))
                continue;

            columnIndex = column.Index;
            return true;
        }

        columnIndex = -1;
        return false;
    }

    private static SKRect GetColumnHandleBounds(NodeLayoutSnapshot layoutSnapshot, NodeColumnLayoutSnapshot columnLayout, float handleWidth, float handleHeight)
    {
        var headerBottom = GetColumnHandleTopEdgeY(layoutSnapshot);
        var halfWidth = handleWidth * 0.5f;
        var halfHeight = handleHeight * 0.5f;
        return new SKRect(
            columnLayout.ContentBounds.MidX - halfWidth,
            headerBottom - halfHeight,
            columnLayout.ContentBounds.MidX + halfWidth,
            headerBottom + halfHeight);
    }

    private void SuppressNoOpInsertion(NodeLayoutSnapshot layoutSnapshot)
    {
        if (_dragSourceComponent == null || !_previewInsertionY.HasValue)
            return;

        var sourceLayout = layoutSnapshot.Components
            .FirstOrDefault(component => ReferenceEquals(component.Component, _dragSourceComponent));
        if (sourceLayout == null)
            return;

        if (IsButtonLikeComponent(_dragSourceComponent))
        {
            var sharedButtonRowCount = layoutSnapshot.Components.Count(component =>
                IsButtonLikeComponent(component.Component)
                && component.ColumnIndex == sourceLayout.ColumnIndex
                && component.YOrder == sourceLayout.YOrder);

            if (sharedButtonRowCount > 1)
                return;
        }

        if (_previewComponent != null && sourceLayout.ColumnIndex != GetComponentColumnIndex(_previewComponent, layoutSnapshot))
            return;

        var insertionY = _previewInsertionY.Value;
        if (!AreClose(insertionY, sourceLayout.Bounds.Top) && !AreClose(insertionY, sourceLayout.Bounds.Bottom))
            return;

        _previewInsertionY = null;
        _previewPlacementBounds = SKRect.Empty;
        _previewDropYNu = _dragSourceComponent.Position.Y;
    }

    private static bool TryGetButtonInsertionIndicatorX(NodeLayoutSnapshot layoutSnapshot, PreviewRowLayout row, NodeComponentLayoutSnapshot hitLayout, bool inlineBefore, out float insertionX)
    {
        var rowButtons = row.Components
            .OrderBy(component => component.XOrder)
            .ThenBy(component => component.Bounds.Left)
            .ToList();
        var hitIndex = rowButtons.FindIndex(component => ReferenceEquals(component.Component, hitLayout.Component));
        if (hitIndex < 0)
        {
            insertionX = 0.0f;
            return false;
        }

        if (inlineBefore)
        {
            var columnLayout = layoutSnapshot.Columns[Math.Clamp(hitLayout.ColumnIndex, 0, layoutSnapshot.Columns.Count - 1)];
            var leftBoundary = hitIndex > 0
                ? rowButtons[hitIndex - 1].Bounds.Right
                : columnLayout.ContentBounds.Left;
            insertionX = (leftBoundary + hitLayout.Bounds.Left) * 0.5f;
            return true;
        }

        var targetColumnLayout = layoutSnapshot.Columns[Math.Clamp(hitLayout.ColumnIndex, 0, layoutSnapshot.Columns.Count - 1)];
        var rightBoundary = hitIndex < rowButtons.Count - 1
            ? rowButtons[hitIndex + 1].Bounds.Left
            : targetColumnLayout.ContentBounds.Right;
        insertionX = (hitLayout.Bounds.Right + rightBoundary) * 0.5f;
        return true;
    }

    private static int ResolveColumnMoveTargetIndex(int sourceIndex, int insertionSlotIndex, int columnCount)
    {
        if (columnCount <= 0)
            return 0;

        var resolvedSourceIndex = Math.Clamp(sourceIndex, 0, columnCount - 1);
        var resolvedInsertionSlotIndex = Math.Clamp(insertionSlotIndex, 0, columnCount);
        if (resolvedInsertionSlotIndex > resolvedSourceIndex)
            resolvedInsertionSlotIndex--;

        return Math.Clamp(resolvedInsertionSlotIndex, 0, columnCount - 1);
    }

    private static NodeComponentVM CreatePreviewComponent(NodeComponentVM component)
    {
        NodeComponentVM previewComponent = component switch
        {
            NodeLabelComponentVM labelComponent => new NodeLabelComponentVM(new NodeText { Label = labelComponent.Label }),
            NodeDropdownComponentVM dropdownComponent => CreatePreviewDropdownComponent(dropdownComponent),
            NodeInputComponentVM inputComponent => CreatePreviewInputComponent(inputComponent),
            NodeValueDisplayComponentVM valueDisplayComponent => new NodeValueDisplayComponentVM(new NodeValueDisplay
            {
                Label = valueDisplayComponent.Label,
                DefaultValue = valueDisplayComponent.DefaultValue,
                ValueColorMode = valueDisplayComponent.ValueColorMode,
                ValueColor = valueDisplayComponent.ValueColor,
            }),
            NodeGridInputComponentVM gridInputComponent => new NodeGridInputComponentVM(new NodeGridInput
            {
                Label = gridInputComponent.Label,
                Rows = gridInputComponent.Rows,
                Columns = gridInputComponent.Columns,
                CellWidth = gridInputComponent.CellWidth,
                CellTypeId = gridInputComponent.CellTypeId,
                MinValue = gridInputComponent.MinValue,
                MaxValue = gridInputComponent.MaxValue,
                DefaultValues = [.. gridInputComponent.DefaultValues],
            }),
            NodeIndicatorComponentVM indicatorComponent => new NodeIndicatorComponentVM(new NodeIndicator
            {
                Label = indicatorComponent.Label,
                Color = indicatorComponent.Color,
                Shape = indicatorComponent.Shape,
            }),
            NodeButtonComponentVM buttonComponent => new NodeButtonComponentVM(new NodeButton { Label = buttonComponent.Label }),
            NodeIconButtonComponentVM iconButtonComponent => new NodeIconButtonComponentVM(new NodeIconButton
            {
                Label = iconButtonComponent.Label,
                Svg = iconButtonComponent.Svg,
                ColorMode = iconButtonComponent.ColorMode,
                FixedWidth = iconButtonComponent.Model.FixedWidth,
            }),
            NodeSeparatorComponentVM separatorComponent => new NodeSeparatorComponentVM(new NodeSeparator { Label = separatorComponent.Label, ColorMode = separatorComponent.ColorMode }),
            NodeSpacerComponentVM spacerComponent => new NodeSpacerComponentVM(new NodeSpacer { Label = spacerComponent.Label, Height = spacerComponent.Height, Expand = spacerComponent.Expand }),
            NodePortComponentVM portComponent => new NodePortComponentVM(new NodePort
            {
                Label = portComponent.Label,
                Side = portComponent.Side,
                Shape = portComponent.Shape,
                FillColor = portComponent.FillColor,
            }),
            _ => throw new NotSupportedException($"Preview creation is not implemented for component type '{component.GetType().Name}'."),
        };

        previewComponent.SetColumn(component.Column, notifyComponentChanged: false);
        previewComponent.SetParentId(component.ParentId, notifyComponentChanged: false);
        previewComponent.SetYOrder(component.YOrder, notifyComponentChanged: false);
        previewComponent.SetXOrder(component.XOrder, notifyComponentChanged: false);
        return previewComponent;
    }

    private static NodeInputComponentVM CreatePreviewInputComponent(NodeInputComponentVM inputComponent)
    {
        var previewModel = new NodeInput
        {
            Label = inputComponent.Label,
            DisplayName = inputComponent.Label,
            TypeId = inputComponent.TypeId,
            DateTimeDisplayMode = inputComponent.DateTimeDisplayMode,
        };

        if (inputComponent.DefaultValue != null)
            previewModel.SetDefaultValue(inputComponent.DefaultValue, InputTypeRegistry.Default);

        return new NodeInputComponentVM(previewModel);
    }

    private static NodeDropdownComponentVM CreatePreviewDropdownComponent(NodeDropdownComponentVM dropdownComponent)
    {
        var previewModel = new NodeInput
        {
            Label = dropdownComponent.Label,
            DisplayName = dropdownComponent.Label,
            TypeId = InputTypeIds.StringSelection,
            DateTimeDisplayMode = dropdownComponent.DateTimeDisplayMode,
        };

        if (dropdownComponent.DefaultValue != null)
            previewModel.SetDefaultValue(dropdownComponent.DefaultValue, InputTypeRegistry.Default);

        return new NodeDropdownComponentVM(previewModel);
    }

    private void ApplyPreviewComponentPlacement(NodeComponentVM component, NodeLayoutSnapshot? layoutSnapshot, HitTestResult hitResult)
    {
        if (layoutSnapshot == null)
            return;

        if (component is NodePortComponentVM portComponent)
        {
            var previewColumnIndex = ResolvePreviewColumnIndex(layoutSnapshot, hitResult, _previewPointWorld);
            var lastColumnIndex = layoutSnapshot.Columns.Count - 1;
            if (layoutSnapshot.Columns.Count > 1)
            {
                if (previewColumnIndex <= 0)
                {
                    portComponent.Side = NodePortSide.Left;
                    portComponent.SetColumn(0, notifyComponentChanged: false);
                    return;
                }

                if (previewColumnIndex >= lastColumnIndex)
                {
                    portComponent.Side = NodePortSide.Right;
                    portComponent.SetColumn(lastColumnIndex, notifyComponentChanged: false);
                    return;
                }
            }

            if (hitResult.Flags.HasFlag(HitRegionFlags.Left) && !hitResult.Flags.HasFlag(HitRegionFlags.Right))
            {
                portComponent.Side = NodePortSide.Left;
                portComponent.SetColumn(0, notifyComponentChanged: false);
                return;
            }

            if (hitResult.Flags.HasFlag(HitRegionFlags.Right) && !hitResult.Flags.HasFlag(HitRegionFlags.Left))
            {
                portComponent.Side = NodePortSide.Right;
                portComponent.SetColumn(layoutSnapshot.Columns.Count - 1, notifyComponentChanged: false);
                return;
            }

            portComponent.Side = _previewPointWorld.X <= layoutSnapshot.ContentBounds.MidX
                ? NodePortSide.Left
                : NodePortSide.Right;
            portComponent.SetColumn(portComponent.Side == NodePortSide.Left ? 0 : lastColumnIndex, notifyComponentChanged: false);
            return;
        }

        component.SetColumn(ResolvePreviewColumnIndex(layoutSnapshot, hitResult, _previewPointWorld), notifyComponentChanged: false);
    }

    private HitTestResult GetPreviewHitResult(NodeLayoutSnapshot layoutSnapshot, SKPoint nodePoint, SKPoint screenPoint)
    {
        var hitResult = HitTest(layoutSnapshot, screenPoint);
        if (hitResult.Component == null)
            return hitResult;

        var componentLayout = layoutSnapshot.Components
            .FirstOrDefault(component => ReferenceEquals(component.Component, hitResult.Component));
        if (componentLayout == null)
            return hitResult;

        var flags = hitResult.Flags;

        if (!flags.HasFlag(HitRegionFlags.Top) && !flags.HasFlag(HitRegionFlags.Bottom) && !flags.HasFlag(HitRegionFlags.Middle))
        {
            var topThreshold = componentLayout.Bounds.Top + (componentLayout.Bounds.Height / 3.0f);
            var bottomThreshold = componentLayout.Bounds.Bottom - (componentLayout.Bounds.Height / 3.0f);
            flags |= nodePoint.Y <= topThreshold
                ? HitRegionFlags.Top
                : nodePoint.Y >= bottomThreshold
                    ? HitRegionFlags.Bottom
                    : HitRegionFlags.Middle;
        }

        if (!flags.HasFlag(HitRegionFlags.Left) && !flags.HasFlag(HitRegionFlags.Right))
        {
            flags |= nodePoint.X <= componentLayout.Bounds.MidX
                ? HitRegionFlags.Left
                : HitRegionFlags.Right;
        }

        return new HitTestResult(hitResult.Target, flags);
    }

    private NodeComponentVM? HitTestComponent(NodeLayoutSnapshot layoutSnapshot, SKPoint position, SKPoint screenPosition)
    {
        var hitResult = HitTest(layoutSnapshot, screenPosition);
        if (hitResult.Component != null)
            return hitResult.Component;

        return layoutSnapshot.Components
            .LastOrDefault(component => component.Bounds.Contains(position.X, position.Y))
            ?.Component;
    }

    private HitTestResult HitTest(NodeLayoutSnapshot layoutSnapshot, SKPoint screenPosition)
    {
        if (_viewportWidth <= 0.0f || _viewportHeight <= 0.0f)
            return HitTestResult.None;

        EnsureHitTestLayer(layoutSnapshot);
        if (_hitTestBitmap == null)
            return HitTestResult.None;

        var pixelX = (int)MathF.Floor(screenPosition.X);
        var pixelY = (int)MathF.Floor(screenPosition.Y);
        if (pixelX < 0 || pixelY < 0 || pixelX >= _hitTestBitmap.Width || pixelY >= _hitTestBitmap.Height)
            return HitTestResult.None;

        var hitId = DecodeHitColor(_hitTestBitmap.GetPixel(pixelX, pixelY));
        return _hitEntries.TryGetValue(hitId, out var hitResult)
            ? hitResult
            : HitTestResult.None;
    }

    private void EnsureHitTestLayer(NodeLayoutSnapshot layoutSnapshot)
    {
        if (!_hitTestLayerDirty)
            return;

        var bitmapWidth = Math.Max(1, (int)MathF.Ceiling(_viewportWidth));
        var bitmapHeight = Math.Max(1, (int)MathF.Ceiling(_viewportHeight));

        if (_hitTestBitmap == null || _hitTestBitmap.Width != bitmapWidth || _hitTestBitmap.Height != bitmapHeight)
        {
            _hitTestBitmap?.Dispose();
            _hitTestBitmap = new SKBitmap(bitmapWidth, bitmapHeight, true);
        }

        _hitEntries.Clear();

        using var canvas = new SKCanvas(_hitTestBitmap);
        canvas.Clear(SKColors.Transparent);
        var nodeSpace = new NodeSpace(_viewportWidth, _viewportHeight);
        canvas.SetMatrix(nodeSpace.CreateViewMatrix(Pan, Zoom));
        DrawHitTestLayer(canvas, layoutSnapshot);
        _hitTestLayerDirty = false;
    }

    private void DrawHitTestLayer(SKCanvas canvas, NodeLayoutSnapshot layoutSnapshot)
    {
        using var fillPaint = new SKPaint
        {
            IsAntialias = false,
            Style = SKPaintStyle.Fill,
        };

        var borderThickness = Host is NodeDefinitionVM definitionHost
            ? definitionHost.Model.Style?.ShapeStyle.BorderThickness ?? 1.0f
            : 1.0f;
        var nodeUnitSize = GetMinorGridSize();
        var nodeStrokeSize = nodeUnitSize / 4.0f;

        DrawButtonRowGapHitRegions(canvas, fillPaint, layoutSnapshot);

        foreach (var componentLayout in layoutSnapshot.Components)
        {
            if (IsButtonLikeComponent(componentLayout.Component))
            {
                DrawButtonHitRegions(canvas, fillPaint, componentLayout.Component, componentLayout.Bounds);
            }
            else
            {
                DrawQuadrantHitRegion(canvas, fillPaint, componentLayout.Component, new SKRect(componentLayout.Bounds.Left, componentLayout.Bounds.Top, componentLayout.Bounds.MidX, componentLayout.Bounds.MidY), HitRegionFlags.Top | HitRegionFlags.Left | HitRegionFlags.Body);
                DrawQuadrantHitRegion(canvas, fillPaint, componentLayout.Component, new SKRect(componentLayout.Bounds.MidX, componentLayout.Bounds.Top, componentLayout.Bounds.Right, componentLayout.Bounds.MidY), HitRegionFlags.Top | HitRegionFlags.Right | HitRegionFlags.Body);
                DrawQuadrantHitRegion(canvas, fillPaint, componentLayout.Component, new SKRect(componentLayout.Bounds.Left, componentLayout.Bounds.MidY, componentLayout.Bounds.MidX, componentLayout.Bounds.Bottom), HitRegionFlags.Bottom | HitRegionFlags.Left | HitRegionFlags.Body);
                DrawQuadrantHitRegion(canvas, fillPaint, componentLayout.Component, new SKRect(componentLayout.Bounds.MidX, componentLayout.Bounds.MidY, componentLayout.Bounds.Right, componentLayout.Bounds.Bottom), HitRegionFlags.Bottom | HitRegionFlags.Right | HitRegionFlags.Body);
            }

            if (componentLayout.Component is NodePortComponentVM portComponent)
            {
                DrawPortGlyphHitRegion(
                    canvas,
                    fillPaint,
                    portComponent,
                    componentLayout.Bounds,
                    borderThickness,
                    nodeUnitSize,
                    nodeStrokeSize,
                    layoutSnapshot.NodeBounds.Left,
                    layoutSnapshot.NodeBounds.Right);
            }
        }
    }

    private void DrawButtonRowGapHitRegions(SKCanvas canvas, SKPaint paint, NodeLayoutSnapshot layoutSnapshot)
    {
        var buttonRows = layoutSnapshot.Components
            .Where(component => IsButtonLikeComponent(component.Component))
            .GroupBy(component => (component.ColumnIndex, Top: component.Bounds.Top, Bottom: component.Bounds.Bottom))
            .Where(group => group.Any())
            .ToList();

        foreach (var row in buttonRows)
        {
            var buttons = row
                .OrderBy(component => component.Bounds.Left)
                .ToList();
            if (buttons.Count == 0)
                continue;

            var columnLayout = layoutSnapshot.Columns[Math.Clamp(row.Key.ColumnIndex, 0, layoutSnapshot.Columns.Count - 1)];
            var rowLeft = columnLayout.ContentBounds.Left;
            var rowRight = columnLayout.ContentBounds.Right;
            var first = buttons[0];
            var last = buttons[^1];

            if (first.Bounds.Left > rowLeft)
            {
                DrawButtonAuxHitRegion(canvas, paint, first.Component, new SKRect(rowLeft, first.Bounds.Top, first.Bounds.Left, first.Bounds.Bottom), useLeftFlags: true);
            }

            for (var i = 0; i < buttons.Count - 1; i++)
            {
                var leftButton = buttons[i];
                var rightButton = buttons[i + 1];
                if (rightButton.Bounds.Left <= leftButton.Bounds.Right)
                    continue;

                var gapMidX = (leftButton.Bounds.Right + rightButton.Bounds.Left) * 0.5f;
                DrawButtonAuxHitRegion(canvas, paint, leftButton.Component, new SKRect(leftButton.Bounds.Right, leftButton.Bounds.Top, gapMidX, leftButton.Bounds.Bottom), useLeftFlags: false);
                DrawButtonAuxHitRegion(canvas, paint, rightButton.Component, new SKRect(gapMidX, rightButton.Bounds.Top, rightButton.Bounds.Left, rightButton.Bounds.Bottom), useLeftFlags: true);
            }

            if (last.Bounds.Right < rowRight)
            {
                DrawButtonAuxHitRegion(canvas, paint, last.Component, new SKRect(last.Bounds.Right, last.Bounds.Top, rowRight, last.Bounds.Bottom), useLeftFlags: false);
            }
        }
    }

    private void DrawButtonAuxHitRegion(SKCanvas canvas, SKPaint paint, NodeComponentVM component, SKRect bounds, bool useLeftFlags)
    {
        if (bounds.IsEmpty || bounds.Width <= 0.0f)
            return;

        var thirdTop = bounds.Top + (bounds.Height / 3.0f);
        var thirdBottom = bounds.Bottom - (bounds.Height / 3.0f);
        var horizontalFlags = useLeftFlags ? HitRegionFlags.Left : HitRegionFlags.Right;

        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.Left, bounds.Top, bounds.Right, thirdTop), HitRegionFlags.Top | horizontalFlags | HitRegionFlags.Body);
        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.Left, thirdTop, bounds.Right, thirdBottom), HitRegionFlags.Middle | horizontalFlags | HitRegionFlags.Body);
        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.Left, thirdBottom, bounds.Right, bounds.Bottom), HitRegionFlags.Bottom | horizontalFlags | HitRegionFlags.Body);
    }

    private void DrawQuadrantHitRegion(SKCanvas canvas, SKPaint paint, NodeComponentVM component, SKRect bounds, HitRegionFlags flags)
    {
        if (bounds.IsEmpty)
            return;

        paint.Color = EncodeHitColor(RegisterHitEntry(component, flags));
        canvas.DrawRect(bounds, paint);
    }

    private void DrawButtonHitRegions(SKCanvas canvas, SKPaint paint, NodeComponentVM component, SKRect bounds)
    {
        var thirdTop = bounds.Top + (bounds.Height / 3.0f);
        var thirdBottom = bounds.Bottom - (bounds.Height / 3.0f);

        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.Left, bounds.Top, bounds.MidX, thirdTop), HitRegionFlags.Top | HitRegionFlags.Left | HitRegionFlags.Body);
        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.MidX, bounds.Top, bounds.Right, thirdTop), HitRegionFlags.Top | HitRegionFlags.Right | HitRegionFlags.Body);
        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.Left, thirdTop, bounds.MidX, thirdBottom), HitRegionFlags.Middle | HitRegionFlags.Left | HitRegionFlags.Body);
        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.MidX, thirdTop, bounds.Right, thirdBottom), HitRegionFlags.Middle | HitRegionFlags.Right | HitRegionFlags.Body);
        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.Left, thirdBottom, bounds.MidX, bounds.Bottom), HitRegionFlags.Bottom | HitRegionFlags.Left | HitRegionFlags.Body);
        DrawQuadrantHitRegion(canvas, paint, component, new SKRect(bounds.MidX, thirdBottom, bounds.Right, bounds.Bottom), HitRegionFlags.Bottom | HitRegionFlags.Right | HitRegionFlags.Body);
    }

    private static bool IsButtonLikeComponent(NodeComponentVM? component)
    {
        return component is NodeButtonComponentVM or NodeIconButtonComponentVM;
    }

    private void DrawPortGlyphHitRegion(
        SKCanvas canvas,
        SKPaint fillPaint,
        NodePortComponentVM portComponent,
        SKRect rowBounds,
        float borderThickness,
        float nodeUnitSize,
        float nodeStrokeSize,
        float nodeLeft,
        float nodeRight)
    {
        var placement = portComponent.Shape == NodePortShape.Hidden ? NodePortPlacement.Over : PortPlacement;
        var portBounds = NodePortGeometry.CreatePortBounds(
            rowBounds,
            portComponent.Side,
            placement,
            PortSize,
            nodeLeft,
            nodeRight,
            borderThickness,
            nodeUnitSize,
            nodeStrokeSize);
        var hitFlags = HitRegionFlags.Glyph | (portComponent.Side == NodePortSide.Left ? HitRegionFlags.Left : HitRegionFlags.Right);
        fillPaint.Color = EncodeHitColor(RegisterHitEntry(portComponent, hitFlags));

        switch (portComponent.Shape)
        {
            case NodePortShape.Circle:
                canvas.DrawOval(portBounds, fillPaint);
                break;

            case NodePortShape.Square:
                canvas.DrawRect(portBounds, fillPaint);
                break;

            case NodePortShape.Diamond:
                using (var path = NodePortGeometry.CreateDiamondPath(portBounds))
                    canvas.DrawPath(path, fillPaint);
                break;

            case NodePortShape.ArrowIn:
                using (var path = NodePortGeometry.CreateArrowPath(portBounds, portComponent.Side, pointsIntoNode: true))
                    canvas.DrawPath(path, fillPaint);
                break;

            case NodePortShape.ArrowOut:
                using (var path = NodePortGeometry.CreateArrowPath(portBounds, portComponent.Side, pointsIntoNode: false))
                    canvas.DrawPath(path, fillPaint);
                break;

            case NodePortShape.Hidden:
                using (var strokePaint = new SKPaint
                {
                    Color = fillPaint.Color,
                    IsAntialias = false,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = Math.Max(1.0f, nodeStrokeSize),
                })
                {
                    canvas.DrawLine(portBounds.Left, portBounds.Top, portBounds.Right, portBounds.Bottom, strokePaint);
                    canvas.DrawLine(portBounds.Left, portBounds.Bottom, portBounds.Right, portBounds.Top, strokePaint);
                }
                break;
        }
    }

    private int RegisterHitEntry(object target, HitRegionFlags flags)
    {
        var hitEntryId = _hitEntries.Count + 1;
        _hitEntries[hitEntryId] = new HitTestResult(target, flags);
        return hitEntryId;
    }

    private NodeColumnLayoutSnapshot GetPreviewColumnLayout(NodeLayoutSnapshot layoutSnapshot)
    {
        return GetColumnLayout(layoutSnapshot, _previewComponent != null ? GetComponentColumnIndex(_previewComponent, layoutSnapshot) : 0);
    }

    private static NodeColumnLayoutSnapshot GetColumnLayout(NodeLayoutSnapshot layoutSnapshot, int columnIndex)
    {
        var resolvedIndex = Math.Clamp(columnIndex, 0, layoutSnapshot.Columns.Count - 1);
        return layoutSnapshot.Columns[resolvedIndex];
    }

    private static int GetComponentColumnIndex(NodeComponentVM component, NodeLayoutSnapshot layoutSnapshot)
    {
        if (component is NodePortComponentVM portComponent)
            return portComponent.Side == NodePortSide.Left ? 0 : layoutSnapshot.Columns.Count - 1;

        return Math.Clamp(component.Column, 0, layoutSnapshot.Columns.Count - 1);
    }

    private static int ResolvePreviewColumnIndex(NodeLayoutSnapshot layoutSnapshot, HitTestResult hitResult, SKPoint nodePoint)
    {
        if (hitResult.Component != null)
        {
            var componentLayout = layoutSnapshot.Components
                .FirstOrDefault(component => ReferenceEquals(component.Component, hitResult.Component));
            if (componentLayout != null)
                return componentLayout.ColumnIndex;
        }

        var directColumn = layoutSnapshot.Columns
            .FirstOrDefault(column => nodePoint.X >= column.Bounds.Left && nodePoint.X <= column.Bounds.Right);
        if (directColumn != null)
            return directColumn.Index;

        return layoutSnapshot.Columns
            .OrderBy(column => MathF.Abs(column.ContentBounds.MidX - nodePoint.X))
            .First()
            .Index;
    }

    private static List<PreviewRowLayout> BuildPreviewRows(NodeDragProjectedColumn column, NodeLayoutSnapshot layoutSnapshot)
    {
        var componentLayouts = layoutSnapshot.Components.ToDictionary(component => component.Component);
        var rows = new List<PreviewRowLayout>();

        foreach (var row in column.Rows)
        {
            var orderedComponents = row.Components
                .Select(component => componentLayouts.TryGetValue(component, out var componentLayout) ? componentLayout : null)
                .Where(componentLayout => componentLayout != null)
                .Cast<NodeComponentLayoutSnapshot>()
                .OrderBy(component => component.XOrder)
                .ThenBy(component => component.Bounds.Left)
                .ToList();
            if (orderedComponents.Count == 0)
                continue;

            var primaryComponent = orderedComponents[0];
            var bounds = orderedComponents
                .Skip(1)
                .Aggregate(primaryComponent.Bounds, (current, component) => new SKRect(
                    MathF.Min(current.Left, component.Bounds.Left),
                    MathF.Min(current.Top, component.Bounds.Top),
                    MathF.Max(current.Right, component.Bounds.Right),
                    MathF.Max(current.Bottom, component.Bounds.Bottom)));

            rows.Add(new PreviewRowLayout(rows.Count, row.Id, row.YOrder, orderedComponents, primaryComponent, bounds));
        }

        return rows;
    }

    private static int GetInlineDropXOrder(PreviewRowLayout row, NodeComponentLayoutSnapshot hitLayout, bool inlineBefore)
    {
        var rowButtons = row.Components
            .Where(component => IsButtonLikeComponent(component.Component))
            .OrderBy(component => component.XOrder)
            .ToList();
        var hitIndex = rowButtons.FindIndex(component => ReferenceEquals(component.Component, hitLayout.Component));
        if (hitIndex < 0)
            return 0;

        return inlineBefore ? hitIndex : hitIndex + 1;
    }

    private NodeDragProjection BuildDragProjection(NodeDefinitionVM definitionHost)
    {
        return NodeDragProjectionBuilder.Build(
            definitionHost,
            _dragSourceComponent != null ? [_dragSourceComponent] : null);
    }

    private IReadOnlySet<NodeComponentVM>? GetGhostedComponents()
    {
        if (_isDraggingSelectedComponent && _dragSourceComponent != null)
            return new HashSet<NodeComponentVM> { _dragSourceComponent };

        if (_isDraggingSelectedColumn
            && _dragSourceColumnIndex.HasValue
            && Host is NodeDefinitionVM definitionHost)
        {
            var orderedColumns = definitionHost.Model.Columns
                .OrderBy(column => column.XOrder)
                .ThenBy(column => column.Id, StringComparer.Ordinal)
                .ToList();
            if (_dragSourceColumnIndex.Value >= 0 && _dragSourceColumnIndex.Value < orderedColumns.Count)
            {
                var draggedColumn = orderedColumns[_dragSourceColumnIndex.Value];
                var draggedComponents = draggedColumn.Rows
                    .SelectMany(row => row.Components)
                    .ToHashSet();
                return definitionHost.NodeComponents
                    .Where(component => draggedComponents.Contains(component.Model))
                    .ToHashSet();
            }
        }

        return null;
    }

    private void CommitPreviewDrop(NodeDefinitionVM definitionHost, NodeComponentVM component, bool isNewComponent)
    {
        ArgumentNullException.ThrowIfNull(definitionHost);
        ArgumentNullException.ThrowIfNull(component);

        ApplyPreviewComponentPlacement(component, (NodeLayoutSnapshot?)LayoutSnapshot, HitTestResult.None);

        if (isNewComponent)
            definitionHost.AddComponent(component, notifyDocumentChanged: false);

        var projection = NodeDragProjectionBuilder.Build(definitionHost, [component]);
        projection = NodeDragProjectionBuilder.InsertTransfer(
            projection,
            _previewDropColumnIndex,
            _previewDropRowIndex,
            _previewInlineWithRow,
            _previewDropXOrder);
        definitionHost.ApplyDragProjection(projection, notifyDocumentChanged: true);
    }

    private sealed record PreviewRowLayout(
        int RowIndex,
        string RowId,
        int YOrder,
        IReadOnlyList<NodeComponentLayoutSnapshot> Components,
        NodeComponentLayoutSnapshot PrimaryComponent,
        SKRect Bounds);

    private void MarkHitTestLayerDirty(bool resetBitmap = false)
    {
        _hitTestLayerDirty = true;

        if (!resetBitmap)
            return;

        _hitTestBitmap?.Dispose();
        _hitTestBitmap = null;
    }

    private static SKColor EncodeHitColor(int hitId)
    {
        var red = (byte)(hitId & 0xFF);
        var green = (byte)((hitId >> 8) & 0xFF);
        var blue = (byte)((hitId >> 16) & 0xFF);
        return new SKColor(red, green, blue, byte.MaxValue);
    }

    private static int DecodeHitColor(SKColor color)
    {
        return color.Red | (color.Green << 8) | (color.Blue << 16);
    }

    private static NodeColor ToNodeColor(SKColor color)
    {
        return new NodeColor(color.Red, color.Green, color.Blue, color.Alpha);
    }

    private static bool AreClose(float left, float right)
    {
        return MathF.Abs(left - right) < 0.01f;
    }

    private bool TryGetPreviewPortGhostBounds(NodeDefinitionVM definitionHost, out SKRect bounds)
    {
        bounds = SKRect.Empty;

        if (_previewComponent is not NodePortComponentVM previewPort
            || _dragSourceComponent is not NodePortComponentVM sourcePort
            || previewPort.Side == sourcePort.Side
            || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return false;

        var sourceBounds = layoutSnapshot.Components
            .FirstOrDefault(component => ReferenceEquals(component.Component, _dragSourceComponent))
            ?.Bounds;

        if (!sourceBounds.HasValue)
            return false;

        if (!_previewPlacementBounds.IsEmpty)
        {
            if (!AreClose(_previewPlacementBounds.Top, sourceBounds.Value.Top)
                || !AreClose(_previewPlacementBounds.Bottom, sourceBounds.Value.Bottom))
                return false;

            bounds = sourceBounds.Value;
            return true;
        }

        if (_previewInsertionY.HasValue)
        {
            if (!AreClose(_previewInsertionY.Value, sourceBounds.Value.Top)
                && !AreClose(_previewInsertionY.Value, sourceBounds.Value.Bottom))
                return false;

            bounds = sourceBounds.Value;
            return true;
        }

        if (!AreClose(_previewDropYNu, _dragSourceComponent.Position.Y))
            return false;

        bounds = sourceBounds.Value;
        return true;
    }

    private void OnHistoryStateChanged(object? sender, HistoryStateArgs e)
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        CommandManager.InvalidateRequerySuggested();
    }

    private static SKTypeface? CreateTypeface(string? fontFamily, NodeFontWeight fontWeight)
    {
        var skiaWeight = fontWeight switch
        {
            NodeFontWeight.Light => SKFontStyleWeight.Light,
            NodeFontWeight.Medium => SKFontStyleWeight.Medium,
            NodeFontWeight.SemiBold => SKFontStyleWeight.SemiBold,
            NodeFontWeight.Bold => SKFontStyleWeight.Bold,
            _ => SKFontStyleWeight.Normal,
        };

        return string.IsNullOrWhiteSpace(fontFamily)
            ? SKTypeface.FromFamilyName(null, skiaWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            : SKTypeface.FromFamilyName(fontFamily, skiaWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    }

    private NodeCanvasStyleContext CreateCanvasStyleContext()
    {
        return new NodeCanvasStyleContext(
            FontFamily,
            FontSize,
            HeaderFontSize,
            FontColor,
            LayoutPadding,
            RowSpacing,
            ColumnSpacing,
            RoundedCornerRadius,
            ControlCornerRadius,
            MinimumColumnWidth,
            PortSize,
            PortPlacement);
    }

    #endregion Private Methods
}
