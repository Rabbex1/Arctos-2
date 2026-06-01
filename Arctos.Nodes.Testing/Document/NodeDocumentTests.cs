using Arctos.Nodes.Models;
using Xunit;

namespace Arctos.Nodes.Testing.Document;

public sealed class NodeDocumentTests
{
    [Fact]
    public void Constructor_Should_Store_Provided_Settings_And_NodeLibrary()
    {
        var settings = new NodeDocumentSettings { Name = "Test Graph", DocumentType = "Routine" };
        var nodeLibrary = new NodeLibrary();
        var nodeGraph = new NodeGraph();

        var document = new NodeDocument(settings, nodeLibrary, nodeGraph);

        Assert.Same(settings, document.Settings);
        Assert.Same(nodeLibrary, document.NodeLibrary);
        Assert.Same(nodeGraph, document.NodeGraph);
        Assert.True(document.Settings.ContainsGraph);
        Assert.NotEqual(Guid.Empty, document.Id);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Are_Null()
    {
        var nodeLibrary = new NodeLibrary();

        Assert.Throws<ArgumentNullException>(() => new NodeDocument(null!, nodeLibrary));
    }

    [Fact]
    public void Constructor_Should_Throw_When_NodeLibrary_Are_Null()
    {
        var settings = new NodeDocumentSettings();

        Assert.Throws<ArgumentNullException>(() => new NodeDocument(settings, null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_Settings_Require_Graph_But_Graph_Is_Not_Provided()
    {
        var settings = new NodeDocumentSettings { ContainsGraph = true };
        var nodeLibrary = new NodeLibrary();

        Assert.Throws<ArgumentException>(() => new NodeDocument(settings, nodeLibrary));
    }

    [Fact]
    public void Save_And_Load_Should_RoundTrip_Document_Settings_NodeLibrary_And_NodeGraph()
    {
        var settings = new NodeDocumentSettings
        {
            FormatVersion = 1,
            ContainsGraph = true,
            CanvasColor = new NodeColor(24, 28, 32),
            Name = "Network Map",
            DocumentType = "NetMap",
            StartupMode = OperationMode.Run,
            AllowLibraryEdit = true,
            AllowGraphEdit = false,
        };

        var nodeLibrary = new NodeLibrary
        {
            ["Server"] = new NodeDefinition
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
            },
        };

        var documentStyle = new NodeStyle();
        documentStyle.ShapeStyle.BackgroundColor = new NodeColor(240, 240, 240);
        documentStyle.ShapeStyle.BorderColor = new NodeColor(32, 32, 32);
        documentStyle.ShapeStyle.BorderThickness = 1.5f;
        documentStyle.ShapeStyle.FontSize = 12.0f;
        documentStyle.ShapeStyle.FontColor = NodeColor.Black;
        documentStyle.ShapeStyle.FontFamily = null;
        documentStyle.ShapeStyle.FontWeight = NodeFontWeight.Normal;
        documentStyle.OverrideRules.AllowStyleOverrides = true;
        documentStyle.OverrideRules.AllowBorderOverrides = true;
        documentStyle.OverrideRules.AllowFontOverrides = true;
        documentStyle.OverrideRules.AllowHeaderOverrides = true;

        nodeLibrary["Server"].Style!.ShapeStyle.BackgroundColor = new NodeColor(7, 7, 255);
        nodeLibrary["Server"].Style!.ShapeStyle.BorderColor = new NodeColor(0, 0, 0);
        nodeLibrary["Server"].Style!.ShapeStyle.BorderThickness = 2.5f;
        nodeLibrary["Server"].Style!.ShapeStyle.FontSize = 14.0f;
        nodeLibrary["Server"].Style!.ShapeStyle.FontColor = NodeColor.White;
        nodeLibrary["Server"].Style!.ShapeStyle.FontFamily = null;
        nodeLibrary["Server"].Style!.ShapeStyle.FontWeight = NodeFontWeight.Bold;
        nodeLibrary["Server"].Style!.OverrideRules.AllowStyleOverrides = true;
        nodeLibrary["Server"].Style!.OverrideRules.AllowBorderOverrides = true;
        nodeLibrary["Server"].Style!.OverrideRules.AllowFontOverrides = false;
        nodeLibrary["Server"].Style!.OverrideRules.AllowHeaderOverrides = true;
        nodeLibrary["Server"].Style!.HeaderStyle = new NodeHeaderStyle
        {
            BackgroundColor = new NodeColor(10, 20, 30),
            FontColor = NodeColor.White,
            FontSize = 14.0f,
            FontFamily = null,
            FontWeight = NodeFontWeight.SemiBold,
        };
        nodeLibrary["Server"].Components[0].SetDefaultValue("Primary Server");
        nodeLibrary["Server"].Components[1].SetDefaultValue(new StringSelectionValue
        {
            Options = ["Web", "Database", "Cache"],
            Value = "Database",
        });

        var nodeGraph = new NodeGraph
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

        var document = new NodeDocument(settings, nodeLibrary, nodeGraph)
        {
            Style = documentStyle,
        };
        var manager = new NodesDocumentManager();
        manager.Add(document);

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.arctos-nodes.json");

        try
        {
            manager.Save(document, path);

            var loadManager = new NodesDocumentManager();
            var loaded = loadManager.LoadAndAdd(path);

            Assert.Equal(1, loaded.Settings.FormatVersion);
            Assert.Equal(document.Id, loaded.Id);
            Assert.True(loaded.Settings.ContainsGraph);
            Assert.Equal(new NodeColor(24, 28, 32), loaded.Settings.CanvasColor);
            Assert.Equal("Network Map", loaded.Settings.Name);
            Assert.Equal("NetMap", loaded.Settings.DocumentType);
            Assert.Equal(OperationMode.Run, loaded.Settings.StartupMode);
            Assert.True(loaded.Settings.AllowLibraryEdit);
            Assert.False(loaded.Settings.AllowGraphEdit);
            Assert.Equal(new NodeColor(240, 240, 240), loaded.Style.ShapeStyle.BackgroundColor);
            Assert.Equal(new NodeColor(32, 32, 32), loaded.Style.ShapeStyle.BorderColor);
            Assert.Equal(1.5f, loaded.Style.ShapeStyle.BorderThickness);
            Assert.Equal(12.0f, loaded.Style.ShapeStyle.FontSize);
            Assert.Equal(NodeColor.Black, loaded.Style.ShapeStyle.FontColor);
            Assert.Null(loaded.Style.ShapeStyle.FontFamily);
            Assert.Equal(NodeFontWeight.Normal, loaded.Style.ShapeStyle.FontWeight);
            Assert.True(loaded.Style.OverrideRules.AllowStyleOverrides);
            Assert.True(loaded.Style.OverrideRules.AllowBorderOverrides);
            Assert.True(loaded.Style.OverrideRules.AllowFontOverrides);
            Assert.True(loaded.Style.OverrideRules.AllowHeaderOverrides);

            Assert.True(loaded.NodeLibrary.Contains("Server"));

            var loadedType = loaded.NodeLibrary["Server"];
            Assert.Equal("Server", loadedType.TypeName);
            Assert.Equal("Server Node", loadedType.DisplayName);
            Assert.Equal("Server", loadedType.DomainType);
            Assert.Equal(NodeShape.RoundedRectangle, loadedType.NodeShape);
            Assert.Equal("<svg viewBox=\"0 0 16 16\"><rect x=\"2\" y=\"3\" width=\"12\" height=\"10\" rx=\"2\" /></svg>", loadedType.IconSvg);
            Assert.NotNull(loadedType.Style);
            Assert.Equal(new NodeColor(7, 7, 255), loadedType.Style!.ShapeStyle.BackgroundColor);
            Assert.Equal(new NodeColor(0, 0, 0), loadedType.Style.ShapeStyle.BorderColor);
            Assert.Equal(2.5f, loadedType.Style.ShapeStyle.BorderThickness);
            Assert.Equal(14.0f, loadedType.Style.ShapeStyle.FontSize);
            Assert.Equal(NodeColor.White, loadedType.Style.ShapeStyle.FontColor);
            Assert.Null(loadedType.Style.ShapeStyle.FontFamily);
            Assert.Equal(NodeFontWeight.Bold, loadedType.Style.ShapeStyle.FontWeight);
            Assert.True(loadedType.Style.OverrideRules.AllowStyleOverrides);
            Assert.True(loadedType.Style.OverrideRules.AllowBorderOverrides);
            Assert.False(loadedType.Style.OverrideRules.AllowFontOverrides);
            Assert.True(loadedType.Style.OverrideRules.AllowHeaderOverrides);
            Assert.NotNull(loadedType.Style.HeaderStyle);
            Assert.Equal(new NodeColor(10, 20, 30), loadedType.Style.HeaderStyle!.BackgroundColor);
            Assert.Equal(NodeColor.White, loadedType.Style.HeaderStyle.FontColor);
            Assert.Equal(14.0f, loadedType.Style.HeaderStyle.FontSize);
            Assert.Null(loadedType.Style.HeaderStyle.FontFamily);
            Assert.Equal(NodeFontWeight.SemiBold, loadedType.Style.HeaderStyle.FontWeight);
            Assert.Collection(
                loadedType.Components,
                component =>
                {
                    Assert.Equal("displayName", component.Id);
                    Assert.Equal("Display Name", component.DisplayName);
                    Assert.Equal(InputTypeIds.String, component.TypeId);
                    Assert.Equal("Primary Server", component.GetDefaultValue<string>());
                },
                component =>
                {
                    Assert.Equal("role", component.Id);
                    Assert.Equal("Role", component.DisplayName);
                    Assert.Equal(InputTypeIds.StringSelection, component.TypeId);

                    var value = component.GetDefaultValue<StringSelectionValue>();
                    Assert.NotNull(value);
                    Assert.Equal("Database", value!.Value);
                    Assert.Equal(["Web", "Database", "Cache"], value.Options);
                });

            Assert.NotNull(loaded.NodeGraph);
            Assert.Collection(
                loaded.NodeGraph!.Nodes,
                node =>
                {
                    var leaf = Assert.IsType<LeafNode>(node);
                    Assert.Equal("server-1", leaf.Id);
                    Assert.Equal("Server", leaf.NodeDefinitionName);
                    Assert.Equal(new NodePoint(100.0f, 200.0f), leaf.Position);
                    Assert.Null(leaf.Size);
                },
                node =>
                {
                    var composite = Assert.IsType<CompositeNode>(node);
                    Assert.Equal("cluster-1", composite.Id);
                    Assert.Equal("Server", composite.NodeDefinitionName);
                    Assert.Equal(new NodePoint(300.0f, 400.0f), composite.Position);
                    Assert.Equal(new NodeSize(640.0f, 480.0f), composite.Size);

                    Assert.Collection(
                        composite.Children,
                        child =>
                        {
                            var leaf = Assert.IsType<LeafNode>(child);
                            Assert.Equal("server-2", leaf.Id);
                            Assert.Equal("Server", leaf.NodeDefinitionName);
                            Assert.Equal(new NodePoint(10.0f, 20.0f), leaf.Position);
                        });
                });
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
