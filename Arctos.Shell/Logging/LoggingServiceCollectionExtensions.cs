using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arctos.Shell.Logging
{
    public static class LoggingServiceCollectionExtensions
    {
        public static IServiceCollection AddArctosShellLogging(this IServiceCollection services)
        {
            services.AddSingleton<ILogStore, LogStore>();
            services.AddSingleton<ILogViewService, LogViewService>();
            services.AddSingleton<LogToolWindowVM>();
            services.AddSingleton<ILoggerProvider, ArctosLoggerProvider>();
            return services;
        }
    }
}
