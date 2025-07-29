namespace Arctos.Plugins;

public interface IPluginMetadata
{
    string Name { get; }
    string? Description { get; }
    string? Version { get; }
}
