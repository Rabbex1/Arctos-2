using System;

namespace Arctos.Plugins;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PluginMetadataAttribute(string name) : Attribute, IPluginMetadata
{
    public string Name { get; } = name;
    public string? Description { get; set; }
    public string? Version { get; set; }
}
