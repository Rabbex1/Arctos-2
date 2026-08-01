namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Represents the intrinsic size of a node definition in minor node-space units.
/// </summary>
public readonly record struct NodeSize(int Width, int Height)
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Creates the default intrinsic size for the specified primitive node shape.
    /// </summary>
    /// <param name="shape">The shape to create a default size for.</param>
    /// <returns>The default size for <paramref name="shape"/> expressed in minor node-space units.</returns>
    public static NodeSize CreateDefault(NodeShape shape)
    {
        return shape switch
        {
            NodeShape.RoundedRectangle => new NodeSize(24, 16),
            NodeShape.Rectangle => new NodeSize(24, 16),
            NodeShape.Diamond => new NodeSize(8, 8),
            NodeShape.Circle => new NodeSize(8, 8),
            NodeShape.Capsule => new NodeSize(24, 8),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Unsupported node shape."),
        };
    }

    #endregion Public Methods
}

