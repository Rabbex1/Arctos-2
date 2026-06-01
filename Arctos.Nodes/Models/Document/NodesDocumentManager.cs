using System.Collections.ObjectModel;

namespace Arctos.Nodes.Models;

/// <summary>
/// Manages a set of node documents owned by the current application session.
/// </summary>
public sealed class NodesDocumentManager
{
    #region Fields

    private readonly List<NodeDocument> _documents = [];
    private readonly ReadOnlyCollection<NodeDocument> _readOnlyDocuments;

    #endregion Fields

    #region Events

    /// <summary>
    /// Raised when a document is added to the manager.
    /// </summary>
    public event EventHandler<NodeDocumentEventArgs>? DocumentAdded;

    /// <summary>
    /// Raised when a document is removed from the manager.
    /// </summary>
    public event EventHandler<NodeDocumentEventArgs>? DocumentRemoved;

    #endregion Events

    #region Properties

    /// <summary>
    /// Gets the documents currently owned by the manager.
    /// </summary>
    public IReadOnlyList<NodeDocument> Documents => _readOnlyDocuments;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node document manager.
    /// </summary>
    public NodesDocumentManager()
    {
        _readOnlyDocuments = _documents.AsReadOnly();
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Adds a node document to the manager.
    /// </summary>
    /// <param name="document">The document to add.</param>
    public void Add(NodeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (_documents.Any(item => item.Id == document.Id))
            throw new InvalidOperationException($"A node document with id '{document.Id}' has already been added.");

        _documents.Add(document);
        DocumentAdded?.Invoke(this, new NodeDocumentEventArgs(document));
    }

    /// <summary>
    /// Removes a node document from the manager.
    /// </summary>
    /// <param name="document">The document to remove.</param>
    /// <returns><see langword="true"/> when the document was removed; otherwise <see langword="false"/>.</returns>
    public bool Remove(NodeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return Remove(document.Id);
    }

    /// <summary>
    /// Removes a node document from the manager by identifier.
    /// </summary>
    /// <param name="documentId">The identifier of the document to remove.</param>
    /// <returns><see langword="true"/> when the document was removed; otherwise <see langword="false"/>.</returns>
    public bool Remove(Guid documentId)
    {
        var document = _documents.FirstOrDefault(item => item.Id == documentId);
        if (document == null)
            return false;

        _documents.Remove(document);
        DocumentRemoved?.Invoke(this, new NodeDocumentEventArgs(document));
        return true;
    }

    /// <summary>
    /// Removes all node documents from the manager.
    /// </summary>
    public void Clear()
    {
        foreach (var document in _documents.ToArray())
            Remove(document.Id);
    }

    /// <summary>
    /// Loads a node document from disk and adds it to the manager.
    /// </summary>
    /// <param name="path">The file path to read.</param>
    /// <returns>The loaded document.</returns>
    public NodeDocument LoadAndAdd(string path)
    {
        var document = NodeDocument.Load(path);
        Add(document);
        return document;
    }

    /// <summary>
    /// Saves a node document managed by the manager to disk.
    /// </summary>
    /// <param name="document">The document to save.</param>
    /// <param name="path">The file path to write.</param>
    public void Save(NodeDocument document, string path)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (_documents.All(item => item.Id != document.Id))
            throw new InvalidOperationException("The provided node document is not managed by this manager.");

        document.Save(path);
    }

    #endregion Public Methods
}
