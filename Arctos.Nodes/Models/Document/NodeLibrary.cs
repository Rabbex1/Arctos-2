using System.Collections;
using System.Collections.Generic;

namespace Arctos.Nodes.Models;

/// <summary>
/// Represents the node library used by a graph document.
/// </summary>
public sealed class NodeLibrary : IEnumerable<NodeDefinition>
{
    #region Fields

    private readonly Dictionary<string, NodeDefinition> _definitions = new(StringComparer.Ordinal);

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the number of node definitions in the library.
    /// </summary>
    public int Count => _definitions.Count;

    /// <summary>
    /// Gets the node definitions stored in the library.
    /// </summary>
    public IEnumerable<NodeDefinition> Definitions => _definitions.Values;

    /// <summary>
    /// Gets or sets the node definition stored under the provided type name.
    /// </summary>
    /// <param name="typeName">The node type name.</param>
    /// <returns>The stored node definition.</returns>
    public NodeDefinition this[string typeName]
    {
        get => _definitions[typeName];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
            ArgumentNullException.ThrowIfNull(value);

            if (!string.Equals(typeName, value.TypeName, StringComparison.Ordinal))
                throw new ArgumentException("The library key must match the definition type name.", nameof(typeName));

            _definitions[typeName] = value;
        }
    }

    #endregion Properties

    #region Public Methods

    /// <summary>
    /// Adds a node definition to the library.
    /// </summary>
    /// <param name="definition">The node definition to add.</param>
    public void Add(NodeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _definitions.Add(definition.TypeName, definition);
    }

    /// <summary>
    /// Removes the node definition with the provided type name.
    /// </summary>
    /// <param name="typeName">The node type name to remove.</param>
    /// <returns><see langword="true"/> when the node definition was removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        return _definitions.Remove(typeName);
    }

    /// <summary>
    /// Determines whether the provided type name exists in the dictionary.
    /// </summary>
    /// <param name="typeName">The node type name to look for.</param>
    /// <returns><see langword="true"/> when the type exists; otherwise, <see langword="false"/>.</returns>
    public bool Contains(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        return _definitions.ContainsKey(typeName);
    }

    /// <summary>
    /// Attempts to retrieve a node definition from the library.
    /// </summary>
    /// <param name="typeName">The node type name to retrieve.</param>
    /// <param name="definition">The resulting node definition when found.</param>
    /// <returns><see langword="true"/> when the node definition exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(string typeName, out NodeDefinition? definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        return _definitions.TryGetValue(typeName, out definition);
    }

    /// <summary>
    /// Removes all node definitions from the library.
    /// </summary>
    public void Clear()
    {
        _definitions.Clear();
    }

    /// <inheritdoc />
    public IEnumerator<NodeDefinition> GetEnumerator()
    {
        return _definitions.Values.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion Public Methods
}
