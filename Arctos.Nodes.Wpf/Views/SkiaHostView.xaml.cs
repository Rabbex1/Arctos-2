using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels;
using SkiaSharp.Views.Desktop;

namespace Arctos.Nodes.Wpf.Views;

/// <summary>
/// Hosts the Skia rendering surface and forwards overlay input into the view model.
/// </summary>
public partial class SkiaHostView : UserControl
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new Skia host view.
    /// </summary>
    public SkiaHostView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (DataContext is not SkiaHostVM skiaHost)
            return;

        skiaHost.OnPaintSurface(e);
    }

    private void OnOverlayPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (DataContext is not SkiaHostVM skiaHost)
            return;

        skiaHost.OnOverlayPaintSurface(e);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SkiaHostVM oldHost)
        {
            oldHost.CanvasInvalidated -= OnCanvasInvalidated;
            oldHost.OverlayInvalidated -= OnOverlayInvalidated;
        }

        if (e.NewValue is SkiaHostVM newHost)
        {
            newHost.CanvasInvalidated += OnCanvasInvalidated;
            newHost.OverlayInvalidated += OnOverlayInvalidated;
        }

        SkiaCanvas.InvalidateVisual();
        OverlayCanvas.InvalidateVisual();
    }

    private void OnCanvasInvalidated()
    {
        SkiaCanvas.InvalidateVisual();
    }

    private void OnOverlayInvalidated()
    {
        OverlayCanvas.InvalidateVisual();
    }

    private void OnOverlayMouseMove(object sender, MouseEventArgs e)
    {
        SendPointerInput(e, PointerInputType.Move);
    }

    private void OnOverlayMouseLeave(object sender, MouseEventArgs e)
    {
        SendPointerInput(e, PointerInputType.Leave);
    }

    private void OnOverlayMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        FocusOverlay();
        InputOverlay.CaptureMouse();
        SendPointerInput(e, PointerInputType.LeftButtonDown);
    }

    private void OnOverlayMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (InputOverlay.IsMouseCaptured)
            InputOverlay.ReleaseMouseCapture();

        SendPointerInput(e, PointerInputType.LeftButtonUp);
    }

    private void OnOverlayMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle)
            return;

        FocusOverlay();
        InputOverlay.CaptureMouse();
        SendPointerInput(e, PointerInputType.MiddleButtonDown);
    }

    private void OnOverlayMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle)
            return;

        if (InputOverlay.IsMouseCaptured)
            InputOverlay.ReleaseMouseCapture();

        SendPointerInput(e, PointerInputType.MiddleButtonUp);
    }

    private void OnOverlayMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        FocusOverlay();
        SendPointerInput(e, PointerInputType.RightButtonDown);
    }

    private void OnOverlayMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        SendPointerInput(e, PointerInputType.RightButtonUp);
    }

    private void OnOverlayMouseWheel(object sender, MouseWheelEventArgs e)
    {
        SendPointerInput(e, PointerInputType.Wheel);
    }

    private void SendPointerInput(MouseEventArgs e, PointerInputType inputType)
    {
        if (DataContext is not SkiaHostVM skiaHost)
            return;

        var position = e.GetPosition(InputOverlay);
        var nodeSpace = new NodeSpace((float)InputOverlay.ActualWidth, (float)InputOverlay.ActualHeight);
        var nodePoint = nodeSpace.ToNodePoint(position.X, position.Y, skiaHost.Pan, skiaHost.Zoom);
        var localPoint = new SkiaSharp.SKPoint((float)position.X, (float)position.Y);
        var screenPosition = InputOverlay.PointToScreen(position);
        var screenPoint = new SkiaSharp.SKPoint((float)screenPosition.X, (float)screenPosition.Y);
        var clickCount = e is MouseButtonEventArgs mouseButtonEvent ? mouseButtonEvent.ClickCount : 0;
        var wheelDelta = e is MouseWheelEventArgs mouseWheelEvent ? mouseWheelEvent.Delta : 0;

        skiaHost.OnPointerInput(new PointerInput(inputType, nodePoint, localPoint, screenPoint, clickCount, wheelDelta));
    }

    private void FocusOverlay()
    {
        if (!InputOverlay.Focusable)
            return;

        Keyboard.Focus(InputOverlay);
    }

    #endregion Private Methods
}
