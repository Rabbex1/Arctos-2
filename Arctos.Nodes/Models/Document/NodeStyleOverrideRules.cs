namespace Arctos.Nodes.Models;

/// <summary>
/// Describes which categories of node styling may be overridden below the current style level.
/// </summary>
public sealed class NodeStyleOverrideRules
{
    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether general node styling may be overridden.
    /// </summary>
    public bool AllowStyleOverrides { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether border styling may be overridden.
    /// </summary>
    public bool AllowBorderOverrides { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether font styling may be overridden.
    /// </summary>
    public bool AllowFontOverrides { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether header styling may be overridden.
    /// </summary>
    public bool AllowHeaderOverrides { get; set; }

    #endregion Properties
}
