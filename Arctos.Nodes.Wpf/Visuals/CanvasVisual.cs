using Arctos.Nodes;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Root visual node for a rendered canvas scene.
/// </summary>
public sealed class CanvasVisual : IVisual
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly List<IVisual> _children = [];
    private SKRect _bounds;
    private NodeColor _backgroundColor = NodeColor.Transparent;

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

    /// <summary>Gets or sets the background color drawn by this canvas root.</summary>
    public NodeColor BackgroundColor
    {
        get => _backgroundColor;
        set => _backgroundColor = value;
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Adds a child visual to the canvas root.
    /// </summary>
    /// <param name="visual">The child visual to add.</param>
    public void AddChild(IVisual visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        _children.Add(visual);
    }

    /// <summary>
    /// Removes all child visuals from the canvas root.
    /// </summary>
    public void Clear()
    {
        _children.Clear();
    }

    /// <summary>
    /// Renders the canvas visual and all child visuals.
    /// </summary>
    /// <param name="canvas">The canvas to render to.</param>
    public void Render(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        canvas.Clear(BackgroundColor.ToSkColor());

        foreach (var child in _children)
            child.Render(canvas);
    }

    #endregion Public Methods
}
