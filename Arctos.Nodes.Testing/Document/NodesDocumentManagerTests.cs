using Arctos.Nodes.Models;
using Xunit;

namespace Arctos.Nodes.Testing.Document;

public sealed class NodesDocumentManagerTests
{
    [Fact]
    public void Add_Should_Store_Document_And_Raise_DocumentAdded()
    {
        var manager = new NodesDocumentManager();
        var document = CreateDocument("One");
        NodeDocument? addedDocument = null;

        manager.DocumentAdded += (_, args) => addedDocument = args.Document;

        manager.Add(document);

        Assert.Single(manager.Documents);
        Assert.Same(document, manager.Documents[0]);
        Assert.Same(document, addedDocument);
    }

    [Fact]
    public void Add_Should_Throw_When_Document_With_Same_Id_Is_Already_Managed()
    {
        var document = CreateDocument("One");
        var duplicate = new NodeDocument(new NodeDocumentSettings(), new NodeLibrary())
        {
            Id = document.Id,
        };
        var manager = new NodesDocumentManager();
        manager.Add(document);

        Assert.Throws<InvalidOperationException>(() => manager.Add(duplicate));
    }

    [Fact]
    public void Remove_Should_Remove_Document_And_Raise_DocumentRemoved()
    {
        var manager = new NodesDocumentManager();
        var document = CreateDocument("One");
        NodeDocument? removedDocument = null;
        manager.Add(document);
        manager.DocumentRemoved += (_, args) => removedDocument = args.Document;

        var removed = manager.Remove(document);

        Assert.True(removed);
        Assert.Empty(manager.Documents);
        Assert.Same(document, removedDocument);
    }

    [Fact]
    public void Save_Should_Throw_When_Document_Is_Not_Managed()
    {
        var manager = new NodesDocumentManager();
        var document = CreateDocument("One");
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.arctos-nodes.json");

        try
        {
            Assert.Throws<InvalidOperationException>(() => manager.Save(document, path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static NodeDocument CreateDocument(string name)
    {
        return new NodeDocument(
            new NodeDocumentSettings
            {
                Name = name,
            },
            new NodeLibrary());
    }
}
