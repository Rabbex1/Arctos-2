using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arctos.Forms.Wpf;

public sealed class BrushEditorPreview : Grid
{
    private const double HandleInset = 9;
    private readonly BrushPreview _preview = new() { Margin = new Thickness(HandleInset) };
    private readonly HandleLayer _handles;
    private BrushEditingModel? _model;
    public BrushEditorPreview()
    {
        _handles = new HandleLayer(this);
        Children.Add(_preview); Children.Add(_handles);
        Unloaded += (_, _) => { if (_model is not null) _model.Changed -= Refresh; };
        Loaded += (_, _) => { if (_model is not null) { _model.Changed -= Refresh; _model.Changed += Refresh; Refresh(); } };
    }
    public BrushEditingModel? Model
    {
        get => _model;
        set { if (_model is not null) _model.Changed -= Refresh; _model = value; if (value is not null) value.Changed += Refresh; Refresh(); }
    }
    private void Refresh()
    {
        if (_model is null) return;
        _preview.Description = _model.IsClear ? null : InputBrushFormat.Format(_model.PreviewDescription());
        ToolTip = _model.Mode == "Radial" ? "View range changes only the editing view. An edge arrow means the centre is outside the view; increase the range or edit X/Y to reveal it." : null;
        _handles.InvalidateVisual();
    }
    private sealed class HandleLayer(BrushEditorPreview owner) : FrameworkElement
    {
        private string? _drag;
        private BrushEditingModel.Stop? _stop;
        private Point Map(double x, double y) => new(HandleInset + x * (ActualWidth - 2 * HandleInset), HandleInset + y * (ActualHeight - 2 * HandleInset));
        protected override void OnRender(DrawingContext dc)
        {
            var m = owner.Model;
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
            if (m is null || !m.IsGradient) return;
            if (m.Mode != "Sweep")
            {
                var end = m.Mode == "Radial" ? m.GuideEnd : (X: m.X2, Y: m.Y2);
                var startView = m.ToPreview(m.X1, m.Y1); var endView = m.ToPreview(end.X, end.Y);
                if (m.Mode == "Radial") dc.PushClip(new EllipseGeometry(Map(.5, .5), (ActualWidth - 2 * HandleInset) / 2, (ActualHeight - 2 * HandleInset) / 2));
                dc.DrawLine(new Pen(Brushes.Black, 2), Map(startView.X, startView.Y), Map(endView.X, endView.Y));
                dc.DrawLine(new Pen(Brushes.White, 1) { DashStyle = DashStyles.Dash }, Map(startView.X, startView.Y), Map(endView.X, endView.Y));
                if (m.Mode == "Radial") dc.Pop();
            }
            if (m.Mode == "Radial")
            {
                var rx = (ActualWidth - 2 * HandleInset) / (2 * m.PreviewScale);
                var ry = (ActualHeight - 2 * HandleInset) / (2 * m.PreviewScale);
                dc.DrawEllipse(null, new Pen(Brushes.Black, 3), Map(.5, .5), rx, ry);
                dc.DrawEllipse(null, new Pen(Brushes.White, 1), Map(.5, .5), rx, ry);
            }
            foreach (var h in m.PreviewHandles())
            {
                if (h.Offscreen)
                {
                    var tip = Map(h.X, h.Y); var direction = tip - Map(.5, .5); direction.Normalize();
                    var side = new Vector(-direction.Y, direction.X);
                    var arrow = new StreamGeometry();
                    using (var context = arrow.Open())
                    {
                        context.BeginFigure(tip, true, true);
                        context.LineTo(tip - direction * 15 + side * 7, true, false);
                        context.LineTo(tip - direction * 15 - side * 7, true, false);
                    }
                    dc.DrawGeometry(Brushes.White, new Pen(Brushes.Black, 2), arrow);
                    continue;
                }
                var radius = h.Selected ? 7 : 5;
                var fill = new SolidColorBrush(Color.FromArgb(h.Color.Alpha, h.Color.Red, h.Color.Green, h.Color.Blue));
                dc.DrawEllipse(fill, new Pen(Brushes.Black, 3), Map(h.X, h.Y), radius, radius);
                dc.DrawEllipse(null, new Pen(Brushes.White, 1), Map(h.X, h.Y), radius, radius);
            }
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var m = owner.Model; if (m is null) return;
            var p = e.GetPosition(this);
            var hits = m.PreviewHandles().Where(h => !h.Offscreen).OrderBy(h => (Map(h.X, h.Y) - p).LengthSquared).ToArray();
            if (hits.Length == 0 || (Map(hits[0].X, hits[0].Y) - p).Length > 13) return;
            _drag = hits[0].Id; _stop = null;
            if (_drag.StartsWith("stop:")) { var index = int.Parse(_drag[5..]); _stop = m.Stops[index]; m.Select(index); }
            CaptureMouse(); e.Handled = true;
        }
        private void Move(Point p)
        {
            var m = owner.Model; if (m is null || _drag is null) return;
            var id = _stop is null ? _drag : $"stop:{m.Stops.ToList().IndexOf(_stop)}";
            m.MovePreviewHandle(id, (p.X - HandleInset) / (ActualWidth - 2 * HandleInset), (p.Y - HandleInset) / (ActualHeight - 2 * HandleInset));
        }
        protected override void OnMouseMove(MouseEventArgs e) { if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed) Move(e.GetPosition(this)); }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) { if (IsMouseCaptured) { Move(e.GetPosition(this)); ReleaseMouseCapture(); _drag = null; } }
    }
}
