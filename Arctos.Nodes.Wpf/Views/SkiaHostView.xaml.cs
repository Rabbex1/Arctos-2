using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels;
using SkiaSharp.Views.Desktop;

namespace Arctos.Nodes.Wpf.Views;

public partial class SkiaHostView : UserControl
{
    public SkiaHostView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (DataContext is not SkiaHostVM skiaHost)
            return;

        skiaHost.OnPaintSurface(e);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SkiaHostVM oldHost)
            oldHost.CanvasInvalidated -= OnCanvasInvalidated;

        if (e.NewValue is SkiaHostVM newHost)
            newHost.CanvasInvalidated += OnCanvasInvalidated;

        SkiaCanvas.InvalidateVisual();
    }

    private void OnCanvasInvalidated()
    {
        SkiaCanvas.InvalidateVisual();
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
        SendPointerInput(e, PointerInputType.LeftButtonDown);
    }

    private void OnOverlayMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        SendPointerInput(e, PointerInputType.LeftButtonUp);
    }

    private void OnOverlayMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle)
            return;

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
        var screenPoint = new SkiaSharp.SKPoint((float)position.X, (float)position.Y);
        var clickCount = e is MouseButtonEventArgs mouseButtonEvent ? mouseButtonEvent.ClickCount : 0;
        var wheelDelta = e is MouseWheelEventArgs mouseWheelEvent ? mouseWheelEvent.Delta : 0;

        skiaHost.OnPointerInput(new PointerInput(inputType, nodePoint, screenPoint, clickCount, wheelDelta));
    }
}
