namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Describes component UID validation issues discovered in a persisted node artifact.
/// </summary>
public sealed class NodeComponentUidValidationResult
{
    /// <summary>
    /// Gets the number of components missing an internal UID.
    /// </summary>
    public required int MissingUidCount { get; init; }

    /// <summary>
    /// Gets the number of components whose internal UID duplicated an earlier component in the same file.
    /// </summary>
    public required int DuplicateUidCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether the file passed validation.
    /// </summary>
    public bool IsValid => MissingUidCount == 0 && DuplicateUidCount == 0;
}

