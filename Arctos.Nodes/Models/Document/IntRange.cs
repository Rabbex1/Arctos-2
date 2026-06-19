namespace Arctos.Nodes.Models;

/// <summary>
/// Represents an inclusive integer range.
/// </summary>
public readonly record struct IntRange(int Minimum, int Maximum)
{
    #region Public Methods

    /// <summary>
    /// Clamps the provided value into the current range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public int Clamp(int value)
    {
        return Math.Clamp(value, Minimum, Maximum);
    }

    #endregion Public Methods
}
