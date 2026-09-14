namespace Arctos.History;

/// <summary>
/// Default in-memory history service implementation.
/// </summary>
public sealed class HistoryService : IHistoryService
{
    public const int DefaultMaxEntries = 100;
    private readonly Dictionary<string, IHistoryContext> _contexts = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a new history service using the default maximum entry count.
    /// </summary>
    public HistoryService()
        : this(DefaultMaxEntries)
    {
    }

    /// <summary>
    /// Creates a new history service.
    /// </summary>
    /// <param name="maxEntries">The maximum number of undo entries to retain per context.</param>
    public HistoryService(int maxEntries)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEntries);
        MaxEntries = maxEntries;
    }

    /// <summary>
    /// Gets the maximum number of undo entries retained per context.
    /// </summary>
    public int MaxEntries { get; }

    /// <inheritdoc/>
    public IHistoryContext GetOrCreateContext(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (_contexts.TryGetValue(id, out var existingContext))
            return existingContext;

        var createdContext = new HistoryContext(id, MaxEntries);
        _contexts.Add(id, createdContext);
        return createdContext;
    }

    /// <inheritdoc/>
    public bool TryRegisterContext(IHistoryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.Id);

        if (_contexts.TryGetValue(context.Id, out var existingContext))
            return ReferenceEquals(existingContext, context);

        _contexts.Add(context.Id, context);
        return true;
    }

    /// <inheritdoc/>
    public bool TryGetContext(string id, out IHistoryContext? context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (_contexts.TryGetValue(id, out var resolvedContext))
        {
            context = resolvedContext;
            return true;
        }

        context = null;
        return false;
    }

    /// <inheritdoc/>
    public bool RemoveContext(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return _contexts.Remove(id);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        foreach (var context in _contexts.Values)
            context.Clear();

        _contexts.Clear();
    }
}
