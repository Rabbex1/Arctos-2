// ReSharper disable once CheckNamespace
namespace Arctos.Shell.Documents
{
    public interface IDocumentVM : IDockingPaneVM
    {
        public DocumentMetadata? Metadata { get; set; }
    }
}
