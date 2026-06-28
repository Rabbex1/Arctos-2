using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arctos.Nodes.Models;

/// <summary>
/// Defines a portable input component that can later be edited by property panels, popups, or in-canvas widgets.
/// </summary>
public sealed class NodeInput : NodeComponent
{
    #region Properties

    /// <summary>
    /// Gets or sets the human-readable input display name.
    /// </summary>
    public string? DisplayName
    {
        get => string.IsNullOrWhiteSpace(Label) ? null : Label;
        set => Label = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the optional input description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the registered input type identifier.
    /// </summary>
    public required InputTypeId TypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the input is read-only from the editor's perspective.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the input is visible by default.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the optional logical group identifier used by property panels or future canvas layouts.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets how DateTime inputs are displayed when rendered in passive visuals.
    /// </summary>
    public DateTimeDisplayMode DateTimeDisplayMode { get; set; } = DateTimeDisplayMode.Both;

    /// <summary>
    /// Gets the serialized default value payload for the input.
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
    /// <param name="registry">The registry used to validate and serialize the input value.</param>
    public void SetDefaultValue<T>(T value, InputTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryGetValue(TypeId, out var definition) || definition == null)
            throw new InvalidOperationException($"The input type '{TypeId}' has not been registered.");

        DefaultValue = definition.Serialize(value);
    }

    /// <summary>
    /// Sets the default value using an untyped CLR object and the provided input type registry.
    /// </summary>
    /// <param name="value">The default value to store.</param>
    /// <param name="registry">The registry used to validate and serialize the input value.</param>
    public void SetDefaultValue(object? value, InputTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryGetValue(TypeId, out var definition) || definition == null)
            throw new InvalidOperationException($"The input type '{TypeId}' has not been registered.");

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
    /// <param name="registry">The registry used to deserialize the stored input value.</param>
    /// <returns>The deserialized default value.</returns>
    public T? GetDefaultValue<T>(InputTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var value = GetDefaultValue(registry);
        if (value is null)
            return default;

        if (value is T typedValue)
            return typedValue;

        throw new InvalidOperationException($"The input '{Id}' stores a value of type '{value.GetType().Name}', not '{typeof(T).Name}'.");
    }

    /// <summary>
    /// Retrieves the default value as an untyped CLR object using the provided registry.
    /// </summary>
    /// <param name="registry">The registry used to deserialize the stored input value.</param>
    /// <returns>The deserialized default value as an object.</returns>
    public object? GetDefaultValue(InputTypeRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (DefaultValue is null)
            return null;

        if (!registry.TryGetValue(TypeId, out var definition) || definition == null)
            throw new InvalidOperationException($"The input type '{TypeId}' has not been registered.");

        return definition.Deserialize(DefaultValue.Value);
    }

    #endregion Public Methods
}
