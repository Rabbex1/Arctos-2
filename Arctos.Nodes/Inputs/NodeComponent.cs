using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arctos.Nodes.Models;

/// <summary>
/// Defines a portable node component that can later be edited by property panels, popups, or in-canvas widgets.
/// </summary>
public sealed class NodeComponent
{
    #region Properties

    /// <summary>
    /// Gets or sets the stable component identifier used by applications and runtime updates.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the human-readable component display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the optional component description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the registered input component type identifier.
    /// </summary>
    public required InputTypeId TypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is read-only from the editor's perspective.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is visible by default.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the optional logical group identifier used by property panels or future canvas layouts.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets the serialized default value payload for the component.
    /// </summary>
    [JsonInclude]
    public JsonElement? DefaultValue { get; private set; }

    #endregion Properties

    #region Public Methods

    /// <summary>
    /// Sets the default value using the shared built-in input type registry.
    /// </summary>
    /// <typeparam name="T">The CLR type of the default value.</typeparam>
    /// <param name="value">The default value to store.</param>
    public void SetDefaultValue<T>(T value)
    {
        SetDefaultValue(value, InputTypeRegistry.Default);
    }

    /// <summary>
    /// Sets the default value using the provided input type registry.
    /// </summary>
    /// <typeparam name="T">The CLR type of the default value.</typeparam>
    /// <param name="value">The default value to store.</param>
    /// <param name="registry">The registry used to validate and serialize the component value.</param>
    public void SetDefaultValue<T>(T value, InputTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryGetValue(TypeId, out var definition) || definition == null)
            throw new InvalidOperationException($"The input component type '{TypeId}' has not been registered.");

        DefaultValue = definition.Serialize(value);
    }

    /// <summary>
    /// Retrieves the default value using the shared built-in input type registry.
    /// </summary>
    /// <typeparam name="T">The CLR type to deserialize.</typeparam>
    /// <returns>The deserialized default value.</returns>
    public T? GetDefaultValue<T>()
    {
        return GetDefaultValue<T>(InputTypeRegistry.Default);
    }

    /// <summary>
    /// Retrieves the default value using the provided input type registry.
    /// </summary>
    /// <typeparam name="T">The CLR type to deserialize.</typeparam>
    /// <param name="registry">The registry used to deserialize the stored component value.</param>
    /// <returns>The deserialized default value.</returns>
    public T? GetDefaultValue<T>(InputTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var value = GetDefaultValue(registry);
        if (value is null)
            return default;

        if (value is T typedValue)
            return typedValue;

        throw new InvalidOperationException($"The component '{Id}' stores a value of type '{value.GetType().Name}', not '{typeof(T).Name}'.");
    }

    /// <summary>
    /// Retrieves the default value as an untyped CLR object using the provided registry.
    /// </summary>
    /// <param name="registry">The registry used to deserialize the stored component value.</param>
    /// <returns>The deserialized default value as an object.</returns>
    public object? GetDefaultValue(InputTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (DefaultValue is null)
            return null;

        if (!registry.TryGetValue(TypeId, out var definition) || definition == null)
            throw new InvalidOperationException($"The input component type '{TypeId}' has not been registered.");

        return definition.Deserialize(DefaultValue.Value);
    }

    #endregion Public Methods
}
