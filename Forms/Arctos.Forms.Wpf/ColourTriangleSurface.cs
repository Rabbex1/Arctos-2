using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Arctos.Forms.Wpf;

/// <summary>Native bitmap colour wheel; the selection markers are drawn independently.</summary>
public sealed class ColourTriangleSurface : FrameworkElement
{
    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColourTriangleSurface),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorChanged));

    private double _hue;
    private ColourTriangle.Weights _weights = new(0, 1, 0);
    private bool _publishing;
    private bool _dragHue;
    private WriteableBitmap? _bitmap;
    private int[] _pixels = [];
    private int[] _ringPixels = [];
    private double _renderedHue = double.NaN;

    public ColourTriangleSurface()
    {
        Focusable = true;
        Cursor = Cursors.Cross;
        ToolTip = "Drag the ring for hue or the triangle for colour. Arrow keys move the colour; Ctrl+Left/Right changes hue.";
        System.Windows.Automation.AutomationProperties.SetName(this, "Hue ring and colour triangle");
    }

    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    private static void ColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var control = (ColourTriangleSurface)sender;
        if (!control._publishing)
        {
            var c = (Color)args.NewValue;
            // Alpha-only changes must not move the hue or selection marker.
            var old = (Color)args.OldValue;
            if (old.R != c.R || old.G != c.G || old.B != c.B)
                (control._hue, control._weights) = ColourTriangle.FromColor(new(c.R, c.G, c.B, c.A), control._hue);
        }
        control.InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Respect the host's slot so WPF does not arrange an oversized, clipped wheel.
        const double defaultSize = 240;
        var width = double.IsPositiveInfinity(availableSize.Width) ? defaultSize : availableSize.Width;
        var height = double.IsPositiveInfinity(availableSize.Height) ? defaultSize : availableSize.Height;
        var size = Math.Min(width, height);
        return new Size(size, size);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) return;
        var origin = new Point((ActualWidth - size) / 2, (ActualHeight - size) / 2);
        var dpi = VisualTreeHelper.GetDpi(this);
        var pixels = Math.Max(1, (int)Math.Ceiling(size * Math.Max(dpi.DpiScaleX, dpi.DpiScaleY)));
        // A size/DPI change rebuilds the ring; a hue change only rebuilds the triangle.
        if (_bitmap?.PixelWidth != pixels)
        {
            _bitmap = new WriteableBitmap(pixels, pixels, 96, 96, PixelFormats.Pbgra32, null);
            _pixels = new int[pixels * pixels];
            _ringPixels = new int[_pixels.Length];
            for (var y = 0; y < pixels; y++)
            for (var x = 0; x < pixels; x++)
            {
                var dx = (x + 0.5) / pixels - 0.5; var dy = (y + 0.5) / pixels - 0.5;
                var radius = Math.Sqrt(dx * dx + dy * dy);
                var coverage = Math.Clamp(Math.Min(radius - 0.41, 0.49 - radius) * pixels + 0.5, 0, 1);
                if (coverage > 0)
                    _ringPixels[y * pixels + x] = Pixel(ColourTriangle.PureHue(Math.Atan2(dy, dx) * 180 / Math.PI + 90), coverage);
            }
            _renderedHue = double.NaN;
        }
        if (_renderedHue != _hue)
        {
            Array.Copy(_ringPixels, _pixels, _pixels.Length);
            var hue = ColourTriangle.PureHue(_hue);
            var start = ColourTriangle.GetWeights(new(0.5 / pixels, 0.5 / pixels), _hue);
            var nextX = ColourTriangle.GetWeights(new(1.5 / pixels, 0.5 / pixels), _hue);
            var nextY = ColourTriangle.GetWeights(new(0.5 / pixels, 1.5 / pixels), _hue);
            for (var y = 0; y < pixels; y++)
            {
                var h = start.Hue + y * (nextY.Hue - start.Hue);
                var w = start.White + y * (nextY.White - start.White);
                var k = start.Black + y * (nextY.Black - start.Black);
                for (var x = 0; x < pixels; x++)
                {
                    var coverage = Math.Clamp(Math.Min(h, Math.Min(w, k)) * (1.5 * ColourTriangle.Radius) * pixels + 0.5, 0, 1);
                    if (coverage > 0) _pixels[y * pixels + x] = Pixel(ColourTriangle.Mix(hue, new(h, w, k)), coverage);
                    h += nextX.Hue - start.Hue; w += nextX.White - start.White; k += nextX.Black - start.Black;
                }
            }
            _bitmap!.WritePixels(new Int32Rect(0, 0, pixels, pixels), _pixels, pixels * 4, 0);
            _renderedHue = _hue;
        }
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        dc.DrawImage(_bitmap, new Rect(origin.X, origin.Y, size, size));
        DrawMarker(dc, ColourTriangle.Vertex(_hue, radius: 0.45), origin, size, 5);
        DrawMarker(dc, ColourTriangle.Position(_hue, _weights), origin, size, 4);
    }

    private static int Pixel(InputColor c, double coverage)
    {
        var a = (int)Math.Round(255 * coverage);
        return (a << 24) | ((c.Red * a / 255) << 16) | ((c.Green * a / 255) << 8) | (c.Blue * a / 255);
    }

    private static void DrawMarker(DrawingContext dc, ColourTriangle.Point p, Point origin, double size, double radius)
    {
        var center = new Point(origin.X + p.X * size, origin.Y + p.Y * size);
        dc.DrawEllipse(null, new Pen(Brushes.Black, 3), center, radius, radius);
        dc.DrawEllipse(null, new Pen(Brushes.White, 1.5), center, radius, radius);
    }

    private ColourTriangle.Point Normalized(Point p)
    {
        var size = Math.Min(ActualWidth, ActualHeight);
        return new((p.X - (ActualWidth - size) / 2) / size, (p.Y - (ActualHeight - size) / 2) / size);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (Math.Min(ActualWidth, ActualHeight) <= 0) return;
        var p = Normalized(e.GetPosition(this));
        var distance = Math.Sqrt(Math.Pow(p.X - 0.5, 2) + Math.Pow(p.Y - 0.5, 2));
        _dragHue = distance >= 0.41 && distance <= 0.49;
        if (!_dragHue && !ColourTriangle.Contains(ColourTriangle.GetWeights(p, _hue))) return;
        Focus(); CaptureMouse(); UpdatePointer(p); e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed) UpdatePointer(Normalized(e.GetPosition(this)));
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (IsMouseCaptured) { UpdatePointer(Normalized(e.GetPosition(this))); ReleaseMouseCapture(); e.Handled = true; }
    }

    private void UpdatePointer(ColourTriangle.Point point)
    {
        if (_dragHue) _hue = ColourTriangle.WrapHue(Math.Atan2(point.Y - 0.5, point.X - 0.5) * 180 / Math.PI + 90);
        else _weights = ColourTriangle.ClampToTriangle(point, _hue);
        Publish();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Left or Key.Right or Key.Up or Key.Down)) return;
        var step = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 0.025 : 0.005;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            _hue = ColourTriangle.WrapHue(_hue + (e.Key is Key.Left or Key.Down ? -1 : 1));
        else
        {
            var p = ColourTriangle.Position(_hue, _weights);
            _weights = ColourTriangle.ClampToTriangle(new(p.X + (e.Key == Key.Left ? -step : e.Key == Key.Right ? step : 0),
                p.Y + (e.Key == Key.Up ? -step : e.Key == Key.Down ? step : 0)), _hue);
        }
        Publish(); e.Handled = true;
    }

    private void Publish()
    {
        var c = ColourTriangle.Mix(ColourTriangle.PureHue(_hue), _weights, SelectedColor.A);
        _publishing = true;
        try { SetCurrentValue(SelectedColorProperty, Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue)); }
        finally { _publishing = false; }
        InvalidateVisual();
    }
}
