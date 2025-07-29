using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;

namespace Arctos.Wpf
{
    public static class ResourceLoader
    {
        private static readonly Dictionary<IResourceProvider, List<Uri>> _loaded = new();

        public static void LoadResources(IEnumerable<IResourceProvider> providers)
        {
            foreach (var provider in providers)
            {
                var uris = provider.GetResourceUris().ToList();
                foreach (var uri in uris)
                {
                    try
                    {
                        var dict = new ResourceDictionary { Source = uri };
                        Application.Current.Resources.MergedDictionaries.Add(dict);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load resource: {uri} {ex.Message}");
                    }
                }

                _loaded[provider] = uris;
            }
        }

        public static void UnloadResources(IResourceProvider provider)
        {
            if (_loaded.TryGetValue(provider, out var uris))
            {
                foreach (var uri in uris)
                {
                    var toRemove = Application.Current.Resources.MergedDictionaries
                        .FirstOrDefault(d => d.Source == uri);
                    if (toRemove != null)
                    {
                        Application.Current.Resources.MergedDictionaries.Remove(toRemove);
                    }
                }

                _loaded.Remove(provider);
            }
        }
    }
}
