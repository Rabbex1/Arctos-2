using Arctos.Nodes.Models;
using Xunit;

namespace Arctos.Nodes.Testing.Inputs;

public sealed class NodeComponentTests
{
    [Fact]
    public void DefaultRegistry_Should_Contain_BuiltIn_Types()
    {
        var registry = new InputTypeRegistry();

        Assert.True(registry.Contains(InputTypeIds.Boolean));
        Assert.True(registry.Contains(InputTypeIds.String));
        Assert.True(registry.Contains(InputTypeIds.Color));
        Assert.True(registry.Contains(InputTypeIds.StringSelection));
        Assert.True(registry.Contains(InputTypeIds.FloatRange));
    }

    [Fact]
    public void Register_Should_Store_Custom_Type()
    {
        var registry = new InputTypeRegistry();
        var typeId = new InputTypeId("Direction");

        registry.Register(InputTypeDefinition.Create<DirectionValue>(typeId, "Direction"));

        Assert.True(registry.Contains(typeId));
        Assert.Equal(typeof(DirectionValue), registry[typeId].ValueType);
    }

    [Fact]
    public void SetDefaultValue_And_GetDefaultValue_Should_RoundTrip_BuiltIn_Value()
    {
        var component = new NodeComponent
        {
            Id = "role",
            DisplayName = "Role",
            TypeId = InputTypeIds.StringSelection,
        };

        component.SetDefaultValue(new StringSelectionValue
        {
            Options = ["Web", "Database"],
            Value = "Web",
        });

        var value = component.GetDefaultValue<StringSelectionValue>();

        Assert.NotNull(value);
        Assert.Equal("Web", value!.Value);
        Assert.Equal(["Web", "Database"], value.Options);
    }

    [Fact]
    public void SetDefaultValue_And_GetDefaultValue_Should_Support_Custom_Registered_Type()
    {
        var registry = new InputTypeRegistry();
        var typeId = new InputTypeId("Direction");
        registry.Register(InputTypeDefinition.Create<DirectionValue>(typeId, "Direction"));

        var component = new NodeComponent
        {
            Id = "direction",
            DisplayName = "Direction",
            TypeId = typeId,
        };

        component.SetDefaultValue(new DirectionValue(1.0f, 2.0f, 3.0f), registry);

        var value = component.GetDefaultValue<DirectionValue>(registry);

        Assert.Equal(new DirectionValue(1.0f, 2.0f, 3.0f), value);
    }

    [Fact]
    public void SetDefaultValue_Should_Throw_When_TypeId_Is_Not_Registered()
    {
        var registry = new InputTypeRegistry(includeBuiltInTypes: false);
        var component = new NodeComponent
        {
            Id = "direction",
            TypeId = new InputTypeId("Direction"),
        };

        Assert.Throws<InvalidOperationException>(() => component.SetDefaultValue(new DirectionValue(1.0f, 2.0f, 3.0f), registry));
    }

    [Fact]
    public void Create_Should_Build_Component_From_BuiltIn_Type()
    {
        var component = InputFactory.Create("count", "Count", 7);

        Assert.Equal("count", component.Id);
        Assert.Equal("Count", component.DisplayName);
        Assert.Equal(InputTypeIds.Integer, component.TypeId);
        Assert.Equal(7, component.GetDefaultValue<int>());
    }

    [Fact]
    public void Create_Should_Build_Component_From_Custom_Registered_Type()
    {
        var registry = new InputTypeRegistry();
        var typeId = new InputTypeId("Direction");
        registry.Register(InputTypeDefinition.Create<DirectionValue>(typeId, "Direction"));

        var component = InputFactory.Create("direction", "Direction", new DirectionValue(1.0f, 2.0f, 3.0f), typeId, registry);

        Assert.Equal(typeId, component.TypeId);
        Assert.Equal(new DirectionValue(1.0f, 2.0f, 3.0f), component.GetDefaultValue<DirectionValue>(registry));
    }

    [Fact]
    public void Create_Should_Throw_When_No_Registered_Type_Matches_The_Value_Type()
    {
        var registry = new InputTypeRegistry(includeBuiltInTypes: false);

        Assert.Throws<InvalidOperationException>(() => InputFactory.Create("direction", "Direction", new DirectionValue(1.0f, 2.0f, 3.0f), registry));
    }

    private sealed record DirectionValue(float X, float Y, float Z);
}
