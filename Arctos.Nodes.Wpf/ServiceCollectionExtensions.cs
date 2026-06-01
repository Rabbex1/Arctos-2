using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Microsoft.Extensions.DependencyInjection;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Registers the first-pass WPF nodes editing services and tool VMs.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public Methods

    /// <summary>
    /// Registers the nodes WPF session and tool-window VMs.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddArctosNodesWpf(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<NodesSessionVM>();
        services.AddSingleton<NodeDocumentVM>();
        services.AddSingleton<NodePropertiesVM>();
        services.AddSingleton<NodePaletteVM>();
        services.AddSingleton<NodesDocumentToolVM>();
        services.AddSingleton<NodesEditToolVM>();

        return services;
    }

    #endregion Public Methods
}
