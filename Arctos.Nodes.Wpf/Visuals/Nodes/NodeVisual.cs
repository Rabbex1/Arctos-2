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

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new primitive node visual using the provided shape style.
    /// </summary>
    /// <param name="shapeStyle">The style used to render the node body.</param>
    protected NodeVisual(NodeShapeStyle shapeStyle)
    {
        _shapeStyle = shapeStyle ?? throw new ArgumentNullException(nameof(shapeStyle));
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
            StrokeWidth = ShapeStyle.BorderThickness,
            Style = SKPaintStyle.Stroke,
        };
    }

    #endregion Protected Methods
}
