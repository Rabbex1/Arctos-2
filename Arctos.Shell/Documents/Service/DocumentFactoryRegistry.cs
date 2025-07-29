using System.Collections.Concurrent;

namespace Arctos.Shell.Documents
{
    public class DocumentFactoryRegistry
    {
        private readonly ConcurrentBag<IDocumentFactory> _factories = [];

        public void RegisterFactory(IDocumentFactory factory)
        {
            _factories.Add(factory);
        }

        public IDocumentFactory? GetFactory(DocumentMetadata metadata)
        {
            return _factories.FirstOrDefault(f => f.CanCreate(metadata));
        }

        public IEnumerable<IDocumentFactory> GetMatchingFactories(DocumentMetadata metadata)
        {
            return _factories.Where(f => f.CanCreate(metadata));
        }
    }
}
