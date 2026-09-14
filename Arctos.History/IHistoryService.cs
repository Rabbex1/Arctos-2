namespace Arctos.History;

/// <summary>
/// Creates and manages reusable history contexts.
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Returns an existing context or creates a new one when needed.
    /// </summary>
    /// <param name="id">The stable context identifier.</param>
    /// <returns>The resolved history context.</returns>
    IHistoryContext GetOrCreateContext(string id);

    /// <summary>
    /// Registers an existing detached context so it can be resolved by its stable identifier again.
    /// </summary>
    /// <param name="context">The context to register.</param>
    /// <returns>
    /// <see langword="true"/> when the context was registered, or when the same instance was
    /// already registered; otherwise, <see langword="false"/> when another instance owns the ID.
    /// </returns>
    bool TryRegisterContext(IHistoryContext context);

    /// <summary>
    /// Attempts to get an existing history context.
    /// </summary>
    /// <param name="id">The stable context identifier.</param>
    /// <param name="context">The resolved context when one exists.</param>
    /// <returns><see langword="true"/> when a context was found; otherwise, <see langword="false"/>.</returns>
    bool TryGetContext(string id, out IHistoryContext? context);

    /// <summary>
    /// Removes a history context.
    /// </summary>
    /// <param name="id">The stable context identifier.</param>
    /// <returns><see langword="true"/> when a context was removed; otherwise, <see langword="false"/>.</returns>
    bool RemoveContext(string id);

    /// <summary>
    /// Clears every tracked history context.
    /// </summary>
    void Clear();
}
