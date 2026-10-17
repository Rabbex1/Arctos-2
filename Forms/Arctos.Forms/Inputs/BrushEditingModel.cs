namespace Arctos.Forms;

/// <summary>Shared transaction, geometry and handle behaviour for both editor surfaces.</summary>
public sealed class BrushEditingModel
{
    public sealed class Stop(double offset, InputColor color)
    {
        public double Offset { get; set; } = offset;
        public InputColor Color { get; set; } = color;
        public string Label { get; set; } = "";
    }
    public readonly record struct Handle(string Id, double X, double Y, InputColor Color, bool Selected, bool Offscreen = false);
    private sealed class State
    {
        public double X1, Y1 = .5, X2 = 1, Y2 = .5;
        public List<Stop> Stops = [new(0, new(30, 144, 255)), new(1, new(199, 21, 133))];
        public int Selected;
    }
    public static readonly string[] Tabs = ["Solid", "Linear", "Radial", "Sweep", "Palette"];
    private readonly Dictionary<string, State> _states = new()
    {
        ["Linear"] = new(),
        ["Radial"] = new() { X1 = .5, Y1 = .5, X2 = .5, Y2 = .5 },
        ["Sweep"] = new() { X1 = .5, Y1 = .5, X2 = 0, Y2 = 360 }
    };
    private InputColor _solid = new(255, 255, 255);
    public InputBrushVM Input { get; }
    public string Mode { get; private set; } = "Solid";
    public string Tool { get; private set; } = "Points";
    public bool IsGradient => _states.ContainsKey(Mode);
    public bool IsClear { get; private set; }
    private State Current => _states[IsGradient ? Mode : "Linear"];
    public IReadOnlyList<Stop> Stops => Current.Stops;
    public int Selected => Current.Selected;
    public InputColor Color => IsGradient ? Stops[Selected].Color : _solid;
    public double X1 => Current.X1;
    public double Y1 => Current.Y1;
    public double X2 => Current.X2;
    public double Y2 => Current.Y2;
    public event Action? Changed;

