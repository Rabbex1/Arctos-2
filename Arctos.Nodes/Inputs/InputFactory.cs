namespace Arctos.Nodes.Models;

/// <summary>
/// Creates portable node components using the shared input type registry.
/// </summary>
public static class InputFactory
{
    #region Public Methods

    /// <summary>
    /// Creates an input component using the shared built-in registry and the CLR type of the provided default value.
    /// </summary>
    /// <typeparam name="T">The CLR type of the input component value.</typeparam>
    /// <param name="id">The stable component identifier.</param>
    /// <param name="displayName">The human-readable component display name.</param>
    /// <param name="value">The default value to store.</param>
    /// <returns>The created node component.</returns>
    public static NodeComponent Create<T>(string id, string? displayName, T value)
    {
        return Create(id, displayName, value, InputTypeRegistry.Default);
    }

    /// <summary>
    /// Creates an input component using the provided registry and the CLR type of the provided default value.
    /// </summary>
    /// <typeparam name="T">The CLR type of the input component value.</typeparam>
    /// <param name="id">The stable component identifier.</param>
    /// <param name="displayName">The human-readable component display name.</param>
    /// <param name="value">The default value to store.</param>
    /// <param name="registry">The registry used to resolve the input component type.</param>
    /// <returns>The created node component.</returns>
    public static NodeComponent Create<T>(string id, string? displayName, T value, InputTypeRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(registry);

        var valueType = typeof(T);
        if (!registry.TryGetValue(valueType, out var definition) || definition == null)
            throw new InvalidOperationException($"No registered input component type was found for CLR type '{valueType.Name}'.");

        return Create(id, displayName, value, definition.Id, registry);
    }

    /// <summary>
    /// Creates an input component using an explicit registered input type identifier.
    /// </summary>
    /// <typeparam name="T">The CLR type of the input component value.</typeparam>
    /// <param name="id">The stable component identifier.</param>
    /// <param name="displayName">The human-readable component display name.</param>
    /// <param name="value">The default value to store.</param>
    /// <param name="typeId">The explicit registered input type identifier.</param>
    /// <param name="registry">The registry used to validate and serialize the component.</param>
    /// <returns>The created node component.</returns>
    public static NodeComponent Create<T>(string id, string? displayName, T value, InputTypeId typeId, InputTypeRegistry registry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryGetValue(typeId, out var definition) || definition == null)
            throw new InvalidOperationException($"The input component type '{typeId}' has not been registered.");

        var component = new NodeComponent
        {
            Id = id,
            DisplayName = displayName,
            TypeId = typeId,
        };

        component.SetDefaultValue(value, registry);
        return component;
    }

    #endregion Public Methods
}
