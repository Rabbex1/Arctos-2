using System.Runtime.Loader;

namespace Arctos.Plugins;

public sealed class PluginHandle
{
    public IArctosPlugin Instance { get; set; } = null!;
    public PluginLoadContext LoadContext { get; set; } = null!;
    public PluginDescriptor Descriptor { get; set; } = null!;
}
