using Arctos.Plugins;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Application = System.Windows.Application;

namespace Arctos.Wpf.Plugins
{
    public static class WpfPluginLoader
    {
        private static readonly Dictionary<Assembly, List<Uri>> LoadedResources = new();

        public static IArctosPlugin? LoadPlugin(PluginDescriptor descriptor, PluginManager pluginManager)
        {
            var (plugin, assembly) = pluginManager.LoadPlugin(descriptor);
            if (assembly != null)
                LoadWpfResources(assembly);
            return plugin;
        }

        public static bool UnloadPlugin(IArctosPlugin plugin, PluginManager pluginManager)
        {
            var fullName = plugin.GetType().FullName;
            if (fullName == null)
            {
                Debug.WriteLine($"[WPFPlugin] Cannot unload plugin: {plugin.GetType().Name} - FullName is null.");
                return false;
            }

            var assembly = plugin.GetType().Assembly;

            if (!pluginManager.UnloadPlugin(fullName))
                return false;
            UnloadWpfResources(assembly);
            return true;
        }

        public static bool UnloadPlugin(string fullName, PluginManager pluginManager)
        {
            var type = Type.GetType(fullName);
            if (type == null) 
                return false;
            var assembly = type.Assembly;
            if (!pluginManager.UnloadPlugin(fullName))
                return false;
            UnloadWpfResources(assembly);
            return true;
        }

        public static void UnloadAllPlugins(PluginManager pluginManager)
        {
            pluginManager.UnloadAll();
            UnloadAllWpfResources();
        }

    public static void LoadWpfResources(Assembly pluginAssembly)
        {
            try
            {
                var addedUris = new List<Uri>();

                foreach (var type in pluginAssembly.GetTypes())
                {
                    if (!typeof(IWpfResourceProvider).IsAssignableFrom(type)
                        || type.IsAbstract
                        || type.GetConstructor(Type.EmptyTypes) is null)
                        continue;
                    if (Activator.CreateInstance(type) is not IWpfResourceProvider provider) 
                        continue;
                    if (Application.Current.Resources.MergedDictionaries.All(d => d.Source != provider.ResourceUri))
                    {
                        var dict = new ResourceDictionary {Source = provider.ResourceUri};
                        Application.Current.Resources.MergedDictionaries.Add(dict);
                        addedUris.Add(provider.ResourceUri);
                    }

                    Debug.WriteLine($"[WPFPlugin] Loaded: {provider.ResourceUri}");
                }

                if (addedUris.Count > 0)
                    LoadedResources[pluginAssembly] = addedUris;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WPFPlugin] Error loading WPF resources: {ex.Message}");
            }
        }

        public static void UnloadWpfResources(Assembly pluginAssembly)
        {
            if (!LoadedResources.TryGetValue(pluginAssembly, out var uris))
                return;

            var appResources = Application.Current.Resources.MergedDictionaries;

            foreach (var uri in uris)
            {
                var dict = appResources
                    .FirstOrDefault(d => d.Source != null && d.Source.Equals(uri));
                if (dict != null)
                {
                    appResources.Remove(dict);
                    Debug.WriteLine($"[WPFPlugin] Unloaded: {uri}");
                }
            }

            LoadedResources.Remove(pluginAssembly);
        }

        public static void UnloadAllWpfResources()
        {
            foreach (var assembly in LoadedResources.Keys.ToList())
                UnloadWpfResources(assembly);
        }
    }
}
