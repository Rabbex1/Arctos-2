using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arctos.Forms.Wpf;

public partial class BrushEditorWindow : Window
{
    public static readonly DependencyProperty EditorColorProperty = DependencyProperty.Register(nameof(EditorColor), typeof(Color), typeof(BrushEditorWindow),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, args) =>
        {
            var editor = (BrushEditorWindow)sender;
            if (!editor._ready || editor._refreshing) return;
            var c = (Color)args.NewValue; editor.Model.SetColor(new(c.R, c.G, c.B, c.A));
        }));
    public Color EditorColor { get => (Color)GetValue(EditorColorProperty); set => SetValue(EditorColorProperty, value); }
    public InputBrushVM Input { get; }
    public BrushEditingModel Model { get; }
    private bool _ready, _refreshing, _allowClose;
    public BrushEditorWindow(InputBrushVM input)
    {
        Input = input; Model = new(input);
        InitializeComponent();
        Resources["EditorSurface"] = Brush(input.EditorBackground);
        Resources["EditorText"] = Brush(input.EditorForeground);
        Preview.Model = Model; PalettePane.ItemsSource = Input.Palette;
        Model.Changed += Refresh; _ready = true; Refresh();
    }
    private static SolidColorBrush Brush(InputColor c) => new(Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue));
    private void Refresh()
    {
        if (!_ready) return;
        _refreshing = true;
        try
        {
            foreach (RadioButton tab in Tabs.Children) { tab.IsEnabled = Model.Allowed((string)tab.Tag); tab.IsChecked = (string)tab.Tag == Model.Mode; }
            var c = Model.Color; EditorColor = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
            EditingPane.Visibility = Model.Mode == "Palette" ? Visibility.Collapsed : Visibility.Visible;
            PalettePane.Visibility = Model.Mode == "Palette" ? Visibility.Visible : Visibility.Collapsed;
            GeometryPanel.Visibility = Model.IsGradient ? Visibility.Visible : Visibility.Collapsed;
            Grid.SetColumnSpan(Fields, Model.Mode == "Solid" ? 3 : 1);
            Fields.HorizontalAlignment = HorizontalAlignment.Center;
            Tools.Visibility = Model.Mode == "Sweep" ? Visibility.Collapsed : Visibility.Visible;
            OtherTool.Content = Model.Mode == "Radial" ? "Centre" : "Angle";
            PointsTool.IsChecked = Model.Tool == "Points"; OtherTool.IsChecked = Model.Tool != "Points";
            PointRow.Visibility = Model.Tool == "Points" || Model.Mode == "Sweep" ? Visibility.Visible : Visibility.Collapsed;
            CoordinateRow.Visibility = Model.Mode != "Sweep" && Model.Tool != "Angle" ? Visibility.Visible : Visibility.Collapsed;
            AngleRow.Visibility = Model.Mode == "Sweep" || Model.Tool == "Angle" ? Visibility.Visible : Visibility.Collapsed;
            ViewRangeRow.Visibility = Model.Mode == "Radial" ? Visibility.Visible : Visibility.Collapsed;
            ViewRangeSlider.Maximum = Model.MaximumViewRange;
            ViewRangeSlider.Value = Model.ViewRange;
            ViewRangeLabel.Text = Model.ViewRange.ToString("0.0'×'", CultureInfo.InvariantCulture);
            PointBox.ItemsSource = Model.Stops; PointBox.Items.Refresh(); PointBox.SelectedIndex = Model.Selected;
            RemoveButton.IsEnabled = Model.Stops.Count > 2;
            var p = Model.PointPosition; XBox.Text = p.X.ToString("0.###", CultureInfo.InvariantCulture); YBox.Text = p.Y.ToString("0.###", CultureInfo.InvariantCulture);
            AngleBox.Text = Model.Angle.ToString("0.##", CultureInfo.InvariantCulture);
            ErrorText.Text = Input.ValidationError; AcceptButton.IsEnabled = Input.CanCommit;
            ClearButton.Visibility = Input.AllowClear ? Visibility.Visible : Visibility.Collapsed;
        }
        finally { _refreshing = false; }
    }
    private void DragStripMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        e.Handled = true;
        DragMove();
    }
    private void ModeChanged(object sender, RoutedEventArgs e) { if (_ready && !_refreshing) Model.SwitchMode((string)((RadioButton)sender).Tag); }
    private void ToolChanged(object sender, RoutedEventArgs e) { if (_ready && !_refreshing) Model.SetTool(sender == PointsTool ? "Points" : Model.Mode == "Radial" ? "Centre" : "Angle"); }
    private void PointChanged(object sender, SelectionChangedEventArgs e) { if (_ready && !_refreshing) Model.Select(PointBox.SelectedIndex); }
    private void AddPoint(object sender, RoutedEventArgs e) => Model.Add();
    private void ViewRangeChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { if (_ready && !_refreshing) Model.ViewRange = e.NewValue; }
    private void RemovePoint(object sender, RoutedEventArgs e) => Model.Remove();
    private void CoordinateChanged(object sender, KeyboardFocusChangedEventArgs e) => SetCoordinate((TextBox)sender);
    private void CoordinateKey(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { SetCoordinate((TextBox)sender); e.Handled = true; } }
    private void SetCoordinate(TextBox box) { if (double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) Model.SetCoordinate(box == XBox, value); Refresh(); }
    private void AngleChanged(object sender, KeyboardFocusChangedEventArgs e) => SetAngle();
    private void AngleKey(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { SetAngle(); e.Handled = true; } }
    private void SetAngle() { if (double.TryParse(AngleBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) Model.Angle = value; Refresh(); }
    private void PaletteClicked(object sender, RoutedEventArgs e) { if (((FrameworkElement)sender).DataContext is InputBrushPaletteEntry entry) Model.SelectPalette(entry); }
    private void ClearClicked(object sender, RoutedEventArgs e) => Model.Clear();
    private void CancelClicked(object sender, RoutedEventArgs e) { Input.CancelEdit(); _allowClose = true; DialogResult = false; }
    private void AcceptClicked(object sender, RoutedEventArgs e) { if (Model.Accept()) { _allowClose = true; DialogResult = true; } }
    protected override void OnClosing(CancelEventArgs e) { if (!_allowClose) e.Cancel = true; base.OnClosing(e); }
    protected override void OnClosed(EventArgs e) { Model.Changed -= Refresh; Preview.Model = null; base.OnClosed(e); }
}
