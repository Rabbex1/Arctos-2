using Arctos.Nodes.Models;
using Xunit;

namespace Arctos.Nodes.Testing.Document;

public sealed class NodesManagerTests
{
    [Fact]
    public void AddMap_Should_Store_Map_And_Raise_MapAdded()
    {
        var manager = new NodesManager();
        var map = CreateMap("One");
        NodeMap? addedMap = null;

        manager.MapAdded += (_, args) => addedMap = args.Map;

        manager.AddMap(map);

        Assert.Single(manager.Maps);
        Assert.Same(map, manager.Maps[0]);
        Assert.Same(map, addedMap);
    }

    [Fact]
    public void AddMap_Should_Throw_When_Map_With_Same_Id_Is_Already_Managed()
    {
        var map = CreateMap("One");
        var duplicate = new NodeMap(new NodeMapSettings(), new NodeGraph(), CreateMapLibraries())
        {
            Id = map.Id,
        };

        var manager = new NodesManager();
        manager.AddMap(map);

        Assert.Throws<InvalidOperationException>(() => manager.AddMap(duplicate));
    }

    [Fact]
    public void RemoveMap_Should_Remove_Map_And_Raise_MapRemoved()
    {
        var manager = new NodesManager();
        var map = CreateMap("One");
        NodeMap? removedMap = null;
        manager.AddMap(map);
        manager.MapRemoved += (_, args) => removedMap = args.Map;

        var removed = manager.RemoveMap(map);

        Assert.True(removed);
        Assert.Empty(manager.Maps);
        Assert.Same(map, removedMap);
    }

    [Fact]
    public void AddLibrary_Should_Store_Library_And_Raise_LibraryAdded()
    {
        var manager = new NodesManager();
        var library = CreateSharedLibrary("Standard", "1.0.0");
        NodeLibrary? addedLibrary = null;

        manager.LibraryAdded += (_, args) => addedLibrary = args.Library;

        manager.AddLibrary(library);

        Assert.Single(manager.Libraries);
        Assert.Same(library, manager.Libraries[0]);
        Assert.Same(library, addedLibrary);
    }

    [Fact]
    public void AddLibrary_Should_Throw_When_Library_With_Same_Name_And_Version_Is_Already_Managed()
    {
        var manager = new NodesManager();
        manager.AddLibrary(CreateSharedLibrary("Standard", "1.0.0"));

        Assert.Throws<InvalidOperationException>(() => manager.AddLibrary(CreateSharedLibrary("Standard", "1.0.0")));
    }

    [Fact]
    public void SaveMap_Should_Throw_When_Map_Is_Not_Managed()
    {
        var manager = new NodesManager();
        var map = CreateMap("One");
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nmap");

        try
        {
            Assert.Throws<InvalidOperationException>(() => manager.SaveMap(map, path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void SaveLibrary_Should_Throw_When_Library_Is_Not_Managed()
    {
        var manager = new NodesManager();
        var library = CreateSharedLibrary("Standard", "1.0.0");
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nlib");

        try
        {
            Assert.Throws<InvalidOperationException>(() => manager.SaveLibrary(library, path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void LoadLibraryAndAdd_Should_Load_Definitions_From_Nlib_File()
    {
        var manager = new NodesManager();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nlib");

        File.WriteAllText(path,
            """
            {
              "Name": "Minimal Library",
              "Version": "1.0.0",
              "Library": {
                "Name": "Minimal Library",
                "Version": "1.0.0",
                "definitions": [
                  {
                    "TypeName": "SimpleNode",
                    "DisplayName": "Simple Node",
                    "DomainType": "Test",
                    "NodeShape": 0,
                    "IconSvg": null,
                    "Style": null,
                    "Components": []
                  }
                ]
              }
            }
            """);

        try
        {
            var library = manager.LoadLibraryAndAdd(path);

            Assert.Equal("Minimal Library", library.Name);
            Assert.Equal("1.0.0", library.Version);
            Assert.Single(library.Definitions);
            Assert.True(library.Contains("SimpleNode"));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static NodeMap CreateMap(string name)
    {
        return new NodeMap(
            new NodeMapSettings
            {
                Name = name,
            },
            new NodeGraph(),
            CreateMapLibraries());
    }

    private static NodeLibrary[] CreateMapLibraries()
    {
        return [new NodeLibrary()];
    }

    private static NodeLibrary CreateSharedLibrary(string name, string version)
    {
        return new NodeLibrary
        {
            Name = name,
            Version = version,
        };
    }
}
