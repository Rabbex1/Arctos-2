using System.Reflection;
using System.Runtime.Loader;

namespace Arctos.Plugins;

public sealed class PluginLoadContext(string pluginPath) : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? dllPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return dllPath != null ? LoadUnmanagedDllFromPath(dllPath) : IntPtr.Zero;
    }
}
