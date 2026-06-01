using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the stable identifier of a registered input component type.
/// </summary>
[JsonConverter(typeof(InputTypeIdJsonConverter))]
public readonly record struct InputTypeId(string Value)
{
    #region Public Methods

    /// <summary>
    /// Returns the input type identifier value.
    /// </summary>
    /// <returns>The string value of the identifier.</returns>
    public override string ToString()
    {
        return Value;
    }

    #endregion Public Methods
}

internal sealed class InputTypeIdJsonConverter : JsonConverter<InputTypeId>
{
    #region Public Methods

    public override InputTypeId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Input type identifiers cannot be null or whitespace.");

        return new InputTypeId(value);
    }

    public override void Write(Utf8JsonWriter writer, InputTypeId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }

    #endregion Public Methods
}
