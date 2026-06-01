namespace Arctos.Nodes.Models;

/// <summary>
/// Describes document-level editor settings that control how a graph document behaves in the editor.
/// </summary>
public sealed class NodeDocumentSettings
{
    #region Properties

    /// <summary>
    /// Gets or sets the persisted Arctos document format version.
    /// </summary>
    public int FormatVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the document contains node graph content.
    /// </summary>
    public bool ContainsGraph { get; set; }

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
    /// Gets or sets a value indicating whether library editing is allowed for the document.
    /// </summary>
    public bool AllowLibraryEdit { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether graph editing is allowed for the document.
    /// </summary>
    public bool AllowGraphEdit { get; set; } = true;

    #endregion Properties

}
