using System.Collections.Generic;
using System.Text.Json;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the top-level node document passed between an application and <c>Arctos.Nodes</c>.
/// </summary>
public sealed class NodeDocument(NodeDocumentSettings settings, NodeLibrary nodeLibrary, NodeGraph? nodeGraph = null)
{
    #region Fields

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the stable identifier of the document.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the document settings that control document-level editor behavior.
    /// </summary>
    public NodeDocumentSettings Settings { get; } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Gets the node library used by the document toolbox and node instances.
    /// </summary>
    public NodeLibrary NodeLibrary { get; } = nodeLibrary ?? throw new ArgumentNullException(nameof(nodeLibrary));

    /// <summary>
    /// Gets or sets the default node style used by the document. Definitions inherit this style when they do not define their own style.
    /// </summary>
    public NodeStyle Style { get; set; } = new();

    /// <summary>
    /// Gets the optional persisted node graph content owned by the document.
    /// </summary>
    public NodeGraph? NodeGraph { get; } = InitializeNodeGraph(settings ?? throw new ArgumentNullException(nameof(settings)), nodeGraph);

    #endregion Properties

    #region Internal Methods

    /// <summary>
    /// Saves the node document to the provided path using the built-in neutral document format.
    /// </summary>
    /// <param name="path">The file path to write.</param>
    internal void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var model = NodeDocumentFileModel.FromDocument(this);
        var json = JsonSerializer.Serialize(model, JsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Loads a node document from the provided path using the built-in neutral document format.
    /// </summary>
    /// <param name="path">The file path to read.</param>
    /// <returns>The loaded node document.</returns>
    internal static NodeDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var json = File.ReadAllText(path);
        var model = JsonSerializer.Deserialize<NodeDocumentFileModel>(json, JsonOptions)
                    ?? throw new InvalidOperationException("The node document could not be deserialized.");

        return model.ToDocument();
    }

    #endregion Internal Methods

    #region Private Methods

    private static NodeGraph? InitializeNodeGraph(NodeDocumentSettings settings, NodeGraph? nodeGraph)
    {
        if (nodeGraph == null)
        {
            if (settings.ContainsGraph)
                throw new ArgumentException("The document settings indicate that a graph is present, but no node graph was provided.", nameof(nodeGraph));

            return null;
        }

        settings.ContainsGraph = true;
        return nodeGraph;
    }

    #endregion Private Methods

}
