namespace Arctos.Nodes.Core.Inputs;

/// <summary>
/// Represents an integer value with a bounded range.
/// </summary>
public sealed class IntRangeValue
{
    #region Properties

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public int Value { get; set; }

    #endregion Properties
}

