using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace Arctos.Plugins;

public sealed class PluginLoaderService
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly List<PluginHandle> _loaded = [];

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public IReadOnlyList<PluginHandle> LoadedPlugins => _loaded;

    public Action<string>? LogMessage { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    //---------------------------------------------------------------------
    #region Public Methods

    public IEnumerable<PluginDescriptor> DiscoverPlugins<TAppSignature>(string[] folders, params string[]? assemblyPrefixes)
    {
        var descriptors = new List<PluginDescriptor>();
        var requiredPrefixes = (assemblyPrefixes ?? [])
            .Append("ArctosPlugin.")
            .Where(prefix => !string.IsNullOrWhiteSpace(prefix))
            .Select(prefix => prefix.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
                continue;

            var dlls = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
            var loadedTypeNames = _loaded.Select(p => p.Descriptor.TypeName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var path in dlls)
            {
                Assembly? assembly = null;
                var fileName = Path.GetFileNameWithoutExtension(path);

                if (!requiredPrefixes.Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine($"[PluginLoader] Skipping non-plugin assembly: {fileName}");
                    continue;
                }

                try
                {
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"Failed to load assembly: {path}\n{ex.Message}");
                    continue;
                }

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (!typeof(IArctosPlugin).IsAssignableFrom(type))
                        continue;

                    if (!typeof(TAppSignature).IsAssignableFrom(type))
                        continue;

                    var meta = type.GetCustomAttribute<PluginMetadataAttribute>();
                    if (meta == null)
                        continue;

                    descriptors.Add(new PluginDescriptor
                    {
                        AssemblyPath = path,
                        TypeName = type.FullName!,
                        Name = meta.Name,
                        Description = meta.Description,
                        Version = meta.Version,
                        IsLoaded = loadedTypeNames.Contains(type.FullName!)
                    });
                }
            }
        }

        return descriptors;
    }

    public (IArctosPlugin? Instance, Assembly? assembly, PluginLoadContext? Context) LoadPluginInstance(PluginDescriptor descriptor, IServiceProvider services)
    {
        if (_loaded.Any(p => p.Descriptor.Name == descriptor.Name))
        {
            LogMessage?.Invoke($"Plugin '{descriptor.Name}' is already loaded.");
            return (null, null, null);
        }

        var context = new PluginLoadContext(descriptor.AssemblyPath);

        try
        {
            var assemblyName = AssemblyName.GetAssemblyName(descriptor.AssemblyPath);
            var assembly = context.LoadFromAssemblyName(assemblyName);

            var type = assembly.GetType(descriptor.TypeName);
            var typeIs = type == null ? "Null" : type.FullName;
            Debug.WriteLine($"Type is - {typeIs}");
            if (type == null || !type.IsAssignableTo(typeof(IArctosPlugin)))
                return (null, null, null);

            var plugin = (IArctosPlugin?)ActivatorUtilities.CreateInstance(services, type);
            plugin?.Initialize(services);

            _loaded.Add(new PluginHandle
            {
                Instance = plugin!,
                LoadContext = context,
                Descriptor = descriptor
            });

            LogMessage?.Invoke($"Plugin loaded: {descriptor.Name}");
            return (plugin, assembly, context);
        }
        catch (Exception ex)
        {
            context.Unload();
            LogMessage?.Invoke($"Failed to load plugin '{descriptor.Name}': {ex.Message}");
            return (null, null, null);
        }
    }

    public bool UnloadPlugin(string name)
    {
        var handle = _loaded.FirstOrDefault(p => p.Descriptor.TypeName == name);
        if (handle == null)
            return false;

        _loaded.Remove(handle);
        handle.Instance = null!;
        handle.LoadContext.Unload();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        LogMessage?.Invoke($"Plugin unloaded: {name}");
        return true;
    }

    public void UnloadAll()
    {
        foreach (var handle in _loaded)
        {
            handle.Instance = null!;
            handle.LoadContext.Unload();
        }

        _loaded.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        LogMessage?.Invoke("All plugins unloaded.");
    }

    #endregion Public Methods

    //---------------------------------------------------------------------
    #region Private Methods

    private static bool IsSystemAssembly(string name)
    {
        return name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("Windows.", StringComparison.OrdinalIgnoreCase)
               || name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase);
    }

    #endregion Private Methods

    #endregion Methods
}
