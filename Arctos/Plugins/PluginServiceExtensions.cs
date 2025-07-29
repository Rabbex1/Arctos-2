using Microsoft.Extensions.DependencyInjection;

namespace Arctos.Plugins;

public static class PluginServicesExtensions
{
    public static IServiceCollection AddPluginServices(this IServiceCollection services)
    {
        services.AddSingleton<PluginLoaderService>();
        services.AddSingleton<PluginManager>();
        return services;
    }
}