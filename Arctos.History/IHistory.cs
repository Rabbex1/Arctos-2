namespace Arctos.History;

/// <summary>
/// Exposes a document- or surface-owned history context together with an explicit release hook.
/// </summary>
public interface IHistory
{
    /// <summary>
    /// Gets the owned history context.
    /// </summary>
    IHistoryContext History { get; }

    /// <summary>
    /// Releases the owned history context when the owner is closing or otherwise disposing of its edit session.
    /// </summary>
    void ReleaseHistoryContext();
}
