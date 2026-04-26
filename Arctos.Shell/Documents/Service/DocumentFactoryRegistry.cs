using System.Collections.Concurrent;

namespace Arctos.Shell.Documents
{
    public class DocumentFactoryRegistry
    {
        private readonly ConcurrentDictionary<IDocumentFactory, byte> _factories = new();

        public void RegisterFactory(IDocumentFactory factory)
        {
            _factories.TryAdd(factory, 0);
        }

        public bool UnregisterFactory(IDocumentFactory factory)
        {
            return _factories.TryRemove(factory, out _);
        }

        public IDocumentFactory? GetFactory(DocumentMetadata metadata)
        {
            return _factories.Keys.FirstOrDefault(f => f.CanCreate(metadata));
        }

        public IEnumerable<IDocumentFactory> GetMatchingFactories(DocumentMetadata metadata)
        {
            return _factories.Keys.Where(f => f.CanCreate(metadata));
        }
    }
}