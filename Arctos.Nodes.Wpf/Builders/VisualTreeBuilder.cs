using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Nodes.Wpf.Visuals;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Builders;

/// <summary>
/// Builds the visual tree for the current Skia host context.
/// </summary>
public sealed class VisualTreeBuilder
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Builds a visual tree for the provided host context.
    /// </summary>
    /// <param name="host">The current model host for the canvas.</param>
    /// <param name="detailedView">A value indicating whether the host is being shown in detailed view mode.</param>
    /// <param name="backgroundColor">The effective background color for the root canvas.</param>
    /// <returns>The built root visual for the current canvas scene.</returns>
    public CanvasVisual Build(IModelHost? host, bool detailedView, NodeColor backgroundColor)
    {
        var rootVisual = new CanvasVisual
        {
            BackgroundColor = backgroundColor,
        };

        if (host is NodeDefinitionVM definitionHost)
            rootVisual.AddChild(CreateNodeVisual(definitionHost));

        return rootVisual;
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private static NodeVisual CreateNodeVisual(NodeDefinitionVM definitionHost)
    {
        ArgumentNullException.ThrowIfNull(definitionHost);

        var shapeStyle = definitionHost.Model.Style?.ShapeStyle ?? new NodeShapeStyle();
        NodeVisual nodeVisual = definitionHost.NodeShape switch
        {
            NodeShape.RoundedRectangle => new RoundedRectangleNodeVisual(shapeStyle),
            NodeShape.Rectangle => new RectangleNodeVisual(shapeStyle),
            NodeShape.Diamond => new DiamondNodeVisual(shapeStyle),
            NodeShape.Circle => new CircleNodeVisual(shapeStyle),
            NodeShape.Capsule => new CapsuleNodeVisual(shapeStyle),
            _ => new RoundedRectangleNodeVisual(shapeStyle),
        };

        nodeVisual.Bounds = CreateOverviewBounds(definitionHost.NodeShape);
        return nodeVisual;
    }

    private static SKRect CreateOverviewBounds(NodeShape nodeShape)
    {
        return nodeShape switch
        {
            NodeShape.Rectangle or NodeShape.RoundedRectangle => new SKRect(-48.0f, -32.0f, 48.0f, 32.0f),
            NodeShape.Diamond or NodeShape.Circle => new SKRect(-16.0f, -16.0f, 16.0f, 16.0f),
            NodeShape.Capsule => new SKRect(-48.0f, -16.0f, 48.0f, 16.0f),
            _ => new SKRect(-48.0f, -32.0f, 48.0f, 32.0f),
        };
    }

    #endregion Private Methods
}
