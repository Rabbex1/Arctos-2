using Arctos;
using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Builders;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.Visuals;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Hosts a Skia rendering surface for the current node-space view.
/// </summary>
public sealed class SkiaHostVM : BaseVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private bool _isPanning;
    private SKPoint _lastPanScreenPosition;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    #region Host
    private IModelHost? _host;
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

    public object? Model => _host?.Model;

    #region Pan
    private NodePoint _pan = new(0.0f, 0.0f);
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
    public float Zoom
    {
        get => _zoom;
        set
        {
            var zoom = value <= 0.0f ? 0.01f : value;

            if (!SetProperty(ref _zoom, zoom))
                return;

            InvalidateCanvas();
        }
    }
    #endregion

    #region BackgroundColor
    private NodeColor _backgroundColor = NodeColor.Transparent;
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
    public IVisual? VisualRoot
    {
        get => _visualRoot;
        set => SetVisualRoot(value);
    }
    #endregion

    #region DetailedView
    private bool _detailedView;
    public bool DetailedView
    {
        get => _detailedView;
        set
        {
            if (!SetProperty(ref _detailedView, value))
                return;

            RebuildVisualTree();
        }
    }
    #endregion

    #region VisualTreeBuilder
    private VisualTreeBuilder _visualTreeBuilder = new();
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

    public bool ShowDrawModeToggle => true;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events

    /// <summary>
    /// Raised when the hosted Skia surface should be invalidated and repainted.
    /// </summary>
    public event Action? CanvasInvalidated;

    #endregion Events

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

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

        if (VisualRoot == null)
            throw new InvalidOperationException("The visual root was not built before the Skia surface requested a paint.");

        VisualRoot.Bounds = nodeSpace.GetVisibleBounds(Pan, Zoom);
        var viewMatrix = nodeSpace.CreateViewMatrix(Pan, Zoom);

        canvas.Save();
        canvas.SetMatrix(viewMatrix);
        VisualRoot.Render(canvas);
        canvas.Restore();
    }

    /// <summary>
    /// Invalidates the hosted Skia surface so it will repaint.
    /// </summary>
    public void InvalidateCanvas()
    {
        CanvasInvalidated?.Invoke();
    }

    /// <summary>
    /// Rebuilds the current visual tree from the active host context.
    /// </summary>
    public void RebuildVisualTree()
    {
        SetVisualRoot(VisualTreeBuilder.Build(Host, DetailedView, BackgroundColor));
    }

    /// <summary>
    /// Handles pointer input forwarded from the transparent host overlay.
    /// </summary>
    /// <param name="pointerInput">The centered node-space pointer input event.</param>
    public void OnPointerInput(PointerInput pointerInput)
    {
        switch (pointerInput.InputType)
        {
            case PointerInputType.MiddleButtonDown:
                _isPanning = true;
                _lastPanScreenPosition = pointerInput.ScreenPosition;
                break;

            case PointerInputType.MiddleButtonUp:
            case PointerInputType.Leave:
                _isPanning = false;
                break;

            case PointerInputType.Move when _isPanning:
                var deltaX = pointerInput.ScreenPosition.X - _lastPanScreenPosition.X;
                var deltaY = pointerInput.ScreenPosition.Y - _lastPanScreenPosition.Y;
                _lastPanScreenPosition = pointerInput.ScreenPosition;

                if (deltaX == 0.0f && deltaY == 0.0f)
                    return;

                Pan = new NodePoint(Pan.X + (deltaX / Zoom), Pan.Y + (deltaY / Zoom));
                break;

            case PointerInputType.Wheel when pointerInput.WheelDelta != 0:
                var zoomFactor = MathF.Pow(1.1f, pointerInput.WheelDelta / 120.0f);
                Zoom *= zoomFactor;
                break;
        }
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void SetVisualRoot(IVisual? visualRoot)
    {
        if (!SetProperty(ref _visualRoot, visualRoot, nameof(VisualRoot)))
            return;

        if (_visualRoot is CanvasVisual canvasVisual)
            canvasVisual.BackgroundColor = BackgroundColor;

        InvalidateCanvas();
    }

    #endregion Private Methods
}
