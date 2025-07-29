namespace Arctos.Plugins;

public sealed class PluginDescriptor
{
    public required string AssemblyPath { get; set; }       // Full DLL path
    public required string TypeName { get; set; }           // Full class name (namespace + type)
    public required string Name { get; set; }               // From [PluginMetadata]
    public string? Description { get; set; }                // From [PluginMetadata]
    public string? Version { get; set; }                    // From [PluginMetadata]

    public bool IsLoaded
    {
        get; 
        set;
    } = false;              // For UI use

    public override string ToString() => $"{Name} ({Version}) — {TypeName}";

    public PluginDescriptor Clone()
    {
        return new PluginDescriptor
        {
            AssemblyPath = AssemblyPath,
            TypeName = TypeName,
            Name = Name,
            Description = Description,
            Version = Version,
            IsLoaded = IsLoaded,
        };
    }
}