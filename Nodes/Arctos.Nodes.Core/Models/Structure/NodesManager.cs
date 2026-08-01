using System.Collections.ObjectModel;
using System.Text.Json;

namespace Arctos.Nodes.Core.Models;

/// <summary>
/// Manages the node maps and shared libraries owned by the current application session.
/// </summary>
public sealed class NodesManager
{
    #region Fields

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly List<NodeMap> _maps = [];
    private readonly ReadOnlyCollection<NodeMap> _readOnlyMaps;
    private readonly List<NodeLibrary> _libraries = [];
    private readonly ReadOnlyCollection<NodeLibrary> _readOnlyLibraries;

    #endregion Fields

    #region Events

    /// <summary>
    /// Raised when a node map is added to the manager.
    /// </summary>
    public event EventHandler<NodeMapEventArgs>? MapAdded;

    /// <summary>
    /// Raised when a node map is removed from the manager.
    /// </summary>
    public event EventHandler<NodeMapEventArgs>? MapRemoved;

    /// <summary>
    /// Raised when a shared node library is added to the manager.
    /// </summary>
    public event EventHandler<NodeLibraryEventArgs>? LibraryAdded;

    /// <summary>
    /// Raised when a shared node library is removed from the manager.
    /// </summary>
    public event EventHandler<NodeLibraryEventArgs>? LibraryRemoved;

    #endregion Events

    #region Properties

    /// <summary>
    /// Gets the node maps currently owned by the manager.
    /// </summary>
    public IReadOnlyList<NodeMap> Maps => _readOnlyMaps;

    /// <summary>
    /// Gets the shared node libraries currently owned by the manager.
    /// </summary>
    public IReadOnlyList<NodeLibrary> Libraries => _readOnlyLibraries;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new nodes manager.
    /// </summary>
    public NodesManager()
    {
        _readOnlyMaps = _maps.AsReadOnly();
        _readOnlyLibraries = _libraries.AsReadOnly();
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Adds a node map to the manager.
    /// </summary>
    public void AddMap(NodeMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (_maps.Any(item => item.Id == map.Id))
            throw new InvalidOperationException($"A node map with id '{map.Id}' has already been added.");

        _maps.Add(map);
        MapAdded?.Invoke(this, new NodeMapEventArgs(map));
    }

    /// <summary>
    /// Removes a node map from the manager.
    /// </summary>
    public bool RemoveMap(NodeMap map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return RemoveMap(map.Id);
    }

    /// <summary>
    /// Removes a node map from the manager by identifier.
    /// </summary>
    public bool RemoveMap(Guid mapId)
    {
        var map = _maps.FirstOrDefault(item => item.Id == mapId);
        if (map == null)
            return false;

        _maps.Remove(map);
        MapRemoved?.Invoke(this, new NodeMapEventArgs(map));
        return true;
    }

    /// <summary>
    /// Adds a shared node library to the manager.
    /// </summary>
    public void AddLibrary(NodeLibrary library)
    {
        ArgumentNullException.ThrowIfNull(library);

        if (_libraries.Contains(library))
            throw new InvalidOperationException("The provided node library has already been added.");

        if (HasLibraryIdentity(library))
        {
            if (_libraries.Any(item => HasSameLibraryIdentity(item, library)))
            {
                throw new InvalidOperationException(
                    $"A node library named '{library.Name}' version '{library.Version}' has already been added.");
            }
        }

        _libraries.Add(library);
        LibraryAdded?.Invoke(this, new NodeLibraryEventArgs(library));
    }

    /// <summary>
    /// Removes a shared node library from the manager.
    /// </summary>
    public bool RemoveLibrary(NodeLibrary library)
    {
        ArgumentNullException.ThrowIfNull(library);

        if (!_libraries.Remove(library))
            return false;

        LibraryRemoved?.Invoke(this, new NodeLibraryEventArgs(library));
        return true;
    }

    /// <summary>
    /// Removes a shared node library from the manager by name and version.
    /// </summary>
    public bool RemoveLibrary(string name, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        var library = _libraries.FirstOrDefault(item =>
            string.Equals(item.Name, name, StringComparison.Ordinal) &&
            string.Equals(item.Version, version, StringComparison.Ordinal));

        return library != null && RemoveLibrary(library);
    }

    /// <summary>
    /// Attempts to resolve a shared node library by name and version.
    /// </summary>
    public bool TryResolveLibrary(string name, string version, out NodeLibrary? library)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        library = _libraries.FirstOrDefault(item =>
            string.Equals(item.Name, name, StringComparison.Ordinal) &&
            string.Equals(item.Version, version, StringComparison.Ordinal));

        return library != null;
    }

