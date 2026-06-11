using Arctos.Nodes.Models;
using Xunit;

namespace Arctos.Nodes.Testing.Document;

public sealed class NodeMapTests
{
    [Fact]
    public void Constructor_Should_Store_Provided_Settings_Graph_And_Libraries()
    {
        var settings = new NodeMapSettings { Name = "Test Map", DocumentType = "Routine" };
        var graph = new NodeGraph();
        var library = CreateLocalLibrary();

        var map = new NodeMap(settings, graph, [library]);

        Assert.Same(settings, map.Settings);
        Assert.Same(graph, map.Graph);
        Assert.Single(map.Libraries);
        Assert.Same(library, map.Libraries[0]);
        Assert.True(map.Settings.ContainsGraph);
        Assert.NotEqual(Guid.Empty, map.Id);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Are_Null()
    {
        var graph = new NodeGraph();
        var library = CreateLocalLibrary();

        Assert.Throws<ArgumentNullException>(() => new NodeMap(null!, graph, [library]));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Graph_Is_Null()
    {
        var settings = new NodeMapSettings();
        var library = CreateLocalLibrary();

        Assert.Throws<ArgumentNullException>(() => new NodeMap(settings, null!, [library]));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Libraries_Are_Null()
    {
        var settings = new NodeMapSettings();
        var graph = new NodeGraph();

        Assert.Throws<ArgumentNullException>(() => new NodeMap(settings, graph, null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Libraries_Are_Empty()
    {
        var settings = new NodeMapSettings();
        var graph = new NodeGraph();

        Assert.Throws<ArgumentException>(() => new NodeMap(settings, graph, []));
    }

    [Fact]
    public void Save_And_Load_Map_With_LocalLibrary_Should_RoundTrip()
    {
        var settings = CreateSettings();
        var library = CreateLocalLibrary();
        var graph = CreateGraph();
        var style = CreateStyle();
        var map = new NodeMap(settings, graph, [library])
        {
            Style = style,
        };

        var manager = new NodesManager();
        manager.AddMap(map);

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nmap");

        try
        {
            manager.SaveMap(map, path);

            var loadManager = new NodesManager();
            var loaded = loadManager.LoadMapAndAdd(path);

            Assert.Equal(map.Id, loaded.Id);
            Assert.Equal("Network Map", loaded.Settings.Name);
            Assert.Equal("NetMap", loaded.Settings.DocumentType);
            Assert.Equal(OperationMode.Run, loaded.Settings.StartupMode);
            Assert.Equal(new NodeColor(24, 28, 32), loaded.Settings.CanvasColor);
            Assert.True(loaded.Settings.RuntimeLibraryEdit);
            Assert.False(loaded.Settings.RuntimeGraphEdit);
            Assert.Equal(new NodeColor(240, 240, 240), loaded.Style.ShapeStyle.BackgroundColor);

            var loadedLibrary = Assert.Single(loaded.Libraries);
            Assert.Null(loadedLibrary.Name);
            Assert.Null(loadedLibrary.Version);
            Assert.True(loadedLibrary.Contains("Server"));

            var loadedType = loadedLibrary["Server"];
            Assert.Equal("Server Node", loadedType.DisplayName);
            Assert.Equal(NodeShape.RoundedRectangle, loadedType.NodeShape);
            Assert.Equal(new NodeColor(7, 7, 255), loadedType.Style!.ShapeStyle.BackgroundColor);
            Assert.Equal(NodeFontWeight.Bold, loadedType.Style.ShapeStyle.FontWeight);
            Assert.Equal("Primary Server", loadedType.Components[0].GetDefaultValue<string>());

            var value = loadedType.Components[1].GetDefaultValue<StringSelectionValue>();
            Assert.NotNull(value);
            Assert.Equal("Database", value!.Value);

            Assert.Collection(
                loaded.Graph.Nodes,
                node =>
                {
                    var leaf = Assert.IsType<LeafNode>(node);
                    Assert.Equal("server-1", leaf.Id);
                    Assert.Equal("Server", leaf.NodeDefinitionName);
                    Assert.Equal(new NodePoint(100.0f, 200.0f), leaf.Position);
                },
                node =>
                {
                    var composite = Assert.IsType<CompositeNode>(node);
                    Assert.Equal("cluster-1", composite.Id);
                    Assert.Equal(new NodeSize(640.0f, 480.0f), composite.Size);
                    Assert.Single(composite.Children);
                });
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Save_And_Load_Map_With_ReferencedLibrary_Should_Resolve_Through_Manager()
    {
        var sharedLibrary = CreateSharedLibrary();
        var manager = new NodesManager();
        manager.AddLibrary(sharedLibrary);

        var map = new NodeMap(
            new NodeMapSettings { Name = "Shared Library Map" },
            new NodeGraph(),
            [sharedLibrary]);

        manager.AddMap(map);

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nmap");

        try
        {
            manager.SaveMap(map, path);

            var loadManager = new NodesManager();
            loadManager.AddLibrary(sharedLibrary);

            var loaded = loadManager.LoadMapAndAdd(path);

            var loadedLibrary = Assert.Single(loaded.Libraries);
            Assert.Same(sharedLibrary, loadedLibrary);
            Assert.Equal("Standard", loadedLibrary.Name);
            Assert.Equal("1.0.0", loadedLibrary.Version);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static NodeMapSettings CreateSettings()
    {
        return new NodeMapSettings
        {
            FormatVersion = 1,
            CanvasColor = new NodeColor(24, 28, 32),
            Name = "Network Map",
            DocumentType = "NetMap",
            StartupMode = OperationMode.Run,
            RuntimeLibraryEdit = true,
            RuntimeGraphEdit = false,
        };
    }

    private static NodeStyle CreateStyle()
    {
        return new NodeStyle
        {
            ShapeStyle =
            {
                BackgroundColor = new NodeColor(240, 240, 240),
                BorderColor = new NodeColor(32, 32, 32),
                BorderThickness = 1.5f,
                FontSize = 12.0f,
                FontColor = NodeColor.Black,
                FontFamily = null,
                FontWeight = NodeFontWeight.Normal,
            },
            OverrideRules =
            {
                AllowStyleOverrides = true,
                AllowBorderOverrides = true,
                AllowFontOverrides = true,
                AllowHeaderOverrides = true,
            },
        };
    }

    private static NodeLibrary CreateLocalLibrary()
    {
        var library = new NodeLibrary();
        library.Add(CreateDefinition());
        return library;
    }

    private static NodeLibrary CreateSharedLibrary()
    {
        var library = CreateLocalLibrary();
        library.Name = "Standard";
        library.Version = "1.0.0";
        return library;
    }

    private static NodeDefinition CreateDefinition()
    {
        var definition = new NodeDefinition
        {
            TypeName = "Server",
            DisplayName = "Server Node",
            DomainType = "Server",
            NodeShape = NodeShape.RoundedRectangle,
            IconSvg = "<svg viewBox=\"0 0 16 16\"><rect x=\"2\" y=\"3\" width=\"12\" height=\"10\" rx=\"2\" /></svg>",
            Style = new NodeStyle(),
            Components =
            [
                new NodeComponent
                {
                    Id = "displayName",
                    DisplayName = "Display Name",
                    TypeId = InputTypeIds.String,
                },
                new NodeComponent
                {
                    Id = "role",
                    DisplayName = "Role",
                    TypeId = InputTypeIds.StringSelection,
                },
            ],
        };

        definition.Style!.ShapeStyle.BackgroundColor = new NodeColor(7, 7, 255);
        definition.Style.ShapeStyle.BorderColor = new NodeColor(0, 0, 0);
        definition.Style.ShapeStyle.BorderThickness = 2.5f;
        definition.Style.ShapeStyle.FontSize = 14.0f;
        definition.Style.ShapeStyle.FontColor = NodeColor.White;
        definition.Style.ShapeStyle.FontFamily = null;
        definition.Style.ShapeStyle.FontWeight = NodeFontWeight.Bold;
        definition.Style.OverrideRules.AllowStyleOverrides = true;
        definition.Style.OverrideRules.AllowBorderOverrides = true;
        definition.Style.OverrideRules.AllowFontOverrides = false;
        definition.Style.OverrideRules.AllowHeaderOverrides = true;
        definition.Style.HeaderStyle = new NodeHeaderStyle
        {
            BackgroundColor = new NodeColor(10, 20, 30),
            FontColor = NodeColor.White,
            FontSize = 14.0f,
            FontFamily = null,
            FontWeight = NodeFontWeight.SemiBold,
        };

        definition.Components[0].SetDefaultValue("Primary Server");
        definition.Components[1].SetDefaultValue(new StringSelectionValue
        {
            Options = ["Web", "Database", "Cache"],
            Value = "Database",
        });

        return definition;
    }

    private static NodeGraph CreateGraph()
    {
        return new NodeGraph
        {
            Nodes =
            [
                new LeafNode
                {
                    Id = "server-1",
                    NodeDefinitionName = "Server",
                    Position = new NodePoint(100.0f, 200.0f),
                },
                new CompositeNode
                {
                    Id = "cluster-1",
                    NodeDefinitionName = "Server",
                    Position = new NodePoint(300.0f, 400.0f),
                    Size = new NodeSize(640.0f, 480.0f),
                    Children =
                    [
                        new LeafNode
                        {
                            Id = "server-2",
                            NodeDefinitionName = "Server",
                            Position = new NodePoint(10.0f, 20.0f),
                        },
                    ],
                },
            ],
        };
    }
}
