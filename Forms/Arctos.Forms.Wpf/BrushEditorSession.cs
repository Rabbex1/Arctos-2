using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media;

namespace Arctos.Forms.Wpf;

public sealed class BrushStopEditor : ObservableObject
{
    private double _offset;
    private Color _color;

    public BrushStopEditor(double offset, Color color)
    {
        _offset = offset;
        _color = color;
    }

    public double Offset { get => _offset; set => SetProperty(ref _offset, Math.Clamp(value, 0, 1)); }
    public Color Color
    {
        get => _color;
        set
        {
            if (SetProperty(ref _color, value))
                OnPropertyChanged(nameof(Hex));
        }
    }
    public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}{Color.A:X2}";
}

public sealed class BrushEditorSession : ObservableObject, IDisposable
{
    private readonly InputBrushVM _input;
    private string _mode = "Linear";
    private Color _solidColor = Colors.White;
    private BrushStopEditor? _selectedStop;
    private double _geometry1;
    private double _geometry2;
    private double _geometry3 = 1;
    private double _geometry4 = 1;
    private bool _loading;

    public BrushEditorSession(InputBrushVM input)
    {
        _input = input;
        Modes = GetModes(input.AllowedKinds).ToArray();
        _mode = Modes.FirstOrDefault() ?? "Linear";
        Stops.CollectionChanged += OnStopsChanged;
        Load(input.DraftValue);
        InitializeGradientDefaults();
        input.PropertyChanged += OnInputPropertyChanged;
    }

    public IReadOnlyList<string> Modes { get; }
    public ObservableCollection<BrushStopEditor> Stops { get; } = [];
    public string Error => _input.ValidationError;
    public bool CanAccept => _input.CanCommit;

    public string Mode
    {
        get => _mode;
        set
        {
            if (!SetProperty(ref _mode, value) || _loading)
                return;
            ApplyModeDefaults();
            Rebuild();
        }
    }

    public Color SolidColor
    {
        get => _solidColor;
        set
        {
            if (SetProperty(ref _solidColor, value) && !_loading)
                _input.DraftValue = InputBrushFormat.Format(new InputSolidBrushDescription(ToInput(value)));
        }
    }

    public BrushStopEditor? SelectedStop
    {
        get => _selectedStop;
        set => SetProperty(ref _selectedStop, value);
    }

    public double Geometry1 { get => _geometry1; set { if (SetProperty(ref _geometry1, value) && !_loading) Rebuild(); } }
    public double Geometry2 { get => _geometry2; set { if (SetProperty(ref _geometry2, value) && !_loading) Rebuild(); } }
    public double Geometry3 { get => _geometry3; set { if (SetProperty(ref _geometry3, value) && !_loading) Rebuild(); } }
    public double Geometry4 { get => _geometry4; set { if (SetProperty(ref _geometry4, value) && !_loading) Rebuild(); } }

    public void SelectPalette(InputBrushPaletteEntry entry) => _input.DraftValue = InputBrushFormat.Format(new InputNamedBrushDescription(entry.Name));

    public void AddStop()
    {
        var stop = new BrushStopEditor(0.5, Colors.White);
        Stops.Add(stop);
        SelectedStop = stop;
        Rebuild();
    }

    public void RemoveSelectedStop()
    {
        if (SelectedStop is null || Stops.Count <= 2)
            return;
        Stops.Remove(SelectedStop);
        SelectedStop = Stops.FirstOrDefault();
        Rebuild();
    }

    public void Dispose()
    {
        _input.PropertyChanged -= OnInputPropertyChanged;
        Stops.CollectionChanged -= OnStopsChanged;
        foreach (var stop in Stops)
            stop.PropertyChanged -= OnStopChanged;
    }

    private void Load(string value)
    {
        if (!_input.TryResolve(value, out var brush, out _))
            return;
        _loading = true;
        try
        {
            Stops.Clear();
            switch (brush)
            {
                case InputSolidBrushDescription solid:
                    SolidColor = ToMedia(solid.Color);
                    break;
                case InputLinearGradientBrushDescription linear:
                    Mode = "Linear";
                    SetGeometry(linear.X1, linear.Y1, linear.X2, linear.Y2);
                    LoadStops(linear.Stops);
                    break;
                case InputRadialGradientBrushDescription radial:
                    Mode = "Radial";
                    SetGeometry(radial.CenterX, radial.CenterY, radial.RadiusX, radial.RadiusY);
                    LoadStops(radial.Stops);
                    break;
                case InputAngularGradientBrushDescription angular:
                    Mode = "Angular";
                    SetGeometry(angular.CenterX, angular.CenterY, angular.StartAngle, angular.EndAngle);
                    LoadStops(angular.Stops);
                    break;
            }
        }
        finally
        {
            _loading = false;
        }
    }

