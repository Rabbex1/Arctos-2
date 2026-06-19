using Arctos.Nodes.Models;
using SkiaSharp;

namespace Arctos.Nodes.Wpf.Visuals;

/// <summary>
/// Provides shared geometry helpers for port glyph rendering and hit testing.
/// </summary>
internal static class NodePortGeometry
{
    /// <summary>
    /// Creates the resolved glyph bounds for a port rendered inside the provided row bounds.
    /// </summary>
    public static SKRect CreatePortBounds(SKRect rowBounds, NodePortSide side, NodePortPlacement placement, float sizeNu, float nodeLeft, float nodeRight, float nodeBorderThickness, float nodeUnitSize, float nodeStrokeSize)
    {
        var size = MathF.Max(nodeUnitSize, sizeNu * nodeUnitSize);
        var centerX = GetPortCenterX(side, placement, size, nodeLeft, nodeRight, nodeBorderThickness, nodeStrokeSize);
        var centerY = rowBounds.MidY;
        return new SKRect(
            centerX - (size * 0.5f),
            centerY - (size * 0.5f),
            centerX + (size * 0.5f),
            centerY + (size * 0.5f));
    }

    /// <summary>
    /// Creates the triangle path used for arrow-shaped ports.
    /// </summary>
    public static SKPath CreateArrowPath(SKRect portBounds, NodePortSide side, bool pointsIntoNode)
    {
        var path = new SKPath();
        var pointsRight = (side == NodePortSide.Left && pointsIntoNode) || (side == NodePortSide.Right && !pointsIntoNode);

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

    private static float GetPortCenterX(NodePortSide side, NodePortPlacement placement, float size, float nodeLeft, float nodeRight, float nodeBorderThickness, float nodeStrokeSize)
    {
        var borderCenter = side == NodePortSide.Left ? nodeLeft : nodeRight;
        if (placement == NodePortPlacement.Over)
            return borderCenter;

        var halfStrokeWorld = (nodeBorderThickness * nodeStrokeSize) * 0.5f;
        var offset = halfStrokeWorld + (size * 0.5f);

        return (side, placement) switch
        {
            (NodePortSide.Left, NodePortPlacement.Inside) => borderCenter + offset,
            (NodePortSide.Left, NodePortPlacement.Outside) => borderCenter - offset,
            (NodePortSide.Right, NodePortPlacement.Inside) => borderCenter - offset,
            (NodePortSide.Right, NodePortPlacement.Outside) => borderCenter + offset,
            _ => borderCenter,
        };
    }
}
