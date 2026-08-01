using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Provides the common base for primitive node visuals.
/// </summary>
public abstract class NodeVisual : IVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly List<IVisual> _children = [];
    private readonly string? _fontFamily;
    private readonly NodeColor _fontColor;
    private readonly string? _headerText;
    private readonly float _headerFontSize;
    private readonly float _headerHeight;
    private readonly float _headerHorizontalPadding;
    private readonly NodeHeaderStyle? _headerStyle;
    private readonly float _nodeUnitSize;
    private readonly float _nodeStrokeSize;
    private readonly NodeShapeStyle _shapeStyle;
    private SKRect _bounds;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>Gets the child visuals for this visual node.</summary>
    public IReadOnlyList<IVisual> Children => _children;

    /// <summary>Gets or sets the current layout bounds for this visual node.</summary>
    public SKRect Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }

    /// <summary>Gets the shape style used when rendering this node visual.</summary>
    protected NodeShapeStyle ShapeStyle => _shapeStyle;

    /// <summary>Gets the current node stroke unit size in world-space units.</summary>
    protected float NodeStrokeSize => _nodeStrokeSize;

    /// <summary>Gets the current node unit size in world-space units.</summary>
    protected float NodeUnitSize => _nodeUnitSize;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new primitive node visual using the provided shape style.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    /// <param name="headerStyle">The style used to render the optional node header.</param>
    /// <param name="headerText">The resolved header text.</param>
    /// <param name="fontFamily">The font family used for node text when one is defined.</param>
    /// <param name="headerFontSize">The header font size used for node header text.</param>
    /// <param name="nodeUnitSize">The world-space size of <c>1nu</c> for the current document typography.</param>
    /// <param name="nodeStrokeSize">The world-space size of <c>1ns</c> for the current document typography.</param>
    protected NodeVisual(NodeShapeStyle shapeStyle, NodeHeaderStyle? headerStyle, string? headerText, string? fontFamily, float headerFontSize, NodeColor fontColor, float headerHorizontalPadding, float nodeUnitSize, float nodeStrokeSize, float headerHeight)
    {
        _shapeStyle = shapeStyle ?? throw new ArgumentNullException(nameof(shapeStyle));
        _headerStyle = headerStyle;
        _headerText = string.IsNullOrWhiteSpace(headerText) ? null : headerText;
        _fontFamily = fontFamily;
        _headerFontSize = headerFontSize > 0.0f ? headerFontSize : throw new ArgumentOutOfRangeException(nameof(headerFontSize));
        _fontColor = fontColor;
        _headerHorizontalPadding = MathF.Max(0.0f, headerHorizontalPadding);
        _nodeUnitSize = nodeUnitSize > 0.0f ? nodeUnitSize : throw new ArgumentOutOfRangeException(nameof(nodeUnitSize));
        _nodeStrokeSize = nodeStrokeSize > 0.0f ? nodeStrokeSize : throw new ArgumentOutOfRangeException(nameof(nodeStrokeSize));
        _headerHeight = MathF.Max(0.0f, headerHeight);
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Adds a child visual to this node visual.
    /// </summary>
    /// <param name="visual">The child visual to add.</param>
    public void AddChild(IVisual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        _children.Add(visual);
    }

    /// <summary>
    /// Removes all child visuals from this node visual.
    /// </summary>
    public void ClearChildren()
    {
        _children.Clear();
    }

    /// <summary>
    /// Renders this node visual to the provided Skia canvas.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public abstract void Render(SKCanvas canvas);

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    /// <summary>
    /// Renders all child visuals for this node visual.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    protected void RenderChildren(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        foreach (var child in _children)
            child.Render(canvas);
    }

    /// <summary>
    /// Creates a fill paint using the current node shape style.
    /// </summary>
    /// <returns>The fill paint.</returns>
    protected SKPaint CreateFillPaint()
    {
        return new SKPaint
        {
            Color = ShapeStyle.BackgroundColor.ToSkColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
    }

    /// <summary>
    /// Creates a stroke paint using the current node shape style.
    /// </summary>
    /// <returns>The stroke paint.</returns>
    protected SKPaint CreateStrokePaint()
    {
        return new SKPaint
        {
            Color = ShapeStyle.BorderColor.ToSkColor(),
            IsAntialias = true,
            StrokeWidth = ShapeStyle.BorderThickness * NodeStrokeSize,
            Style = SKPaintStyle.Stroke,
        };
    }

    /// <summary>
    /// Creates the fill paint used for the optional node header band.
    /// </summary>
    /// <returns>The header fill paint.</returns>
    protected SKPaint CreateHeaderFillPaint()
    {
        return new SKPaint
        {
            Color = (_headerStyle?.BackgroundColor ?? NodeColor.Transparent).ToSkColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
    }

    /// <summary>
    /// Creates the text paint used to render the optional node header text.
    /// </summary>
    /// <returns>The header text paint.</returns>
    protected SKPaint CreateHeaderTextPaint()
    {
        return new SKPaint
        {
            Color = _fontColor.ToSkColor(),
            IsAntialias = true,
            TextSize = _headerFontSize,
            TextAlign = GetTextAlignment(_headerStyle?.Alignment ?? NodeTextAlignment.Center),
            Typeface = CreateTypeface(_fontFamily, _headerStyle?.FontWeight ?? NodeFontWeight.Normal),
        };
    }

    /// <summary>
    /// Gets the current header band bounds inside the node body.
    /// </summary>
    /// <returns>The header bounds, or <see langword="null"/> when the node has no header.</returns>
    protected SKRect? GetHeaderBounds()
    {
        if (!HasHeader)
            return null;

        return new SKRect(Bounds.Left, Bounds.Top, Bounds.Right, Bounds.Top + _headerHeight);
    }

    /// <summary>
    /// Gets a value indicating whether the node currently has a renderable header.
    /// </summary>
    protected bool HasHeader => _headerStyle != null && !string.IsNullOrWhiteSpace(_headerText);

    /// <summary>
    /// Renders the current header text inside the provided header bounds.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    /// <param name="headerBounds">The header bounds.</param>
    protected void RenderHeaderText(SKCanvas canvas, SKRect headerBounds)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        if (!HasHeader || string.IsNullOrWhiteSpace(_headerText))
            return;

        using var textPaint = CreateHeaderTextPaint();
        var textBounds = SKRect.Empty;
        textPaint.MeasureText(_headerText, ref textBounds);
        var baseline = headerBounds.MidY - textBounds.MidY;
        var textX = (_headerStyle?.Alignment ?? NodeTextAlignment.Center) switch
        {
            NodeTextAlignment.Left => headerBounds.Left + _headerHorizontalPadding,
            NodeTextAlignment.Right => headerBounds.Right - _headerHorizontalPadding,
            _ => headerBounds.MidX,
        };

        canvas.DrawText(_headerText, textX, baseline, textPaint);
    }

    private static SKTextAlign GetTextAlignment(NodeTextAlignment alignment)
    {
        return alignment switch
        {
            NodeTextAlignment.Left => SKTextAlign.Left,
            NodeTextAlignment.Right => SKTextAlign.Right,
            _ => SKTextAlign.Center,
        };
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

    #endregion Protected Methods
}
