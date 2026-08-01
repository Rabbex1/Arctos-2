namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Describes map-level editor settings that control how a node map behaves in the editor.
/// </summary>
public sealed class NodeMapSettings
{
    #region Properties

    /// <summary>
    /// Gets or sets the persisted Arctos document format version.
    /// </summary>
    public int FormatVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the initial editor mode used when the document is opened.
    /// </summary>
    public OperationMode StartupMode { get; set; } = OperationMode.GraphEdit;

    /// <summary>
    /// Gets or sets the canvas background color used when rendering the document surface.
    /// </summary>
    public NodeColor CanvasColor { get; set; } = NodeColor.White;

    /// <summary>
    /// Gets or sets the human-readable document name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the application-defined document kind.
    /// </summary>
    public string? DocumentType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether runtime library editing is allowed for the map.
    /// </summary>
    public bool RuntimeLibraryEdit { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether runtime graph editing is allowed for the map.
    /// </summary>
    public bool RuntimeGraphEdit { get; set; } = true;

    #endregion Properties

}

