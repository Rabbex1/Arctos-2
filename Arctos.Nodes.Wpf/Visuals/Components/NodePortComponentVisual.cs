using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Renders a passive port component inside a node definition.
/// </summary>
public sealed class NodePortComponentVisual : NodeComponentVisual
{
    private readonly string? _fontFamily;
    private readonly NodeFontWeight _fontWeight;
    private readonly bool _isHovered;
    private readonly bool _isSelected;
    private readonly string _label;
    private readonly float _nodeLeft;
    private readonly float _nodeRight;
    private readonly float _nodeUnitSize;
    private readonly float _nodeStrokeSize;
    private readonly float _nodeBorderThickness;
    private readonly NodePortPlacement _placement;
    private readonly NodePortShape _shape;
    private readonly NodePortSide _side;
    private readonly NodeColor _fillColor;
    private readonly NodeColor _borderColor;
    private readonly SKColor _selectionColor;
    private readonly float _selectionDashLength;
    private readonly bool _showDebugBounds;
    private readonly float _sizeNu;
    private readonly NodeColor _textColor;
    private readonly float _textSize;
    private readonly float _opacity;

    public NodePortComponentVisual(
        string label,
        NodePortSide side,
        NodePortShape shape,
        NodePortPlacement placement,
        float sizeNu,
        string? fontFamily,
        float textSize,
        NodeColor textColor,
        NodeFontWeight fontWeight,
        NodeColor fillColor,
        NodeColor borderColor,
        float nodeBorderThickness,
        float nodeUnitSize,
        float nodeStrokeSize,
        float nodeLeft,
        float nodeRight,
        float opacity = 1.0f,
        bool showDebugBounds = false,
        bool isHovered = false,
        bool isSelected = false,
        SKColor defaultSelectionColor = default,
        float selectionDashLength = 1.0f)
    {
        _label = label ?? string.Empty;
        _side = side;
        _shape = shape;
        _placement = placement;
        _sizeNu = sizeNu;
        _fontFamily = fontFamily;
        _textSize = textSize;
        _textColor = textColor;
        _fontWeight = fontWeight;
        _fillColor = fillColor;
        _borderColor = borderColor;
        _nodeBorderThickness = MathF.Max(0.0f, nodeBorderThickness);
        _nodeUnitSize = nodeUnitSize;
        _nodeStrokeSize = nodeStrokeSize;
        _nodeLeft = nodeLeft;
        _nodeRight = nodeRight;
        _opacity = Math.Clamp(opacity, 0.0f, 1.0f);
        _showDebugBounds = showDebugBounds;
        _isHovered = isHovered;
        _isSelected = isSelected;
        _selectionColor = defaultSelectionColor == default ? SKColors.Gray : defaultSelectionColor;
        _selectionDashLength = selectionDashLength;
    }

    public override void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        var size = MathF.Max(_nodeUnitSize, _sizeNu * _nodeUnitSize);
        var centerX = GetPortCenterX(size);
        var centerY = Bounds.MidY;
        var portBounds = new SKRect(
            centerX - (size * 0.5f),
            centerY - (size * 0.5f),
            centerX + (size * 0.5f),
            centerY + (size * 0.5f));

        DrawPort(canvas, portBounds);
        DrawLabel(canvas, portBounds);

        if (_showDebugBounds)
            RenderDebugBounds(canvas);

        if (_isHovered && !_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, true);

