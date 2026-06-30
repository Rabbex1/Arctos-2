namespace Arctos.History;

/// <summary>
/// Represents a single undoable and redoable history operation.
/// </summary>
public interface IHistoryEntry
{
    /// <summary>
    /// Gets the user-facing description of the operation.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Reverts the operation.
    /// </summary>
    void Undo();

    /// <summary>
    /// Reapplies the operation.
    /// </summary>
    void Redo();
}
