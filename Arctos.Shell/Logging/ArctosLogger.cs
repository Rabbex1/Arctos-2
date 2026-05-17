using System.Text;
using Microsoft.Extensions.Logging;

namespace Arctos.Shell.Logging
{
    public sealed class ArctosLogger(
        string category,
        ILogStore logStore,
        Func<IExternalScopeProvider?> scopeProviderFactory) : ILogger
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly string _category = category;
        private readonly ILogStore _logStore = logStore;
        private readonly Func<IExternalScopeProvider?> _scopeProviderFactory = scopeProviderFactory;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region ILogger

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _scopeProviderFactory()?.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message) && exception == null)
                return;

            _logStore.Add(new LogEntry(
                DateTimeOffset.Now,
                logLevel,
                _category,
                message,
                eventId,
                exception,
                BuildScopeText()));
        }

        #endregion ILogger

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        private string? BuildScopeText()
        {
            var scopeProvider = _scopeProviderFactory();
            if (scopeProvider == null)
                return null;

            var builder = new StringBuilder();
            scopeProvider.ForEachScope((scope, text) =>
            {
                if (text.Length > 0)
                    text.Append(" => ");

                text.Append(scope);
            }, builder);

            return builder.Length == 0 ? null : builder.ToString();
        }

        #endregion Methods
    }
}