        if (_isSelected)
            RenderSelectionBounds(canvas, _selectionColor, _selectionDashLength, 0.0f, false);
    }

    private void DrawPort(SKCanvas canvas, SKRect portBounds)
    {
        using var strokePaint = new SKPaint
        {
            Color = WithOpacity(_borderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _nodeStrokeSize,
        };
        using var fillPaint = new SKPaint
        {
            Color = WithOpacity(_fillColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        switch (_shape)
        {
            case NodePortShape.Circle:
                canvas.DrawOval(portBounds, fillPaint);
                canvas.DrawOval(portBounds, strokePaint);
                break;

            case NodePortShape.Square:
                canvas.DrawRect(portBounds, fillPaint);
                canvas.DrawRect(portBounds, strokePaint);
                break;

            case NodePortShape.ArrowIn:
                using (var path = CreateArrowPath(portBounds, pointsIntoNode: true))
                {
                    canvas.DrawPath(path, fillPaint);
                    canvas.DrawPath(path, strokePaint);
                }
                break;

            case NodePortShape.ArrowOut:
                using (var path = CreateArrowPath(portBounds, pointsIntoNode: false))
                {
                    canvas.DrawPath(path, fillPaint);
                    canvas.DrawPath(path, strokePaint);
                }
                break;

            case NodePortShape.Hidden:
                DrawHiddenMarker(canvas, portBounds);
                break;
        }
    }

    private void DrawHiddenMarker(SKCanvas canvas, SKRect portBounds)
    {
        using var hiddenPaint = new SKPaint
        {
            Color = WithOpacity(_borderColor.ToSkColor()),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.0f,
        };

        canvas.DrawLine(portBounds.Left, portBounds.Top, portBounds.Right, portBounds.Bottom, hiddenPaint);
        canvas.DrawLine(portBounds.Left, portBounds.Bottom, portBounds.Right, portBounds.Top, hiddenPaint);
    }

    private SKPath CreateArrowPath(SKRect portBounds, bool pointsIntoNode)
    {
        var path = new SKPath();
        var pointsRight = (_side == NodePortSide.Left && pointsIntoNode) || (_side == NodePortSide.Right && !pointsIntoNode);

        if (pointsRight)
        {
            path.MoveTo(portBounds.Right, portBounds.MidY);
            path.LineTo(portBounds.Left, portBounds.Top);
            path.LineTo(portBounds.Left, portBounds.Bottom);
        }
        else
        {
            path.MoveTo(portBounds.Left, portBounds.MidY);
            path.LineTo(portBounds.Right, portBounds.Top);
            path.LineTo(portBounds.Right, portBounds.Bottom);
        }

        path.Close();
        return path;
    }

    private void DrawLabel(SKCanvas canvas, SKRect portBounds)
    {
        if (string.IsNullOrWhiteSpace(_label))
            return;

        using var textPaint = new SKPaint
        {
            Color = WithOpacity(_textColor.ToSkColor()),
            IsAntialias = true,
            TextSize = _textSize,
            TextAlign = _side == NodePortSide.Left ? SKTextAlign.Left : SKTextAlign.Right,
            Typeface = CreateTypeface(_fontFamily, _fontWeight),
        };

        var textBounds = SKRect.Empty;
        textPaint.MeasureText(_label, ref textBounds);
        var gap = _nodeUnitSize;
        var baseline = Bounds.MidY - textBounds.MidY;
        var x = _side == NodePortSide.Left
            ? MathF.Max(Bounds.Left, portBounds.Right + gap)
            : MathF.Min(Bounds.Right, portBounds.Left - gap);

        canvas.DrawText(_label, x, baseline, textPaint);
    }

    private float GetPortCenterX(float size)
    {
        var borderCenter = _side == NodePortSide.Left ? _nodeLeft : _nodeRight;
        if (_placement == NodePortPlacement.Over)
            return borderCenter;

        // Border thickness is authored in ns, so half of the resolved stroke width aligns on nf steps.
        var halfStrokeWorld = (_nodeBorderThickness * _nodeStrokeSize) * 0.5f;
        var offset = halfStrokeWorld + (size * 0.5f);

        return (_side, _placement) switch
        {
            (NodePortSide.Left, NodePortPlacement.Inside) => borderCenter + offset,
            (NodePortSide.Left, NodePortPlacement.Outside) => borderCenter - offset,
            (NodePortSide.Right, NodePortPlacement.Inside) => borderCenter - offset,
            (NodePortSide.Right, NodePortPlacement.Outside) => borderCenter + offset,
            _ => borderCenter,
        };
    }

    private SKColor WithOpacity(SKColor color)
    {
        return color.WithAlpha((byte)Math.Clamp(MathF.Round(color.Alpha * _opacity), 0.0f, 255.0f));
    }

    private static SKTypeface? CreateTypeface(string? fontFamily, NodeFontWeight fontWeight)
    {
        var skiaWeight = fontWeight switch
        {
            NodeFontWeight.Light => SKFontStyleWeight.Light,
            NodeFontWeight.Medium => SKFontStyleWeight.Medium,
            NodeFontWeight.SemiBold => SKFontStyleWeight.SemiBold,
            NodeFontWeight.Bold => SKFontStyleWeight.Bold,
            _ => SKFontStyleWeight.Normal,
        };

        return string.IsNullOrWhiteSpace(fontFamily)
            ? SKTypeface.FromFamilyName(null, skiaWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            : SKTypeface.FromFamilyName(fontFamily, skiaWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    }
}
