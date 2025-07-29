// ReSharper disable once CheckNamespace

namespace Arctos.Shell.Documents
{
    public interface IDocumentFactory
    {
        /// <summary>
        /// Returns true if this factory can create a document for the given metadata.
        /// </summary>
        bool CanCreate(DocumentMetadata metadata);

        /// <summary>
        /// Creates a document for the given metadata.
        /// </summary>
        IDocumentVM? Create(DocumentMetadata metadata);

    }
}
