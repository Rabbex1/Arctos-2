namespace Arctos.Nodes.Core.Inputs;

/// <summary>
/// Represents a floating-point value with a bounded range.
/// </summary>
public sealed class FloatRangeValue
{
    #region Properties

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    public float Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    public float Max { get; set; }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public float Value { get; set; }

    #endregion Properties
}

