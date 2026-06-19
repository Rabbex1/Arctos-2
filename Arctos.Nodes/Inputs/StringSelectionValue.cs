using System.Collections.Generic;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents a string value chosen from a fixed set of options.
/// </summary>
public sealed class StringSelectionValue
{
    #region Properties

    /// <summary>
    /// Gets or sets where the available options are sourced from.
    /// </summary>
    public StringSelectionSource Source { get; set; }

    /// <summary>
    /// Gets or sets the available string options.
    /// </summary>
    public List<string> Options { get; set; } = [];

    /// <summary>
    /// Gets or sets the currently selected string value.
    /// </summary>
    public string? Value { get; set; }

    #endregion Properties
}
