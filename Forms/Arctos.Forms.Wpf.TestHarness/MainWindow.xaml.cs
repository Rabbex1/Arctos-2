using Arctos.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Arctos.Forms.Wpf.TestHarness;

public partial class MainWindow : Window
{
    private bool _darkPalette;
    private readonly BrushInputOptions _brushOptions = new() { MaximumRadialViewRange = 4 };

    private void ThemeChanged(object sender, RoutedEventArgs args)
    {
        var dark = (sender as System.Windows.Controls.CheckBox)?.IsChecked == true;
        var background = dark ? new InputColor(38, 38, 42) : new InputColor(247, 246, 242);
        var foreground = dark ? new InputColor(240, 240, 240) : new InputColor(32, 32, 32);
        Background = new SolidColorBrush(Color.FromRgb(background.Red, background.Green, background.Blue));
        Foreground = new SolidColorBrush(Color.FromRgb(foreground.Red, foreground.Green, foreground.Blue));
        if (Inputs is null) return;
        foreach (var input in Inputs) { input.EditorBackground = background; input.EditorForeground = foreground; }
    }

    public MainWindow()
    {
        Inputs =
        [
            Create("Named palette brush", "@Surface", InputBrushKinds.All),
            Create("Solid-only property", "@AccentSolid", InputBrushKinds.Named | InputBrushKinds.Solid),
            Create("Linear gradient", "linear(0,0.5,1,0.5;0:#1E90FFFF;1:#C71585FF)", InputBrushKinds.All),
            Create("Radial gradient", "radial(0.5,0.5,0.5,0.5;0:#FFFFFFFF;1:#4169E100)", InputBrushKinds.All),
            Create("Angular gradient", "angular(0.5,0.5,0,360;0:#FF0000FF;0.333333:#00FF00FF;0.666667:#0000FFFF;1:#FF0000FF)", InputBrushKinds.All),
            Create("Clearable / inherited", "@Surface", InputBrushKinds.All, allowClear: true)
        ];
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<InputBrushVM> Inputs { get; }
    public ObservableCollection<string> Events { get; } = [];

    private InputBrushVM Create(string label, string value, InputBrushKinds kinds, bool allowClear = false)
    {
        InputBrushVM? input = null;
        input = new InputBrushVM(label, value, CreatePalette(false), kinds,
            changed => Events.Insert(0, $"{DateTime.Now:HH:mm:ss.fff}  {label}: {changed}"), allowClear, _brushOptions);
        return input;
    }

    private static InputBrushPaletteEntry[] CreatePalette(bool dark) =>
    [
        new("Surface", dark ? "#20242BFF" : "#F7F8FAFF"),
        new("AccentSolid", dark ? "#8AB4F8FF" : "#2457D6FF"),
        new("Hero", dark
            ? "linear(0,0,1,1;0:#11243DFF;0.5:#71436FFF;1:#C06C47FF)"
            : "linear(0,0,1,1;0:#DFF3FFFF;0.5:#FCE1FFFF;1:#FFE8CFFF)"),
        new("Spotlight", "radial(0.35,0.3,0.65,0.7;0:#FFFFFFFF;1:#4169E100)"),
        new("Spectrum", "angular(0.5,0.5,0,360;0:#FF0000FF;0.333333:#00FF00FF;0.666667:#0000FFFF;1:#FF0000FF)")
    ];

    private void Edit_Click(object sender, RoutedEventArgs args)
    {
        if ((sender as FrameworkElement)?.DataContext is not InputBrushVM input)
            return;
        new BrushEditorWindow(input) { Owner = this }.ShowDialog();
    }

    private void RefreshPalette_Click(object sender, RoutedEventArgs args)
    {
        _darkPalette = !_darkPalette;
        foreach (var input in Inputs)
            input.ReplacePalette(CreatePalette(_darkPalette));
    }

    private void InvalidDraft_Click(object sender, RoutedEventArgs args)
    {
        var input = Inputs[0];
        input.BeginEdit();
        input.DraftValue = "@MissingBrush";
        new BrushEditorWindow(input) { Owner = this }.ShowDialog();
    }

    private void Benchmark_Click(object sender, RoutedEventArgs args)
    {
        const int frames = 120;
        var input = Inputs[4];
        input.BeginEdit();
        var preview = new BrushPreview { Input = input, UseDraft = true, Width = 256, Height = 256 };
        preview.Measure(new Size(256, 256));
        preview.Arrange(new Rect(0, 0, 256, 256));
        var target = new RenderTargetBitmap(256, 256, 96, 96, PixelFormats.Pbgra32);
        var stopwatch = Stopwatch.StartNew();
        for (var frame = 0; frame < frames; frame++)
        {
            var start = frame * 3d;
            input.DraftValue = $"angular(0.5,0.5,{start},{start + 360};0:#FF0000FF;0.333333:#00FF00FF;0.666667:#0000FFFF;1:#FF0000FF)";
            preview.InvalidateVisual();
            target.Render(preview);
        }
        stopwatch.Stop();
        input.CancelEdit();
        var framesPerSecond = frames / stopwatch.Elapsed.TotalSeconds;
        Events.Insert(0, $"BENCHMARK  {frames} frames in {stopwatch.Elapsed.TotalMilliseconds:0} ms ({framesPerSecond:0.0} fps)");
    }
}
