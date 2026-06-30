namespace Arctos.History;

/// <summary>
/// Represents a document-scoped undo/redo history context.
/// </summary>
public interface IHistoryContext
{
    /// <summary>
    /// Gets the stable context identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets a value indicating whether undo is currently available.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Gets a value indicating whether redo is currently available.
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Gets the current undo description, if any.
    /// </summary>
    string? UndoDescription { get; }

    /// <summary>
    /// Gets the current redo description, if any.
    /// </summary>
    string? RedoDescription { get; }

    /// <summary>
    /// Raised whenever the undo/redo availability or descriptions change.
    /// </summary>
    event EventHandler<HistoryStateArgs>? StateChanged;

    /// <summary>
    /// Executes a delegate-backed entry and records it in the context.
    /// </summary>
    /// <param name="description">The user-facing entry description.</param>
    /// <param name="redo">The action that applies the change.</param>
    /// <param name="undo">The action that reverts the change.</param>
    void Execute(string description, Action redo, Action undo);

    /// <summary>
    /// Executes a prepared history entry and records it in the context.
    /// </summary>
    /// <param name="entry">The history entry to execute.</param>
    void Execute(IHistoryEntry entry);

    /// <summary>
    /// Begins a grouped transaction that records multiple entries as a single history item.
    /// </summary>
    /// <param name="description">The user-facing transaction description.</param>
    /// <returns>The active transaction scope.</returns>
    IHistoryTransaction BeginTransaction(string description);

    /// <summary>
    /// Undoes the most recent committed entry, if any.
    /// </summary>
    void Undo();

    /// <summary>
    /// Redoes the most recent reverted entry, if any.
    /// </summary>
    void Redo();

    /// <summary>
    /// Clears the history state for this context.
    /// </summary>
    void Clear();
}
