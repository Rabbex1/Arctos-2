namespace Arctos.Nodes.Models;

/// <summary>
/// Defines how textual content, ports, and internal components may be used by a specific <see cref="NodeShape"/>.
/// </summary>
public sealed class ShapeRules
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the shape these rules apply to.
    /// </summary>
    public NodeShape Shape { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of ports allowed on the left side of the shape.
    /// </summary>
    public int MaxLeftPorts { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of ports allowed on the right side of the shape.
    /// </summary>
    public int MaxRightPorts { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of ports allowed on the top side of the shape.
    /// </summary>
    public int MaxTopPorts { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of ports allowed on the bottom side of the shape.
    /// </summary>
    public int MaxBottomPorts { get; set; }

    /// <summary>
    /// Gets or sets where the node header label may be displayed.
    /// </summary>
    public LabelPosition HeaderPosition { get; set; }

    /// <summary>
    /// Gets or sets where port labels may be displayed.
    /// </summary>
    public LabelPosition PortLabelPosition { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the shape may host internal components.
    /// </summary>
    public bool AllowInternalComponents { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Creates the default rules for the specified primitive node shape.
    /// </summary>
    /// <param name="shape">The shape to create rules for.</param>
    /// <returns>A new <see cref="ShapeRules"/> instance initialized with the default rules for <paramref name="shape"/>.</returns>
    public static ShapeRules CreateDefault(NodeShape shape)
    {
        return shape switch
        {
            NodeShape.RoundedRectangle => new ShapeRules
            {
                Shape = NodeShape.RoundedRectangle,
                MaxLeftPorts = int.MaxValue,
                MaxRightPorts = int.MaxValue,
                MaxTopPorts = 0,
                MaxBottomPorts = 0,
                HeaderPosition = LabelPosition.Inside,
                PortLabelPosition = LabelPosition.Inside,
                AllowInternalComponents = true,
            },

            NodeShape.Rectangle => new ShapeRules
            {
                Shape = NodeShape.Rectangle,
                MaxLeftPorts = int.MaxValue,
                MaxRightPorts = int.MaxValue,
                MaxTopPorts = 0,
                MaxBottomPorts = 0,
                HeaderPosition = LabelPosition.Inside,
                PortLabelPosition = LabelPosition.Inside,
                AllowInternalComponents = true,
            },

            NodeShape.Diamond => new ShapeRules
            {
                Shape = NodeShape.Diamond,
                MaxLeftPorts = 1,
                MaxRightPorts = 1,
                MaxTopPorts = 1,
                MaxBottomPorts = 1,
                HeaderPosition = LabelPosition.None,
                PortLabelPosition = LabelPosition.Outside,
                AllowInternalComponents = false,
            },

            NodeShape.Circle => new ShapeRules
            {
                Shape = NodeShape.Circle,
                MaxLeftPorts = 1,
                MaxRightPorts = 1,
                MaxTopPorts = 1,
                MaxBottomPorts = 1,
                HeaderPosition = LabelPosition.None,
                PortLabelPosition = LabelPosition.Outside,
                AllowInternalComponents = false,
            },

            NodeShape.Capsule => new ShapeRules
            {
                Shape = NodeShape.Capsule,
                MaxLeftPorts = 1,
                MaxRightPorts = 1,
                MaxTopPorts = 0,
                MaxBottomPorts = 0,
                HeaderPosition = LabelPosition.Inside,
                PortLabelPosition = LabelPosition.Outside,
                AllowInternalComponents = false,
            },

            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Unsupported node shape."),
        };
    }

    #endregion Public Methods
}

/// <summary>
/// Defines where a label may be displayed relative to a node shape or port.
/// </summary>
public enum LabelPosition
{
    None = 0,
    Inside = 1,
    Outside = 2,
}
