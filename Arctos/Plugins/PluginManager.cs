using System.Diagnostics;
using System.Reflection;

namespace Arctos.Plugins;

public class PluginManager(PluginLoaderService loader, IServiceProvider services)
{
    public IEnumerable<PluginDescriptor> DiscoverPlugins<TAppSignature>(string[] folders, params string[]? assemblyPrefixes)
    {
        return loader.DiscoverPlugins<TAppSignature>(folders, assemblyPrefixes);
    }

    public (IArctosPlugin? instance, Assembly? assembly) LoadPlugin(PluginDescriptor descriptor)
    {
        var (plugin, assembly, _) = loader.LoadPluginInstance(descriptor, services);
        return (plugin, assembly);
    }

    public bool UnloadPlugin(string name)
    {
        var handle = loader.LoadedPlugins
            .FirstOrDefault(p => string.Equals(p.Descriptor.TypeName, name, StringComparison.OrdinalIgnoreCase));

        if (handle?.Instance != null)
        {
            try
            {
                handle.Instance.Terminate(services);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PluginManager] Terminate failed for '{name}': {ex.Message}");
            }
        }

        return loader.UnloadPlugin(name);
    }

    public void UnloadAll()
    {
        foreach (var handle in loader.LoadedPlugins.ToList())
        {
            try
            {
                handle.Instance?.Terminate(services);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PluginManager] Terminate failed for '{handle.Descriptor.TypeName}': {ex.Message}");
            }
        }

        loader.UnloadAll();
    }

    public IReadOnlyList<PluginHandle> LoadedPlugins => loader.LoadedPlugins;

    public void SetLogger(Action<string> logAction)
    {
        loader.LogMessage = logAction;
    }
}