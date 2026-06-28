namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a repeated grid of numeric or integer input cells inside a detailed node.
/// </summary>
public sealed class NodeGridInput : NodeComponent
{
    #region Fields

    /// <summary>
    /// Defines the valid row-count range.
    /// </summary>
    public static readonly IntRange RowRange = new(1, 8);

    /// <summary>
    /// Defines the valid column-count range.
    /// </summary>
    public static readonly IntRange ColumnRange = new(1, 4);

    /// <summary>
    /// Defines the minimum allowed cell width in node units.
    /// </summary>
    public const float MinimumCellWidth = 1.0f;

    /// <summary>
    /// Defines the default cell width in node units.
    /// </summary>
    public const float DefaultCellWidth = 5.0f;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the number of rows in the grid.
    /// </summary>
    public int Rows { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of columns in the grid.
    /// </summary>
    public int Columns { get; set; } = 1;

    /// <summary>
    /// Gets or sets the width of each cell in node units.
    /// </summary>
    public float CellWidth { get; set; } = DefaultCellWidth;

    /// <summary>
    /// Gets or sets the shared cell type used by the grid.
    /// </summary>
    public InputTypeId CellTypeId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the inclusive minimum cell value.
    /// </summary>
    public double MinValue { get; set; } = 0.0d;

    /// <summary>
    /// Gets or sets the inclusive maximum cell value.
    /// </summary>
    public double MaxValue { get; set; } = 1.0d;

    /// <summary>
    /// Gets or sets the row-major default values for the grid.
    /// </summary>
    public double[] DefaultValues { get; set; } = [0.0d];

    #endregion Properties
}
