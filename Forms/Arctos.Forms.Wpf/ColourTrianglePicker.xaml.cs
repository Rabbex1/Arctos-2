using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arctos.Forms.Wpf;

public partial class ColourTrianglePicker : UserControl
{
    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColourTrianglePicker),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (sender, _) => ((ColourTrianglePicker)sender).RefreshFields()));

    private bool _ready;
    public bool ShowWheel { get => Wheel.Visibility == Visibility.Visible; set => Wheel.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
    private string Format => (FormatBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "RGB";
    private void FormatChanged(object sender, SelectionChangedEventArgs e) => RefreshFields();
    private bool _refreshing;
    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    public ColourTrianglePicker()
    {
        InitializeComponent();
        _ready = true;
        RefreshFields();
    }

    private void RefreshFields()
    {
        if (!_ready) return;
        _refreshing = true;
        var c = SelectedColor;
        var values = ColourChannels.Read(new(c.R, c.G, c.B, c.A), Format);
        var labels = Format == "RGB" ? new[] { "R", "G", "B" } : Format == "HSV" ? new[] { "H°", "S%", "V%" } : new[] { "H°", "S%", "L%" };
        FirstLabel.Text = labels[0]; SecondLabel.Text = labels[1]; ThirdLabel.Text = labels[2];
        RedBox.Text = values[0].ToString("0.##", CultureInfo.InvariantCulture);
        GreenBox.Text = values[1].ToString("0.##", CultureInfo.InvariantCulture);
        BlueBox.Text = values[2].ToString("0.##", CultureInfo.InvariantCulture);
        HexBox.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
        AlphaSlider.Value = c.A;
        _refreshing = false;
    }

    private void AlphaChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_ready || _refreshing) return;
        var c = SelectedColor;
        SetCurrentValue(SelectedColorProperty, Color.FromArgb((byte)e.NewValue, c.R, c.G, c.B));
    }

    private void CommitChannels()
    {
        if (double.TryParse(RedBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
            && double.TryParse(GreenBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var g)
            && double.TryParse(BlueBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var b)
            && double.IsFinite(r) && double.IsFinite(g) && double.IsFinite(b))
        {
            var color = ColourChannels.Write(Format, r, g, b, SelectedColor.A);
            SetCurrentValue(SelectedColorProperty, Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));
        }
        RefreshFields();
    }

    private void CommitHex()
    {
        var text = HexBox.Text.Trim().TrimStart('#');
        if (text.Length is 6 or 8 && uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            if (text.Length == 6) value = (value << 8) | SelectedColor.A;
            SetCurrentValue(SelectedColorProperty, Color.FromArgb((byte)value, (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8)));
        }
        RefreshFields();
    }

    private void ChannelsChanged(object sender, KeyboardFocusChangedEventArgs e) => CommitChannels();
    private void HexChanged(object sender, KeyboardFocusChangedEventArgs e) => CommitHex();
    private void ChannelsKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { CommitChannels(); e.Handled = true; } }
    private void HexKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { CommitHex(); e.Handled = true; } }
}
