using Arctos.Nodes.Models;
using Xunit;

namespace Arctos.Nodes.Testing.Document;

public sealed class NodeLibraryTests
{
    [Fact]
    public void Add_Should_Store_Definition_By_TypeName()
    {
        var definition = CreateDefinition("Server", "Server");
        var library = new NodeLibrary {definition};

        Assert.True(library.Contains("Server"));
        Assert.Same(definition, library["Server"]);
    }

    [Fact]
    public void IndexerSetter_Should_Throw_When_Key_Does_Not_Match_Definition_TypeName()
    {
        var definition = CreateDefinition("Routine", "Routine");
        var library = new NodeLibrary();

        Assert.Throws<ArgumentException>(() => library["Shader"] = definition);
    }

    [Fact]
    public void TryGetValue_Should_Return_False_When_Type_Is_Not_Found()
    {
        var library = new NodeLibrary();

        var found = library.TryGetValue("Missing", out var definition);

        Assert.False(found);
        Assert.Null(definition);
    }

    [Fact]
    public void Remove_Should_Return_True_When_Type_Exists()
    {
        var definition = CreateDefinition("Invoke", "Routine");
        var library = new NodeLibrary {definition};

        var removed = library.Remove("Invoke");

        Assert.True(removed);
        Assert.False(library.Contains("Invoke"));
    }

    private static NodeDefinition CreateDefinition(string typeName, string domainType)
    {
        return new NodeDefinition
        {
            TypeName = typeName,
            DomainType = domainType,
        };
    }
}
