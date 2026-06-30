namespace Arctos.History;

/// <summary>
/// Default in-memory implementation of a document-scoped history context.
/// </summary>
public sealed class HistoryContext : IHistoryContext
{
    private readonly LinkedList<IHistoryEntry> _undoStack = [];
    private readonly Stack<IHistoryEntry> _redoStack = new();
    private readonly int _maxEntries;
    private HistoryTransaction? _activeTransaction;
    private bool _isReplaying;

    /// <summary>
    /// Creates a new history context.
    /// </summary>
    /// <param name="id">The stable context identifier.</param>
    /// <param name="maxEntries">The maximum number of undo entries to retain.</param>
    public HistoryContext(string id, int maxEntries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEntries);

        Id = id;
        _maxEntries = maxEntries;
    }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public bool CanUndo => _undoStack.Count > 0;

    /// <inheritdoc/>
    public bool CanRedo => _redoStack.Count > 0;

    /// <inheritdoc/>
    public string? UndoDescription => _undoStack.Last?.Value.Description;

    /// <inheritdoc/>
    public string? RedoDescription => _redoStack.Count > 0 ? _redoStack.Peek().Description : null;

    /// <inheritdoc/>
    public event EventHandler<HistoryStateArgs>? StateChanged;

    /// <inheritdoc/>
    public void Execute(string description, Action redo, Action undo)
    {
        Execute(new HistoryEntry(description, redo, undo));
    }

    /// <inheritdoc/>
    public void Execute(IHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        EnsureWritableState();

        if (_activeTransaction != null)
        {
            ExecuteEntry(entry);
            _activeTransaction.Add(entry);
            return;
        }

        ExecuteEntry(entry);
        PushUndoEntry(entry);
        _redoStack.Clear();
        RaiseStateChanged();
    }

    /// <inheritdoc/>
    public IHistoryTransaction BeginTransaction(string description)
    {
        EnsureWritableState();
        if (_activeTransaction != null)
            throw new InvalidOperationException("Nested history transactions are not supported.");

        _activeTransaction = new HistoryTransaction(this, description);
        return _activeTransaction;
    }

    /// <inheritdoc/>
    public void Undo()
    {
        if (_undoStack.Count == 0)
            return;

        var entry = _undoStack.Last!.Value;
        _undoStack.RemoveLast();
        Replay(() => entry.Undo());
        _redoStack.Push(entry);
        RaiseStateChanged();
    }

    /// <inheritdoc/>
    public void Redo()
    {
        if (_redoStack.Count == 0)
            return;

        var entry = _redoStack.Pop();
        Replay(() => entry.Redo());
        PushUndoEntry(entry);
        RaiseStateChanged();
    }

    /// <inheritdoc/>
    public void Clear()
    {
        if (_activeTransaction != null)
            _activeTransaction.Rollback();

        _undoStack.Clear();
        _redoStack.Clear();
        RaiseStateChanged();
    }

    private void EnsureWritableState()
    {
        if (_isReplaying)
            throw new InvalidOperationException("History cannot be recorded while undo or redo is in progress.");
    }

    private void ExecuteEntry(IHistoryEntry entry)
    {
        Replay(() => entry.Redo());
    }

    private void Replay(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_isReplaying)
            throw new InvalidOperationException("History replay is already in progress.");

        _isReplaying = true;
        try
        {
            action();
        }
        finally
        {
            _isReplaying = false;
        }
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this, new HistoryStateArgs(CanUndo, CanRedo, UndoDescription, RedoDescription));
    }

    private void PushUndoEntry(IHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _undoStack.AddLast(entry);

        while (_undoStack.Count > _maxEntries)
            _undoStack.RemoveFirst();
    }

    private void CommitTransaction(HistoryTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (!ReferenceEquals(_activeTransaction, transaction))
            throw new InvalidOperationException("The supplied transaction is not active.");

        _activeTransaction = null;

        if (transaction.EntryCount == 0)
        {
            RaiseStateChanged();
            return;
        }

        PushUndoEntry(transaction.BuildCompositeEntry());
        _redoStack.Clear();
        RaiseStateChanged();
    }

    private void RollbackTransaction(HistoryTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (!ReferenceEquals(_activeTransaction, transaction))
            throw new InvalidOperationException("The supplied transaction is not active.");

        _activeTransaction = null;

        foreach (var entry in transaction.Entries.Reverse())
            Replay(() => entry.Undo());

        RaiseStateChanged();
    }

    private sealed class HistoryTransaction : IHistoryTransaction
    {
        private readonly HistoryContext _owner;
        private readonly List<IHistoryEntry> _entries = [];
        private bool _isCompleted;

        public HistoryTransaction(HistoryContext owner, string description)
        {
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            _owner = owner;
            Description = description;
        }

        public string Description { get; }

        public IReadOnlyList<IHistoryEntry> Entries => _entries;

        public int EntryCount => _entries.Count;

        public void Commit()
        {
            EnsureActive();
            _isCompleted = true;
            _owner.CommitTransaction(this);
        }

        public void Rollback()
        {
            EnsureActive();
            _isCompleted = true;
            _owner.RollbackTransaction(this);
        }

        public void Dispose()
        {
            if (_isCompleted)
                return;

            Rollback();
        }

        public void Add(IHistoryEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);
            EnsureActive();
            _entries.Add(entry);
        }

        public CompositeHistoryEntry BuildCompositeEntry()
        {
            return new CompositeHistoryEntry(Description, _entries);
        }

        private void EnsureActive()
        {
            if (_isCompleted)
                throw new InvalidOperationException("The transaction has already completed.");
        }
    }

    private sealed class CompositeHistoryEntry : IHistoryEntry
    {
        private readonly IReadOnlyList<IHistoryEntry> _entries;

        public CompositeHistoryEntry(string description, IReadOnlyList<IHistoryEntry> entries)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            ArgumentNullException.ThrowIfNull(entries);

            Description = description;
            _entries = entries.ToArray();
        }

        public string Description { get; }

        public void Undo()
        {
            for (var index = _entries.Count - 1; index >= 0; index--)
                _entries[index].Undo();
        }

        public void Redo()
        {
            foreach (var entry in _entries)
                entry.Redo();
        }
    }
}
