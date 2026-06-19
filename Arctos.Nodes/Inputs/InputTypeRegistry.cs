using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Arctos.Nodes.Models;

/// <summary>
/// Stores the known input component types available to graph documents and applications.
/// </summary>
public sealed class InputTypeRegistry : IEnumerable<InputTypeDefinition>
{
    #region Fields

    private readonly Dictionary<InputTypeId, InputTypeDefinition> _definitions = [];

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the shared registry that contains the built-in input component types.
    /// </summary>
    public static InputTypeRegistry Default { get; } = new();

    /// <summary>
    /// Gets the registered type definition for the provided identifier.
    /// </summary>
    /// <param name="typeId">The input type identifier.</param>
    public InputTypeDefinition this[InputTypeId typeId] => _definitions[typeId];

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new input type registry.
    /// </summary>
    /// <param name="includeBuiltInTypes">
    /// When <see langword="true"/>, the built-in input component types are registered automatically.
    /// </param>
    public InputTypeRegistry(bool includeBuiltInTypes = true)
    {
        if (includeBuiltInTypes)
            RegisterBuiltInTypes();
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Registers a new input type definition.
    /// </summary>
    /// <param name="definition">The type definition to register.</param>
    public void Register(InputTypeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_definitions.ContainsKey(definition.Id))
            throw new ArgumentException($"The input type '{definition.Id}' is already registered.", nameof(definition));

        _definitions.Add(definition.Id, definition);
    }

    /// <summary>
    /// Determines whether the provided input type identifier has been registered.
    /// </summary>
    /// <param name="typeId">The type identifier to check.</param>
    /// <returns><see langword="true"/> when the identifier is registered; otherwise <see langword="false"/>.</returns>
    public bool Contains(InputTypeId typeId)
    {
        return _definitions.ContainsKey(typeId);
    }

    /// <summary>
    /// Attempts to retrieve the type definition for the provided identifier.
    /// </summary>
    /// <param name="typeId">The type identifier to resolve.</param>
    /// <param name="definition">The resolved type definition when found.</param>
    /// <returns><see langword="true"/> when the identifier is registered; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(InputTypeId typeId, out InputTypeDefinition? definition)
    {
        return _definitions.TryGetValue(typeId, out definition);
    }

    /// <summary>
    /// Attempts to retrieve the registered input type definition for the provided CLR value type.
    /// </summary>
    /// <param name="valueType">The CLR value type to resolve.</param>
    /// <param name="definition">The resolved type definition when found.</param>
    /// <returns><see langword="true"/> when a matching type definition is registered; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(Type valueType, out InputTypeDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(valueType);

        definition = _definitions.Values.FirstOrDefault(item => item.ValueType == valueType);
        return definition != null;
    }

    /// <summary>
    /// Returns the registered input type definitions.
    /// </summary>
    /// <returns>The registered input type definitions.</returns>
    public IEnumerator<InputTypeDefinition> GetEnumerator()
    {
        return _definitions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion Public Methods

    #region Private Methods

    private void RegisterBuiltInTypes()
    {
        Register(InputTypeDefinition.Create<bool>(InputTypeIds.Boolean, "Boolean"));
        Register(InputTypeDefinition.Create<int>(InputTypeIds.Integer, "Integer"));
        Register(InputTypeDefinition.Create<uint>(InputTypeIds.UnsignedInteger, "Unsigned Integer"));
        //Register(InputTypeDefinition.Create<float>(InputTypeIds.Float, "Float"));
        Register(InputTypeDefinition.Create<double>(InputTypeIds.Double, "Numeric"));
        Register(InputTypeDefinition.Create<string>(InputTypeIds.String, "String"));
        Register(InputTypeDefinition.Create<DateTime>(InputTypeIds.DateTime, "Date Time"));
        Register(InputTypeDefinition.Create<TimeSpan>(InputTypeIds.TimeSpan, "Time Span"));
        //Register(InputTypeDefinition.Create<NodeColor>(InputTypeIds.Color, "Color"));
        Register(InputTypeDefinition.Create<StringSelectionValue>(InputTypeIds.StringSelection, "String Selection"));
        //Register(InputTypeDefinition.Create<FloatRangeValue>(InputTypeIds.FloatRange, "Float Range"));
        //Register(InputTypeDefinition.Create<IntRangeValue>(InputTypeIds.IntRange, "Int Range"));
    }

    #endregion Private Methods
}
