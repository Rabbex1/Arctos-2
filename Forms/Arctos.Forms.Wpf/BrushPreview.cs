using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Arctos.Forms.Wpf;

/// <summary>Renders portable Arctos brush descriptions, including cached bitmap sweep gradients.</summary>
public sealed class BrushPreview : FrameworkElement
{
    public static readonly DependencyProperty InputProperty = DependencyProperty.Register(
        nameof(Input), typeof(InputBrushVM), typeof(BrushPreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnInputChanged));

    public static readonly DependencyProperty UseDraftProperty = DependencyProperty.Register(
        nameof(UseDraft), typeof(bool), typeof(BrushPreview),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(BrushPreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    private WriteableBitmap? _angularBitmap;
    private int[] _pixels = [];
    private double[] _angles = [];
    private int _bitmapWidth;
    private int _bitmapHeight;
    private double _cachedCenterX = double.NaN;
    private double _cachedCenterY = double.NaN;

    public BrushPreview()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public InputBrushVM? Input
    {
        get => (InputBrushVM?)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public bool UseDraft
    {
        get => (bool)GetValue(UseDraftProperty);
        set => SetValue(UseDraftProperty, value);
    }

    /// <summary>Optional literal description used for standalone palette-entry previews.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) =>
        new(double.IsInfinity(availableSize.Width) ? 120 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 28 : availableSize.Height);

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) return;
        var bounds = new Rect((ActualWidth - size) / 2, (ActualHeight - size) / 2, size, size);
        InputBrushDescription? description = null;
        var parsed = Description is not null
            ? InputBrushFormat.TryParse(Description, out description, out _)
            : Input is not null && Input.TryResolve(UseDraft ? Input.DraftValue : Input.Value, out description, out _);
        var outline = CreateOutline(description?.Kind ?? InputBrushKind.Solid, bounds);
        drawingContext.PushClip(outline);
        DrawTransparencyGrid(drawingContext, bounds);
        if (!parsed)
        {
            drawingContext.Pop();
            drawingContext.DrawGeometry(null, new Pen(Brushes.IndianRed, 1), outline);
            return;
        }

        switch (description)
        {
            case InputSolidBrushDescription solid:
                drawingContext.DrawRectangle(ToBrush(solid.Color), null, bounds);
                break;
            case InputLinearGradientBrushDescription linear:
                drawingContext.DrawRectangle(CreateLinear(linear), null, bounds);
                break;
            case InputRadialGradientBrushDescription radial:
                drawingContext.DrawRectangle(CreateRadial(radial), null, bounds);
                break;
            case InputAngularGradientBrushDescription angular:
                DrawAngular(drawingContext, bounds, angular);
                break;
        }
        drawingContext.Pop();
        drawingContext.DrawGeometry(null, new Pen(SystemColors.ControlDarkBrush, 1), outline);
    }

    internal static Geometry CreateOutline(InputBrushKind kind, Rect bounds) => kind switch
    {
        InputBrushKind.Radial or InputBrushKind.Angular => new EllipseGeometry(bounds),
        InputBrushKind.Linear => new RectangleGeometry(bounds),
        _ => new RectangleGeometry(bounds, bounds.Width * .06, bounds.Height * .06)
    };

    private static void OnInputChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var preview = (BrushPreview)sender;
        if (args.OldValue is InputBrushVM oldInput)
            preview.Detach(oldInput);
        if (preview.IsLoaded && args.NewValue is InputBrushVM newInput)
            preview.Attach(newInput);
        preview.InvalidateVisual();
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        if (Input is not null)
            Attach(Input);
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        if (Input is not null)
            Detach(Input);
    }

    private void Attach(InputBrushVM input)
    {
        input.PropertyChanged -= OnInputPropertyChanged;
        input.Palette.CollectionChanged -= OnPaletteChanged;
        input.PropertyChanged += OnInputPropertyChanged;
        input.Palette.CollectionChanged += OnPaletteChanged;
    }

    private void Detach(InputBrushVM input)
    {
        input.PropertyChanged -= OnInputPropertyChanged;
        input.Palette.CollectionChanged -= OnPaletteChanged;
    }

