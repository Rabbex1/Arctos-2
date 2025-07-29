using System.Reflection;

namespace Arctos.Plugins;

public class PluginManager(PluginLoaderService loader, IServiceProvider services)
{
    public IEnumerable<PluginDescriptor> DiscoverPlugins<TAppSignature>(string[] folders)
    {
        return loader.DiscoverPlugins<TAppSignature>(folders);
    }

    public (IArctosPlugin? instance, Assembly? assembly) LoadPlugin(PluginDescriptor descriptor)
    {
        var (plugin, assembly, _) = loader.LoadPluginInstance(descriptor, services);
        return (plugin, assembly);
    }

    public bool UnloadPlugin(string name)
    {
        return loader.UnloadPlugin(name);
    }

    public void UnloadAll()
    {
        loader.UnloadAll();
    }

    public IReadOnlyList<PluginHandle> LoadedPlugins => loader.LoadedPlugins;

    public void SetLogger(Action<string> logAction)
    {
        loader.LogMessage = logAction;
    }
}