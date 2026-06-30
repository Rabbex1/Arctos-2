namespace Arctos.History;

/// <summary>
/// Delegate-backed history entry implementation.
/// </summary>
public sealed class HistoryEntry : IHistoryEntry
{
    private readonly Action _undo;
    private readonly Action _redo;

    /// <summary>
    /// Creates a new history entry.
    /// </summary>
    /// <param name="description">The user-facing entry description.</param>
    /// <param name="redo">The action used to apply the change.</param>
    /// <param name="undo">The action used to revert the change.</param>
    public HistoryEntry(string description, Action redo, Action undo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(redo);
        ArgumentNullException.ThrowIfNull(undo);

        Description = description;
        _redo = redo;
        _undo = undo;
    }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public void Undo()
    {
        _undo();
    }

    /// <inheritdoc/>
    public void Redo()
    {
        _redo();
    }
}
