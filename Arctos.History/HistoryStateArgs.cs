namespace Arctos.History;

/// <summary>
/// Provides the current state of a history context after a state change.
/// </summary>
public sealed class HistoryStateArgs : EventArgs
{
    /// <summary>
    /// Creates a new history state snapshot.
    /// </summary>
    /// <param name="canUndo">Whether the context can currently undo.</param>
    /// <param name="canRedo">Whether the context can currently redo.</param>
    /// <param name="undoDescription">The current undo description, if any.</param>
    /// <param name="redoDescription">The current redo description, if any.</param>
    public HistoryStateArgs(bool canUndo, bool canRedo, string? undoDescription, string? redoDescription)
    {
        CanUndo = canUndo;
        CanRedo = canRedo;
        UndoDescription = undoDescription;
        RedoDescription = redoDescription;
    }

    /// <summary>
    /// Gets a value indicating whether undo is currently available.
    /// </summary>
    public bool CanUndo { get; }

    /// <summary>
    /// Gets a value indicating whether redo is currently available.
    /// </summary>
    public bool CanRedo { get; }

    /// <summary>
    /// Gets the current undo description, if any.
    /// </summary>
    public string? UndoDescription { get; }

    /// <summary>
    /// Gets the current redo description, if any.
    /// </summary>
    public string? RedoDescription { get; }
}