    private void SetGeometry(double first, double second, double third, double fourth)
    {
        Geometry1 = first; Geometry2 = second; Geometry3 = third; Geometry4 = fourth;
    }

    private void LoadStops(IEnumerable<InputGradientStop> stops)
    {
        foreach (var stop in stops)
            Stops.Add(new BrushStopEditor(stop.Offset, ToMedia(stop.Color)));
        SelectedStop = Stops.FirstOrDefault();
    }

    private void ApplyModeDefaults()
    {
        if (Mode == "Solid")
            return;
        if (Stops.Count < 2)
        {
            Stops.Clear();
            Stops.Add(new BrushStopEditor(0, Colors.DodgerBlue));
            Stops.Add(new BrushStopEditor(1, Colors.MediumVioletRed));
            SelectedStop = Stops[0];
        }
        SetGeometry(Mode == "Linear" ? 0 : 0.5, Mode == "Linear" ? 0.5 : 0.5, Mode == "Angular" ? 0 : (Mode == "Linear" ? 1 : 0.5), Mode == "Angular" ? 360 : (Mode == "Linear" ? 0.5 : 0.5));
    }

    private void InitializeGradientDefaults()
    {
        if (Stops.Count >= 2 || Modes.Count == 0)
            return;
        _loading = true;
        try
        {
            Stops.Add(new BrushStopEditor(0, Colors.DodgerBlue));
            Stops.Add(new BrushStopEditor(1, Colors.MediumVioletRed));
            SelectedStop = Stops[0];
            if (Mode == "Linear") SetGeometry(0, 0.5, 1, 0.5);
            else if (Mode == "Radial") SetGeometry(0.5, 0.5, 0.5, 0.5);
            else SetGeometry(0.5, 0.5, 0, 360);
        }
        finally
        {
            _loading = false;
        }
    }

    private void Rebuild()
    {
        if (_loading)
            return;
        var stops = Stops.OrderBy(stop => stop.Offset).Select(stop => new InputGradientStop(stop.Offset, ToInput(stop.Color))).ToArray();
        InputBrushDescription brush = Mode switch
        {
            "Linear" => new InputLinearGradientBrushDescription(Geometry1, Geometry2, Geometry3, Geometry4, stops),
            "Radial" => new InputRadialGradientBrushDescription(Geometry1, Geometry2, Geometry3, Geometry4, stops),
            "Angular" => new InputAngularGradientBrushDescription(Geometry1, Geometry2, Geometry3, Geometry4, stops),
            _ => new InputSolidBrushDescription(ToInput(SolidColor))
        };
        try { _input.DraftValue = InputBrushFormat.Format(brush); }
        catch (FormatException exception) { _input.DraftValue = $"invalid({exception.Message})"; }
    }

    private void OnStopsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
            foreach (BrushStopEditor stop in args.OldItems) stop.PropertyChanged -= OnStopChanged;
        if (args.NewItems is not null)
            foreach (BrushStopEditor stop in args.NewItems) stop.PropertyChanged += OnStopChanged;
        if (!_loading) Rebuild();
    }

    private void OnStopChanged(object? sender, PropertyChangedEventArgs args)
    {
        Rebuild();
    }

    private void OnInputPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(InputBrushVM.ValidationError)) OnPropertyChanged(nameof(Error));
        if (args.PropertyName is nameof(InputBrushVM.ValidationError) or nameof(InputBrushVM.IsEnabled) or nameof(InputBrushVM.IsReadOnly)) OnPropertyChanged(nameof(CanAccept));
    }

    private static IEnumerable<string> GetModes(InputBrushKinds kinds)
    {
        if (kinds.HasFlag(InputBrushKinds.Linear)) yield return "Linear";
        if (kinds.HasFlag(InputBrushKinds.Radial)) yield return "Radial";
        if (kinds.HasFlag(InputBrushKinds.Angular)) yield return "Angular";
    }

    private static Color ToMedia(InputColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    private static InputColor ToInput(Color color) => new(color.R, color.G, color.B, color.A);
}
