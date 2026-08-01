using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Serializes a <see cref="NodeLibrary"/> as a named object rather than as a bare collection.
/// </summary>
public sealed class NodeLibraryJsonConverter : JsonConverter<NodeLibrary>
{
    private sealed class SerializableNodeLibrary
    {
        public string? Name { get; set; }

        public string? Version { get; set; }

        [JsonPropertyName("definitions")]
        public List<NodeDefinition> Definitions { get; set; } = [];
    }

    public override NodeLibrary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var serializable = JsonSerializer.Deserialize<SerializableNodeLibrary>(ref reader, options)
                           ?? throw new JsonException("The node library payload could not be deserialized.");

        var library = new NodeLibrary
        {
            Name = serializable.Name,
            Version = serializable.Version,
        };

        foreach (var definition in serializable.Definitions)
            library.Add(definition);

        return library;
    }

    public override void Write(Utf8JsonWriter writer, NodeLibrary value, JsonSerializerOptions options)
    {
        var serializable = new SerializableNodeLibrary
        {
            Name = value.Name,
            Version = value.Version,
            Definitions = value.Definitions,
        };

        JsonSerializer.Serialize(writer, serializable, options);
    }
}

