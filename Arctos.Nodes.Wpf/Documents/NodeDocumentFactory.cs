using Arctos.History;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.Documents;
using Arctos.Shell.Documents;

namespace Arctos.Nodes.Wpf.Documents;

/// <summary>
/// Creates file-backed node artifact document view models.
/// </summary>
public sealed class NodeDocumentFactory : IDocumentFactory
{
    #region Fields

    private readonly NodesManager _nodesManager;
    private readonly NodeSession _session;
    private readonly IHistoryService _historyService;

    #endregion Fields

    #region Constructors

    public NodeDocumentFactory(NodesManager nodesManager, NodeSession session, IHistoryService historyService)
    {
        _nodesManager = nodesManager ?? throw new ArgumentNullException(nameof(nodesManager));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
    }

    #endregion Constructors

    #region Public Methods

    public bool CanCreate(DocumentMetadata metadata)
    {
        return metadata.DocumentType == "File"
               && metadata.Properties?.TryGetValue("extension", out var extension) == true
               && (string.Equals(extension, "nmap", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(extension, "nlib", StringComparison.OrdinalIgnoreCase));
    }

    public IDocumentVM? Create(DocumentMetadata metadata)
    {
        var extension = metadata.Properties?.TryGetValue("extension", out var value) == true
            ? value
            : null;
        var historyContext = _historyService.GetOrCreateContext($"nodes-document:{Guid.NewGuid():N}");

        return string.Equals(extension, "nlib", StringComparison.OrdinalIgnoreCase)
            ? new NodeLibraryDocumentVM(_nodesManager, _session, _historyService, historyContext)
            {
                DisplayName = string.IsNullOrWhiteSpace(metadata.Title) ? "Untitled.nlib" : metadata.Title
            }
            : new NodeMapDocumentVM(_nodesManager, _session, _historyService, historyContext)
        {
            DisplayName = string.IsNullOrWhiteSpace(metadata.Title) ? "Untitled.nmap" : metadata.Title
        };
    }

    #endregion Public Methods
}
