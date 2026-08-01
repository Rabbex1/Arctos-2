using Arctos.Nodes.Models;
using Xunit;

namespace Arctos.Nodes.Testing.Document;

public sealed class NodeGraphTests
{
    [Fact]
    public void CompositeNode_Should_Inherit_From_LeafNode()
    {
        LeafNode node = new CompositeNode
        {
            Id = "composite-1",
            NodeDefinitionName = "Group",
        };

        Assert.IsType<CompositeNode>(node);
    }

    [Fact]
    public void CompositeNode_Should_Store_Child_Nodes()
    {
        var composite = new CompositeNode
        {
            Id = "group-1",
            NodeDefinitionName = "Group",
            Position = new NodePoint(25.0f, 50.0f),
            Children =
            [
                new LeafNode
                {
                    Id = "leaf-1",
                    NodeDefinitionName = "Server",
                    Position = new NodePoint(10.0f, 15.0f),
                },
            ],
        };

        Assert.Single(composite.Children);
        Assert.Equal("leaf-1", composite.Children[0].Id);
    }
}
