namespace Arctos.History;

/// <summary>
/// Represents a pending grouped history transaction.
/// </summary>
public interface IHistoryTransaction : IDisposable
{
    /// <summary>
    /// Gets the user-facing transaction description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Commits the grouped entries to the owning history context.
    /// </summary>
    void Commit();

    /// <summary>
    /// Rolls the grouped entries back without adding them to history.
    /// </summary>
    void Rollback();
}
