using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace Arctos.Nodes.Models;

/// <summary>
/// Describes a registered input component type, including its backing value type and serialization behavior.
/// </summary>
public sealed class InputTypeDefinition
{
    #region Fields

    private static readonly JsonSerializerOptions JsonOptions = new();

    private readonly Func<object, JsonElement>? _serializeValue;
    private readonly Func<JsonElement, object?>? _deserializeValue;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the stable input type identifier.
    /// </summary>
    public required InputTypeId Id { get; init; }

    /// <summary>
    /// Gets the human-readable input type display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the CLR value type represented by the input type.
    /// </summary>
    public required Type ValueType { get; init; }

    #endregion Properties

    #region Constructors

    private InputTypeDefinition()
    {
    }

    [SetsRequiredMembers]
    private InputTypeDefinition(InputTypeId id, string displayName, Type valueType, Func<object, JsonElement>? serializeValue, Func<JsonElement, object?>? deserializeValue)
    {
        Id = id;
        DisplayName = displayName;
        ValueType = valueType;
        _serializeValue = serializeValue;
        _deserializeValue = deserializeValue;
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Creates a new registered input type definition.
    /// </summary>
    /// <typeparam name="T">The CLR type represented by the input type.</typeparam>
    /// <param name="id">The stable input type identifier.</param>
    /// <param name="displayName">The human-readable display name.</param>
    /// <param name="serializeValue">An optional custom serializer for the CLR type.</param>
    /// <param name="deserializeValue">An optional custom deserializer for the CLR type.</param>
    /// <returns>The created type definition.</returns>
    public static InputTypeDefinition Create<T>(InputTypeId id, string displayName, Func<T, JsonElement>? serializeValue = null, Func<JsonElement, T>? deserializeValue = null)
    {
        return new InputTypeDefinition(
            id,
            displayName,
            typeof(T),
            serializeValue is null ? null : value => serializeValue((T)value),
            deserializeValue is null ? null : element => deserializeValue(element));
    }

    /// <summary>
    /// Serializes the provided value into a JSON element using the registered type behavior.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized JSON element.</returns>
    public JsonElement Serialize(object? value)
    {
        if (value is null)
            return JsonSerializer.SerializeToElement<object?>(null, JsonOptions);

        if (!ValueType.IsInstanceOfType(value))
            throw new ArgumentException($"The provided value must be assignable to {ValueType.Name}.", nameof(value));

        if (_serializeValue != null)
            return _serializeValue(value);

        return JsonSerializer.SerializeToElement(value, ValueType, JsonOptions);
    }

    /// <summary>
    /// Deserializes the provided JSON element into the registered CLR type.
    /// </summary>
    /// <param name="value">The JSON element to deserialize.</param>
    /// <returns>The deserialized CLR value.</returns>
    public object? Deserialize(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            return null;

        if (_deserializeValue != null)
            return _deserializeValue(value);

        return JsonSerializer.Deserialize(value.GetRawText(), ValueType, JsonOptions);
    }

    #endregion Public Methods
}
