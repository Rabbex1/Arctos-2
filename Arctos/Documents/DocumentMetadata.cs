// ReSharper disable once CheckNamespace
namespace Arctos.Shell.Documents
{
    public class DocumentMetadata
    {
        public string DocumentType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