    private double _viewRange = 1;
    public double MaximumViewRange => Input.Options.MaximumRadialViewRange;
    public double ViewRange
    {
        get => Math.Min(_viewRange, MaximumViewRange);
        set
        {
            if (!double.IsFinite(value)) return;
            var range = Math.Clamp(value, 1, MaximumViewRange);
            if (_viewRange == range) return;
            _viewRange = range;
            Changed?.Invoke(); // View-only: never rewrite the draft or brush geometry.
        }
    }
    public double PreviewScale => Mode == "Radial" ? ViewRange : 1;
    public (double X, double Y) ToPreview(double x, double y) => (.5 + (x - .5) / PreviewScale, .5 + (y - .5) / PreviewScale);
    public IReadOnlyList<Handle> PreviewHandles()
    {
        var handles = new List<Handle>();
        foreach (var h in Handles())
        {
            var p = ToPreview(h.X, h.Y);
            var dx = p.X - .5; var dy = p.Y - .5;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            if (Mode == "Radial" && distance > .5 + 1e-9)
            {
                if (h.Id == "centre") handles.Add(h with { X = .5 + .5 * dx / distance, Y = .5 + .5 * dy / distance, Offscreen = true });
                continue;
            }
            handles.Add(h with { X = p.X, Y = p.Y });
        }
        return handles;
    }
    public void MovePreviewHandle(string id, double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y)) return;
        if (Mode == "Radial")
        {
            var p = ClampCircle(x, y);
            x = .5 + (p.X - .5) * PreviewScale;
            y = .5 + (p.Y - .5) * PreviewScale;
        }
        MoveHandle(id, x, y);
    }

    public InputBrushDescription PreviewDescription()
    {
        var description = Description();
        if (description is not InputRadialGradientBrushDescription radial || Mode != "Radial") return description;
        var p = ToPreview(radial.CenterX, radial.CenterY);
        return radial with { CenterX = p.X, CenterY = p.Y, RadiusX = radial.RadiusX / PreviewScale, RadiusY = radial.RadiusY / PreviewScale };
    }

    public BrushEditingModel(InputBrushVM input)
    {
        Input = input;
        if (!input.IsEditing) input.BeginEdit();
        if (input.TryResolve(input.DraftValue, out var brush, out _))
        {
            switch (brush)
            {
                case InputSolidBrushDescription s: _solid = s.Color; break;
                case InputLinearGradientBrushDescription l: Load("Linear", l.X1, l.Y1, l.X2, l.Y2, l.Stops); break;
                case InputRadialGradientBrushDescription r: Load("Radial", r.CenterX, r.CenterY, r.RadiusX, r.RadiusY, r.Stops); break;
                case InputAngularGradientBrushDescription a: Load("Sweep", a.CenterX, a.CenterY, a.StartAngle, a.EndAngle, a.Stops); break;
            }
        }
        if (!Allowed(Mode)) Mode = Tabs.FirstOrDefault(Allowed) ?? "Solid";
        foreach (var state in _states.Values) Relabel(state);
    }
    private void Load(string mode, double a, double b, double c, double d, IReadOnlyList<InputGradientStop> stops)
    {
        Mode = mode;
        var s = _states[mode]; s.X1 = a; s.Y1 = b; s.X2 = c; s.Y2 = d;
        s.Stops = stops.Select(p => new Stop(p.Offset, p.Color)).ToList();
    }
    public bool Allowed(string mode) => Input.AllowedKinds.HasFlag(mode switch
    {
        "Solid" => InputBrushKinds.Solid, "Linear" => InputBrushKinds.Linear,
        "Radial" => InputBrushKinds.Radial, "Sweep" => InputBrushKinds.Angular, _ => InputBrushKinds.Named
    });
    public void SwitchMode(string mode)
    {
        if (Mode == mode || !Allowed(mode)) return;
        Mode = mode; Tool = "Points";
        if (mode == "Palette") Changed?.Invoke(); else Publish();
    }
    public void SetTool(string tool) { Tool = tool; Changed?.Invoke(); }
    public void Select(int index) { if (index >= 0 && index < Stops.Count) { Current.Selected = index; Changed?.Invoke(); } }
    public void SetColor(InputColor color)
    {
        if (IsGradient) Current.Stops[Selected].Color = color; else _solid = color;
        Publish();
    }
    public void Add()
    {
        var s = Current;
        var offset = (s.Stops[Selected].Offset + (Selected + 1 < s.Stops.Count ? s.Stops[Selected + 1].Offset : 1)) / 2;
        if (offset == s.Stops[Selected].Offset) offset = .5;
        var point = new Stop(offset, Color); s.Stops.Add(point);
        Sort(point); Publish();
    }
    public void Remove()
    {
        if (Stops.Count <= 2) return;
        Current.Stops.RemoveAt(Selected); Current.Selected = Math.Min(Selected, Stops.Count - 1);
        Relabel(Current); Publish();
    }
    private void Sort(Stop selected)
    {
        Current.Stops = Current.Stops.OrderBy(s => s.Offset).ToList();
        Current.Selected = Current.Stops.IndexOf(selected); Relabel(Current);
    }
    private static void Relabel(State s)
    {
        for (var i = 0; i < s.Stops.Count; i++) s.Stops[i].Label = $"Point {i + 1}";
    }
    public void SelectPalette(InputBrushPaletteEntry entry)
    {
        IsClear = false;
        Input.DraftValue = InputBrushFormat.Format(new InputNamedBrushDescription(entry.Name));
        Changed?.Invoke();
    }
    public void Clear() { IsClear = true; Input.DraftValue = ""; Changed?.Invoke(); }
    public bool Accept() => IsClear ? Input.CommitClear() : Input.CommitEdit();

    public InputBrushDescription Description()
    {
        if (Mode == "Palette" && Input.TryResolve(Input.DraftValue, out var palette, out _)) return palette!;
        if (!IsGradient) return new InputSolidBrushDescription(_solid);
        var s = Current; var stops = s.Stops.Select(p => new InputGradientStop(p.Offset, p.Color)).ToArray();
        return Mode switch
        {
            "Linear" => new InputLinearGradientBrushDescription(s.X1, s.Y1, s.X2, s.Y2, stops),
            "Radial" => new InputRadialGradientBrushDescription(s.X1, s.Y1, s.X2, s.Y2, stops),
            _ => new InputAngularGradientBrushDescription(s.X1, s.Y1, s.X2, s.Y2, stops)
        };
    }
    private void Publish()
    {
        IsClear = false;
        Input.DraftValue = InputBrushFormat.Format(Description()); Changed?.Invoke();
    }
    public double Angle
    {
        get => Mode == "Sweep" ? X2 + Stops[Selected].Offset * (Y2 - X2)
            : ColourTriangle.WrapHue(Math.Atan2(X2 - X1, Y1 - Y2) * 180 / Math.PI);
        set
        {
            if (!double.IsFinite(value)) return;
            if (Mode == "Sweep") SetOffset((Math.Clamp(value, X2, Y2) - X2) / (Y2 - X2));
            else { Rotate(value); Publish(); }
        }
    }
    public (double X, double Y) GuideEnd
    {
        get
        {
            if (Mode != "Radial") return (X2, Y2);
            var dx = .5 - X1; var dy = .5 - Y1; var length = Math.Sqrt(dx * dx + dy * dy);
            var ux = length < 1e-9 ? 1 : dx / length;
            var uy = length < 1e-9 ? 0 : dy / length;
            // The guide points across the brush, but ends on its actual gradient
            // ellipse, not on the unit-circle preview boundary.
            var radius = 1 / Math.Sqrt(Math.Pow(ux / X2, 2) + Math.Pow(uy / Y2, 2));
            return (X1 + ux * radius, Y1 + uy * radius);
        }
    }
    public (double X, double Y) PointPosition
    {
        get
        {
            if (Mode == "Radial" && Tool == "Centre") return (X1, Y1);
            var end = Mode == "Radial" ? GuideEnd : (X: X2, Y: Y2);
            var t = Stops[Selected].Offset;
            return (X1 + (end.X - X1) * t, Y1 + (end.Y - Y1) * t);
        }
    }
    public void SetCoordinate(bool xAxis, double value)
    {
        if (!double.IsFinite(value)) return;
        var p = PointPosition;
        MoveHandle(Mode == "Radial" && Tool == "Centre" ? "centre" : $"stop:{Selected}", xAxis ? value : p.X, xAxis ? p.Y : value);
    }
    private void SetOffset(double offset)
    {
        var selected = Current.Stops[Selected]; selected.Offset = Math.Clamp(offset, 0, 1);
        Sort(selected); Publish();
    }
    public IReadOnlyList<Handle> Handles()
    {
        if (!IsGradient) return [];
        if (Mode == "Radial" && Tool == "Centre") return [new("centre", X1, Y1, new(255, 255, 255), true)];
        if (Mode == "Linear" && Tool == "Angle") return [new("start", X1, Y1, Stops[0].Color, false), new("end", X2, Y2, Stops[^1].Color, false)];
        var handles = new List<Handle>();
        for (var i = 0; i < Stops.Count; i++)
        {
            double x, y;
            if (Mode == "Sweep")
            {
                var angle = (X2 + Stops[i].Offset * (Y2 - X2) - 90) * Math.PI / 180;
                // Separate the coincident 0/360 handles so both remain selectable.
                var radius = i == Stops.Count - 1 && Math.Abs(Y2 - X2 - 360) < .001 && Stops[i].Offset == 1 && Stops[0].Offset == 0 ? .43 : .48;
                x = .5 + radius * Math.Cos(angle); y = .5 + radius * Math.Sin(angle);
            }
            else
            {
                var end = Mode == "Radial" ? GuideEnd : (X: X2, Y: Y2);
                x = X1 + (end.X - X1) * Stops[i].Offset; y = Y1 + (end.Y - Y1) * Stops[i].Offset;
            }
            handles.Add(new($"stop:{i}", x, y, Stops[i].Color, i == Selected));
        }
        return handles;
    }
    public void MoveHandle(string id, double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y)) return;
        if (id.StartsWith("stop:") && int.TryParse(id[5..], out var index) && index < Stops.Count && index >= 0) Current.Selected = index;
        var p = Mode == "Linear" ? (X: Math.Clamp(x, 0, 1), Y: Math.Clamp(y, 0, 1)) : ClampCircle(x, y);
        if (Mode == "Linear" && Tool == "Angle")
        {
            var mx = (X1 + X2) / 2; var my = (Y1 + Y2) / 2;
            var angle = Math.Atan2(p.X - mx, my - p.Y) * 180 / Math.PI;
            Rotate(angle + (id == "start" ? 180 : 0)); Publish();
        }
        else if (Mode == "Radial" && id == "centre")
        {
            Current.X1 = x; Current.Y1 = y;
            var length = Math.Sqrt(Math.Pow(.5 - x, 2) + Math.Pow(.5 - y, 2)) + .5;
            Current.X2 = Current.Y2 = Math.Max(.001, length); Publish();
        }
        else if (Mode == "Sweep")
        {
            var angle = ColourTriangle.WrapHue(Math.Atan2(x - .5, .5 - y) * 180 / Math.PI);
            angle += 360 * Math.Round((Angle - angle) / 360);
            Angle = angle;
        }
        else if (Mode == "Linear" && (Stops[Selected].Offset == 0 || Stops[Selected].Offset == 1))
        {
            var start = Stops[Selected].Offset == 0;
            var ox = start ? X2 : X1; var oy = start ? Y2 : Y1;
            if (Math.Pow(p.X - ox, 2) + Math.Pow(p.Y - oy, 2) < .000001) return;
            if (start) { Current.X1 = p.X; Current.Y1 = p.Y; } else { Current.X2 = p.X; Current.Y2 = p.Y; }
            Publish();
        }
        else
        {
            var end = Mode == "Radial" ? GuideEnd : (X: X2, Y: Y2);
            var dx = end.X - X1; var dy = end.Y - Y1;
            var offset = ((x - X1) * dx + (y - Y1) * dy) / Math.Max(1e-12, dx * dx + dy * dy);
            if (Mode == "Radial" && offset > 1)
            {
                var radiusX = X2 * offset; var radiusY = Y2 * offset;
                if (!double.IsFinite(radiusX) || !double.IsFinite(radiusY)) return;
                var selected = Current.Stops[Selected];
                // Extend the gradient while preserving every other stop's physical
                // position (radius * offset), including when the first stop is dragged.
                foreach (var stop in Current.Stops) stop.Offset /= offset;
                Current.X2 = radiusX; Current.Y2 = radiusY;
                selected.Offset = 1;
                Sort(selected); Publish();
            }
            else SetOffset(offset);
        }
    }
    private void Rotate(double angle)
    {
        var mx = (X1 + X2) / 2; var my = (Y1 + Y2) / 2;
        var radians = angle * Math.PI / 180; var dx = Math.Sin(radians); var dy = -Math.Cos(radians);
        var half = Math.Sqrt(Math.Pow(X2 - X1, 2) + Math.Pow(Y2 - Y1, 2)) / 2;
        // Fit the rotated segment to the square, not its inscribed circle.
        var maxX = Math.Abs(dx) < 1e-12 ? double.PositiveInfinity : Math.Min(mx, 1 - mx) / Math.Abs(dx);
        var maxY = Math.Abs(dy) < 1e-12 ? double.PositiveInfinity : Math.Min(my, 1 - my) / Math.Abs(dy);
        var max = Math.Min(maxX, maxY);
        half = Math.Min(half, Math.Max(.001, max));
        Current.X1 = Math.Clamp(mx - dx * half, 0, 1); Current.Y1 = Math.Clamp(my - dy * half, 0, 1);
        Current.X2 = Math.Clamp(mx + dx * half, 0, 1); Current.Y2 = Math.Clamp(my + dy * half, 0, 1);
    }
    private static (double X, double Y) ClampCircle(double x, double y)
    {
        var dx = x - .5; var dy = y - .5; var length = Math.Sqrt(dx * dx + dy * dy);
        return length <= .5 ? (x, y) : (.5 + dx / length * .5, .5 + dy / length * .5);
    }
}