    private void OnInputPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(InputBrushVM.Value) or nameof(InputBrushVM.DraftValue) or nameof(InputBrushVM.DisplayDescription))
            InvalidateVisual();
    }

    private void OnPaletteChanged(object? sender, NotifyCollectionChangedEventArgs args) => InvalidateVisual();

    private static void DrawTransparencyGrid(DrawingContext context, Rect bounds)
    {
        context.DrawRectangle(Brushes.White, null, bounds);
        const double size = 8;
        for (var y = 0d; y < bounds.Height; y += size)
        for (var x = 0d; x < bounds.Width; x += size)
            if (((int)(x / size) + (int)(y / size)) % 2 == 0)
                context.DrawRectangle(Brushes.LightGray, null, new Rect(bounds.X + x, bounds.Y + y, Math.Min(size, bounds.Width - x), Math.Min(size, bounds.Height - y)));
    }

    private static SolidColorBrush ToBrush(InputColor color) =>
        new(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));

    private static LinearGradientBrush CreateLinear(InputLinearGradientBrushDescription description)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(description.X1, description.Y1),
            EndPoint = new Point(description.X2, description.Y2),
            MappingMode = BrushMappingMode.RelativeToBoundingBox
        };
        AddStops(brush.GradientStops, description.Stops);
        brush.Freeze();
        return brush;
    }

    private static RadialGradientBrush CreateRadial(InputRadialGradientBrushDescription description)
    {
        var brush = new RadialGradientBrush
        {
            Center = new Point(description.CenterX, description.CenterY),
            GradientOrigin = new Point(description.CenterX, description.CenterY),
            RadiusX = description.RadiusX,
            RadiusY = description.RadiusY,
            MappingMode = BrushMappingMode.RelativeToBoundingBox
        };
        AddStops(brush.GradientStops, description.Stops);
        brush.Freeze();
        return brush;
    }

    private static void AddStops(GradientStopCollection target, IEnumerable<InputGradientStop> stops)
    {
        foreach (var stop in stops)
            target.Add(new GradientStop(Color.FromArgb(stop.Color.Alpha, stop.Color.Red, stop.Color.Green, stop.Color.Blue), stop.Offset));
    }

    private void DrawAngular(DrawingContext context, Rect bounds, InputAngularGradientBrushDescription description)
    {
        var width = Math.Clamp((int)Math.Ceiling(bounds.Width), 1, 256);
        var height = Math.Clamp((int)Math.Ceiling(bounds.Height), 1, 256);
        EnsureAngularStorage(width, height, description.CenterX, description.CenterY);
        var sweep = description.EndAngle - description.StartAngle;

        for (var index = 0; index < _pixels.Length; index++)
        {
            var relative = NormalizeAngle(_angles[index] - description.StartAngle);
            _pixels[index] = relative <= sweep
                ? Pack(Interpolate(description.Stops, relative / sweep))
                : 0;
        }

        _angularBitmap!.WritePixels(new Int32Rect(0, 0, width, height), _pixels, width * 4, 0);
        context.DrawImage(_angularBitmap, bounds);
    }

    private void EnsureAngularStorage(int width, int height, double centerX, double centerY)
    {
        if (_angularBitmap is null || width != _bitmapWidth || height != _bitmapHeight)
        {
            _bitmapWidth = width;
            _bitmapHeight = height;
            _pixels = new int[width * height];
            _angles = new double[width * height];
            _angularBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            _cachedCenterX = double.NaN;
        }
        if (_cachedCenterX == centerX && _cachedCenterY == centerY)
            return;

        var cx = centerX * width;
        var cy = centerY * height;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            _angles[(y * width) + x] = NormalizeAngle(Math.Atan2((x + 0.5) - cx, cy - (y + 0.5)) * 180 / Math.PI);
        _cachedCenterX = centerX;
        _cachedCenterY = centerY;
    }

    private static InputColor Interpolate(IReadOnlyList<InputGradientStop> stops, double offset)
    {
        if (offset <= stops[0].Offset)
            return stops[0].Color;
        for (var index = 1; index < stops.Count; index++)
        {
            if (offset > stops[index].Offset)
                continue;
            var previous = stops[index - 1];
            var next = stops[index];
            var span = next.Offset - previous.Offset;
            var amount = span <= 0 ? 1 : (offset - previous.Offset) / span;
            return new InputColor(
                Lerp(previous.Color.Red, next.Color.Red, amount),
                Lerp(previous.Color.Green, next.Color.Green, amount),
                Lerp(previous.Color.Blue, next.Color.Blue, amount),
                Lerp(previous.Color.Alpha, next.Color.Alpha, amount));
        }
        return stops[^1].Color;
    }

    private static byte Lerp(byte first, byte second, double amount) =>
        (byte)Math.Clamp(Math.Round(first + ((second - first) * amount)), 0, 255);

    private static int Pack(InputColor color) => color.Blue | (color.Green << 8) | (color.Red << 16) | (color.Alpha << 24);
    private static double NormalizeAngle(double angle) => ((angle % 360) + 360) % 360;
}
