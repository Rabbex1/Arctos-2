namespace Arctos.Nodes.Core.Inputs;

/// <summary>
/// Creates portable node inputs using the shared input type registry.
/// </summary>
public static class InputFactory
{
    #region Public Methods

    /// <summary>
    /// Creates a node input using the shared built-in registry and the CLR type of the provided default value.
    /// </summary>
    /// <typeparam name="T">The CLR type of the input value.</typeparam>
    /// <param name="id">The stable input identifier.</param>
    /// <param name="displayName">The human-readable input display name.</param>
    /// <param name="value">The default value to store.</param>
    /// <returns>The created node input.</returns>
    public static NodeInput Create<T>(string id, string? displayName, T value)
    {
        return Create(id, displayName, value, InputTypeRegistry.Default);
    }

    /// <summary>
    /// Creates a node input using the provided registry and the CLR type of the provided default value.
    /// </summary>
    /// <typeparam name="T">The CLR type of the input value.</typeparam>
    /// <param name="id">The stable input identifier.</param>
    /// <param name="displayName">The human-readable input display name.</param>
    /// <param name="value">The default value to store.</param>
    /// <param name="registry">The registry used to resolve the input type.</param>
    /// <returns>The created node input.</returns>
    public static NodeInput Create<T>(string id, string? displayName, T value, InputTypeRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(registry);

        var valueType = typeof(T);
        if (!registry.TryGetValue(valueType, out var definition) || definition == null)
            throw new InvalidOperationException($"No registered input type was found for CLR type '{valueType.Name}'.");

        return Create(id, displayName, value, definition.Id, registry);
    }

    /// <summary>
    /// Creates a node input using an explicit registered input type identifier.
    /// </summary>
    /// <typeparam name="T">The CLR type of the input value.</typeparam>
    /// <param name="id">The stable input identifier.</param>
    /// <param name="displayName">The human-readable input display name.</param>
    /// <param name="value">The default value to store.</param>
    /// <param name="typeId">The explicit registered input type identifier.</param>
    /// <param name="registry">The registry used to validate and serialize the input.</param>
    /// <returns>The created node input.</returns>
    public static NodeInput Create<T>(string id, string? displayName, T value, InputTypeId typeId, InputTypeRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryGetValue(typeId, out var definition) || definition == null)
            throw new InvalidOperationException($"The input type '{typeId}' has not been registered.");

        var input = new NodeInput
        {
            Id = id,
            DisplayName = displayName,
            TypeId = typeId,
            Label = displayName ?? string.Empty,
        };

        input.SetDefaultValue(value, registry);
        return input;
    }

    #endregion Public Methods
}

