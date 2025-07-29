// ReSharper disable once CheckNamespace
namespace Arctos.Shell.Documents
{
    public interface IFileDocumentVM : IPersistentDocumentVM
    {
        bool Load(string filePath);
        Task<bool> LoadAsync(string filePath);

    }
}
