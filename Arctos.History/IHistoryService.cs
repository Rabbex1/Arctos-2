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
