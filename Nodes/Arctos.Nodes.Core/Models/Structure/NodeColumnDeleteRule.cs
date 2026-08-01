namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Defines how a column deletion request should resolve any existing content.
/// </summary>
public enum NodeColumnDeleteRule
{
    /// <summary>
    /// Cancels the delete request without making any changes.
    /// </summary>
    Cancel = 0,

    /// <summary>
    /// Deletes the selected column and any content it owns.
    /// </summary>
    Delete = 1,

    /// <summary>
    /// Moves the selected column content into the column on the left, then removes the selected column.
    /// </summary>
    MoveLeft = 2,

    /// <summary>
    /// Moves the selected column content into the column on the right, then removes the selected column.
    /// </summary>
    MoveRight = 3,
}

