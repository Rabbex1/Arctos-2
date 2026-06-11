using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.Documents;
using Arctos.Nodes.Wpf.ViewModels.Documents;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Nodes.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Registers the first-pass WPF node services and tool-window VMs.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Public Methods

    /// <summary>
    /// Registers the core nodes manager together with the initial WPF tool windows.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddNodesServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<NodesManager>();
        services.AddSingleton<NodeSession>();
        services.AddSingleton<NodeDocumentFactory>();
        services.AddTransient<NodeMapVM>();
        services.AddTransient<NodeLibraryVM>();
        services.AddSingleton<NodeStructureToolsVM>();
        services.AddSingleton<NodeEditToolsVM>();

        return services;
    }

    #endregion Public Methods
}
