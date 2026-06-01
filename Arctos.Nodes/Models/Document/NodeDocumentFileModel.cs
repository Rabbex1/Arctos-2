using System.Collections.Generic;

namespace Arctos.Nodes.Models;

internal sealed class NodeDocumentFileModel
{
    #region Properties

    public required Guid Id { get; set; }

    public required NodeDocumentSettings Settings { get; set; }

    public required NodeStyle Style { get; set; }

    public required List<NodeDefinition> NodeLibrary { get; set; }

    public NodeGraph? NodeGraph { get; set; }

    #endregion Properties

    #region Public Methods

    public static NodeDocumentFileModel FromDocument(NodeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new NodeDocumentFileModel
        {
            Id = document.Id,
            Settings = document.Settings,
            Style = document.Style,
            NodeLibrary = [.. document.NodeLibrary],
            NodeGraph = document.NodeGraph,
        };
    }

    public NodeDocument ToDocument()
    {
        var nodeLibrary = new NodeLibrary();

        foreach (var nodeDefinition in NodeLibrary)
            nodeLibrary.Add(nodeDefinition);

        return new NodeDocument(Settings, nodeLibrary, NodeGraph)
        {
            Id = Id,
            Style = Style,
        };
    }

    #endregion Public Methods
}
