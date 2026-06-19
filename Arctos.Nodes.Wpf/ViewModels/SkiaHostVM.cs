using Arctos;
using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Builders;
using Arctos.Nodes.Wpf.Layout;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Nodes.Wpf.Visuals;
using Arctos.Wpf.DragDrop;
using Arctos.Wpf;
using Arctos.Wpf.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows.Input;
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
    private bool _isPanning;
    private bool _isLeftButtonDown;
    private bool _isDraggingSelectedComponent;
    private SKPoint _lastPanScreenPosition;
    private SKPoint _leftButtonDownPosition;
    private ILayoutEngine _layoutEngine = new NodeLayoutEngine();
    private ILayoutSnapshot? _layoutSnapshot;
    private NodeComponentVM? _dragSourceComponent;
    private NodeComponentVM? _hoveredComponent;
    private NodeComponentVM? _pressedComponent;
    private NodeComponentVM? _previewComponent;
    private SKPoint _previewPointWorld = new(0.0f, 0.0f);
    private SKRect _previewPlacementBounds = SKRect.Empty;
    private float? _previewInsertionY;
    private float _previewDropYNu;
    private bool _previewVisible;
    private ICommand? _deleteSelectedComponentCommand;
    private ICommand? _fitToViewCommand;
    private float _viewportHeight;
    private float _viewportWidth;

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
            if (!SetProperty(ref _host, value))
                return;

            OnPropertyChanged(nameof(Model));
            RebuildVisualTree();
        }
    }
    #endregion

    /// <summary>
    /// Gets the raw current model being shown by the canvas.
    /// </summary>
    public object? Model => _host?.Model;

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

            OnPropertyChanged(nameof(ZoomText));
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
        _viewportWidth = e.Info.Width;
        _viewportHeight = e.Info.Height;

        if (VisualRoot == null)
            throw new InvalidOperationException("The visual root was not built before the Skia surface requested a paint.");

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

        if (!_previewVisible || _previewComponent == null || Host is not NodeDefinitionVM definitionHost)
            return;

        var nodeSpace = new NodeSpace(e.Info.Width, e.Info.Height);
        var viewMatrix = nodeSpace.CreateViewMatrix(Pan, Zoom);
        canvas.Save();
        canvas.SetMatrix(viewMatrix);
        RenderPreviewOverlay(canvas, definitionHost);
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
        SetVisualRoot(VisualTreeBuilder.Build(Host, Overview, BackgroundColor, GetMinorGridSize(), canvasStyle, LayoutSnapshot, showDebugBounds: ShowDebugBounds));
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
                HandleLeftButtonDown(pointerInput.Position);
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
                    ClearPreview();
                }
                break;

            case PointerInputType.Move:
                if (HandleComponentDragMove(pointerInput.Position))
                    break;

                UpdateHoveredComponent(pointerInput.Position);

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

        _previewComponent = paletteItem.CreateComponent();
        UpdatePreview(location);
    }

    /// <summary>
    /// Handles a compatible drag payload moving over the canvas.
    /// </summary>
    /// <param name="data">The dragged data payload.</param>
    /// <param name="location">The pointer location relative to the canvas.</param>
    public void OnDragOver(object data, Point location)
    {
        if (!CanDrop(data) || _previewComponent == null)
            return;

        UpdatePreview(location);
    }

    /// <summary>
    /// Handles the current drag payload leaving the canvas.
    /// </summary>
    public void OnDragLeave()
    {
        if (_previewComponent == null)
            return;

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

        if (!_previewVisible)
            return;

        var component = _previewComponent ?? paletteItem.CreateComponent();
        component.Position = new NodePoint(component.Position.X, _previewDropYNu);
        ApplyPreviewComponentPlacement(component);
        definitionHost.AddComponent(component);
        ClearPreview(false);
        RebuildVisualTree();
    }

    /// <summary>
    /// Returns the required drag/drop effect for the supplied object.
    /// </summary>
    /// <param name="dropObject">The dragged object.</param>
    /// <returns>The required drag/drop effect.</returns>
    public DragDropEffects GetRequiredEffect(object dropObject)
    {
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

        _zoom = fitZoom;
        _pan = new NodePoint(-centerX, -centerY);

        OnPropertyChanged(nameof(Zoom));
        OnPropertyChanged(nameof(ZoomText));
        OnPropertyChanged(nameof(Pan));
        InvalidateCanvas();
    }

    private bool CanDeleteSelectedComponent()
    {
        return Host is NodeDefinitionVM definitionHost && definitionHost.SelectedComponent != null;
    }

    private void DeleteSelectedComponent()
    {
        if (Host is not NodeDefinitionVM definitionHost)
            return;

        var componentToDelete = definitionHost.SelectedComponent;

        if (!definitionHost.RemoveSelectedComponent())
            return;

        if (ReferenceEquals(_hoveredComponent, componentToDelete))
            SetHoveredComponent(null);

        RebuildVisualTree();
    }

    private void HandleLeftButtonDown(SKPoint position)
    {
        if (Host is not NodeDefinitionVM definitionHost || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        _isLeftButtonDown = true;
        _leftButtonDownPosition = position;
        _pressedComponent = HitTestComponent(layoutSnapshot, position);

        var selectionChanged = _pressedComponent != null
            ? definitionHost.SelectComponent(_pressedComponent)
            : definitionHost.ClearComponentSelection();

        if (selectionChanged)
            RebuildVisualTree();
    }

    private void HandleLeftButtonUp()
    {
        _isLeftButtonDown = false;

        if (_isDraggingSelectedComponent && _dragSourceComponent != null)
        {
            if (_previewVisible)
            {
                _dragSourceComponent.Position = new NodePoint(_dragSourceComponent.Position.X, _previewDropYNu);
                ApplyPreviewComponentPlacement(_dragSourceComponent);
                RebuildVisualTree();
            }

            ClearSelectedComponentDrag();
            return;
        }

        _pressedComponent = null;
    }

    private bool HandleComponentDragMove(SKPoint position)
    {
        if (!_isLeftButtonDown || Host is not NodeDefinitionVM definitionHost || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return false;

        if (!_isDraggingSelectedComponent)
        {
            if (_pressedComponent == null)
                return false;

            var deltaX = position.X - _leftButtonDownPosition.X;
            var deltaY = position.Y - _leftButtonDownPosition.Y;
            var dragDistanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            if (dragDistanceSquared < 16.0f)
                return true;

            _isDraggingSelectedComponent = true;
            _dragSourceComponent = _pressedComponent;
            SetHoveredComponent(null);
            _previewComponent = CreatePreviewComponent(_pressedComponent);
        }

        UpdatePreview(position, layoutSnapshot);
        return true;
    }

    private void RenderPreviewOverlay(SKCanvas canvas, NodeDefinitionVM definitionHost)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(definitionHost);

        var indicatorColor = GetPreviewBaseColor(definitionHost);

        if (_previewInsertionY.HasValue)
        {
            DrawInsertionIndicator(canvas, indicatorColor, _previewInsertionY.Value);
        }
        else if (!_previewPlacementBounds.IsEmpty)
        {
            using var placementPaint = new SKPaint
            {
                Color = indicatorColor.WithAlpha((byte)Math.Clamp(MathF.Round(PreviewIndicatorOpacity * 255.0f), 0.0f, 255.0f)),
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
            var layoutSnapshot = (NodeLayoutSnapshot?)LayoutSnapshot;
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
                layoutSnapshot?.NodeBounds.Left ?? definitionHost.Size.Width * -0.5f * GetMinorGridSize(),
                layoutSnapshot?.NodeBounds.Right ?? definitionHost.Size.Width * 0.5f * GetMinorGridSize(),
                0.28f)
            {
                Bounds = portGhostBounds,
            };

            portVisual.Render(canvas);
        }

        if (_previewComponent is not NodeLabelComponentVM labelComponent)
            return;

        using var textPaint = CreatePreviewTextPaint(definitionHost);
        var textBounds = SKRect.Empty;
        var text = labelComponent.Label ?? string.Empty;
        textPaint.MeasureText(text, ref textBounds);
        var baseline = _previewPointWorld.Y - textBounds.Top;
        canvas.DrawText(text, _previewPointWorld.X, baseline, textPaint);
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

    private void UpdatePreview(Point location)
    {
        if (_previewComponent == null || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        var nodeSpace = new NodeSpace(_viewportWidth, _viewportHeight);
        _previewPointWorld = nodeSpace.ToNodePoint((float)location.X, (float)location.Y, Pan, Zoom);
        UpdatePreview(_previewPointWorld, layoutSnapshot);
    }

    private void UpdatePreview(SKPoint nodePoint, NodeLayoutSnapshot layoutSnapshot)
    {
        if (_previewComponent == null)
            return;

        _previewPointWorld = nodePoint;

        if (!layoutSnapshot.NodeBounds.Contains(_previewPointWorld.X, _previewPointWorld.Y))
        {
            _previewVisible = false;
            _previewPlacementBounds = SKRect.Empty;
            _previewInsertionY = null;
            OverlayInvalidated?.Invoke();
            return;
        }

        _previewVisible = true;
        var minorGridSize = GetMinorGridSize();
        _previewComponent.Position = new NodePoint(_previewPointWorld.X / minorGridSize, _previewPointWorld.Y / minorGridSize);
        ApplyPreviewComponentPlacement(_previewComponent);
        UpdateProjectedPlacement(layoutSnapshot);
        OverlayInvalidated?.Invoke();
    }

    private void UpdateProjectedPlacement(NodeLayoutSnapshot layoutSnapshot)
    {
        var lineHeight = layoutSnapshot.LineHeight;
        var components = layoutSnapshot.Components
            .Where(component => !ReferenceEquals(component.Component, _dragSourceComponent))
            .OrderBy(component => component.Bounds.Top)
            .ToList();
        var minorGridSize = GetMinorGridSize();

        if (components.Count == 0)
        {
            _previewInsertionY = null;
            _previewPlacementBounds = new SKRect(
                layoutSnapshot.ContentBounds.Left,
                layoutSnapshot.ContentBounds.Top,
                layoutSnapshot.ContentBounds.Right,
                layoutSnapshot.ContentBounds.Top + lineHeight);
            _previewDropYNu = layoutSnapshot.ContentBounds.Top / minorGridSize;
            return;
        }

        foreach (var component in components)
        {
            if (_previewPointWorld.Y > component.Bounds.Bottom)
                continue;

            if (_previewPointWorld.Y <= component.Bounds.MidY)
            {
                _previewInsertionY = component.Bounds.Top;
                _previewPlacementBounds = SKRect.Empty;
                _previewDropYNu = component.PositionNu.Y - 0.5f;
                SuppressNoOpInsertion(layoutSnapshot);
                return;
            }

            if (ReferenceEquals(component, components[^1]))
            {
                _previewInsertionY = component.Bounds.Bottom;
                _previewPlacementBounds = _dragSourceComponent == null
                    ? new SKRect(
                        layoutSnapshot.ContentBounds.Left,
                        component.Bounds.Bottom,
                        layoutSnapshot.ContentBounds.Right,
                        component.Bounds.Bottom + lineHeight)
                    : SKRect.Empty;
                _previewDropYNu = component.PositionNu.Y + 0.5f;
                SuppressNoOpInsertion(layoutSnapshot);
                return;
            }

            _previewInsertionY = component.Bounds.Bottom;
            _previewPlacementBounds = SKRect.Empty;
            _previewDropYNu = component.PositionNu.Y + 0.5f;
            SuppressNoOpInsertion(layoutSnapshot);
            return;
        }

        var last = components[^1];
        _previewInsertionY = last.Bounds.Bottom;
        _previewPlacementBounds = _dragSourceComponent == null
            ? new SKRect(
                layoutSnapshot.ContentBounds.Left,
                last.Bounds.Bottom,
                layoutSnapshot.ContentBounds.Right,
                last.Bounds.Bottom + lineHeight)
            : SKRect.Empty;
        _previewDropYNu = last.PositionNu.Y + 0.5f;
        SuppressNoOpInsertion(layoutSnapshot);
    }

    private void DrawInsertionIndicator(SKCanvas canvas, SKColor indicatorColor, float insertionY)
    {
        if (LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        var left = layoutSnapshot.ContentBounds.Left;
        var right = layoutSnapshot.ContentBounds.Right;
        var arrowSize = MathF.Max(3.0f, GetMinorGridSize() * 0.5f);
        var strokeWidth = 2.0f / MathF.Max(Zoom, 0.01f);
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

    private SKPaint CreatePreviewTextPaint(NodeDefinitionVM definitionHost)
    {
        var fontWeight = definitionHost.Model.Style?.ShapeStyle.FontWeight ?? NodeFontWeight.Normal;

        return new SKPaint
        {
            Color = FontColor
                .ToSkColor()
                .WithAlpha((byte)Math.Clamp(MathF.Round(PreviewOpacity * 255.0f), 0.0f, 255.0f)),
            IsAntialias = true,
            TextSize = FontSize,
            TextAlign = SKTextAlign.Left,
            Typeface = CreateTypeface(FontFamily, fontWeight),
        };
    }

    private SKColor GetPreviewBaseColor(NodeDefinitionVM definitionHost)
    {
        var backgroundColor = definitionHost.Model.Style?.ShapeStyle.BackgroundColor ?? BackgroundColor;
        var grayscale = ((backgroundColor.Red * 0.299f) + (backgroundColor.Green * 0.587f) + (backgroundColor.Blue * 0.114f));

        if (grayscale <= 64.0f || grayscale >= 191.0f)
            return new SKColor(128, 128, 128);

        if (grayscale <= 127.0f)
            return new SKColor(192, 192, 192);

        return new SKColor(72, 72, 72);
    }

    private void ClearPreview(bool invalidateOverlay = true)
    {
        _previewComponent = null;
        _previewVisible = false;
        _previewInsertionY = null;
        _previewPlacementBounds = SKRect.Empty;
        _previewDropYNu = 0.0f;
        _dragSourceComponent = null;

        if (invalidateOverlay)
            OverlayInvalidated?.Invoke();
    }

    private void ClearSelectedComponentDrag()
    {
        _pressedComponent = null;
        _isDraggingSelectedComponent = false;
        ClearPreview();
    }

    private void UpdateHoveredComponent(SKPoint position)
    {
        if (_isLeftButtonDown || Host is not NodeDefinitionVM || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
        {
            SetHoveredComponent(null);
            return;
        }

        SetHoveredComponent(HitTestComponent(layoutSnapshot, position));
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

    private void SuppressNoOpInsertion(NodeLayoutSnapshot layoutSnapshot)
    {
        if (_dragSourceComponent == null || !_previewInsertionY.HasValue)
            return;

        var sourceBounds = layoutSnapshot.Components
            .FirstOrDefault(component => ReferenceEquals(component.Component, _dragSourceComponent))
            ?.Bounds;

        if (!sourceBounds.HasValue)
            return;

        var insertionY = _previewInsertionY.Value;
        if (!AreClose(insertionY, sourceBounds.Value.Top) && !AreClose(insertionY, sourceBounds.Value.Bottom))
            return;

        _previewInsertionY = null;
        _previewPlacementBounds = SKRect.Empty;
        _previewDropYNu = _dragSourceComponent.Position.Y;
    }

    private static NodeComponentVM CreatePreviewComponent(NodeComponentVM component)
    {
        return component switch
        {
            NodeLabelComponentVM labelComponent => new NodeLabelComponentVM(new NodeText { Label = labelComponent.Label }),
            NodeInputComponentVM inputComponent => new NodeInputComponentVM(new NodeInput
            {
                Label = inputComponent.Label,
                DisplayName = inputComponent.Label,
                TypeId = inputComponent.TypeId,
            }),
            NodeButtonComponentVM buttonComponent => new NodeButtonComponentVM(new NodeButton { Label = buttonComponent.Label }),
            NodeSeparatorComponentVM separatorComponent => new NodeSeparatorComponentVM(new NodeSeparator { Label = separatorComponent.Label, ColorMode = separatorComponent.ColorMode }),
            NodeSpacerComponentVM spacerComponent => new NodeSpacerComponentVM(new NodeSpacer { Label = spacerComponent.Label, Height = spacerComponent.Height }),
            NodePortComponentVM portComponent => new NodePortComponentVM(new NodePort
            {
                Label = portComponent.Label,
                Side = portComponent.Side,
                Shape = portComponent.Shape,
                FillColor = portComponent.FillColor,
            }),
            _ => throw new NotSupportedException($"Preview creation is not implemented for component type '{component.GetType().Name}'."),
        };
    }

    private void ApplyPreviewComponentPlacement(NodeComponentVM component)
    {
        if (component is not NodePortComponentVM portComponent || LayoutSnapshot is not NodeLayoutSnapshot layoutSnapshot)
            return;

        portComponent.Side = _previewPointWorld.X <= layoutSnapshot.ContentBounds.MidX
            ? NodePortSide.Left
            : NodePortSide.Right;
    }

    private static NodeComponentVM? HitTestComponent(NodeLayoutSnapshot layoutSnapshot, SKPoint position)
    {
        return layoutSnapshot.Components
            .LastOrDefault(component => component.Bounds.Contains(position.X, position.Y))
            ?.Component;
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
            MinimumColumnWidth,
            PortSize,
            PortPlacement);
    }

    #endregion Private Methods
}