    /// <summary>
    /// Removes all maps and libraries from the manager.
    /// </summary>
    public void Clear()
    {
        ClearMaps();
        ClearLibraries();
    }

    /// <summary>
    /// Removes all maps from the manager.
    /// </summary>
    public void ClearMaps()
    {
        foreach (var map in _maps.ToArray())
            RemoveMap(map.Id);
    }

    /// <summary>
    /// Removes all shared libraries from the manager.
    /// </summary>
    public void ClearLibraries()
    {
        foreach (var library in _libraries.ToArray())
            RemoveLibrary(library);
    }

    /// <summary>
    /// Loads a node map from a stream and adds it to the manager.
    /// </summary>
    public NodeMap LoadMapAndAdd(Stream stream)
    {
        var map = ReadStream<NodeMap>(stream);
        AddMap(map);
        return map;
    }

    /// <summary>
    /// Loads a shared node library from a stream and adds it to the manager.
    /// </summary>
    public NodeLibrary LoadLibraryAndAdd(Stream stream)
    {
        var library = ReadStream<NodeLibrary>(stream);

        if (HasLibraryIdentity(library) && TryResolveLibrary(library.Name!, library.Version!, out var existing))
            return existing!;

        AddLibrary(library);
        return library;
    }

    /// <summary>
    /// Saves a managed node map to a stream as a <c>.nmap</c> file.
    /// </summary>
    public void SaveMap(NodeMap map, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (_maps.All(item => item.Id != map.Id))
            throw new InvalidOperationException("The provided node map is not managed by this manager.");

        WriteStream(stream, map);
    }

    /// <summary>
    /// Saves a managed shared node library to a stream as a <c>.nlib</c> file.
    /// </summary>
    public void SaveLibrary(NodeLibrary library, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(library);

        if (!_libraries.Contains(library))
            throw new InvalidOperationException("The provided node library is not managed by this manager.");

        WriteStream(stream, library);
    }

    #endregion Public Methods

    #region Private Methods

    private static T ReadStream<T>(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new InvalidOperationException("The provided stream is not readable.");

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

        return JsonSerializer.Deserialize<T>(stream, JsonOptions)
               ?? throw new InvalidOperationException($"The provided stream could not be deserialized as {typeof(T).Name}.");
    }

    private static void WriteStream<T>(Stream stream, T value)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(value);

        if (!stream.CanWrite)
            throw new InvalidOperationException("The provided stream is not writable.");

        if (stream.CanSeek)
        {
            stream.SetLength(0);
            stream.Seek(0, SeekOrigin.Begin);
        }

        JsonSerializer.Serialize(stream, value, JsonOptions);

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
    }

    private static bool HasLibraryIdentity(NodeLibrary library)
    {
        return !string.IsNullOrWhiteSpace(library.Name) && !string.IsNullOrWhiteSpace(library.Version);
    }

    private static bool HasSameLibraryIdentity(NodeLibrary left, NodeLibrary right)
    {
        return string.Equals(left.Name, right.Name, StringComparison.Ordinal)
               && string.Equals(left.Version, right.Version, StringComparison.Ordinal);
    }

    #endregion Private Methods
}

