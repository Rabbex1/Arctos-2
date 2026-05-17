using Microsoft.Extensions.Logging;

namespace Arctos.Shell.Logging
{
    public sealed class ArctosLoggerProvider(ILogStore logStore) : ILoggerProvider, ISupportExternalScope
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly ILogStore _logStore = logStore;
        private IExternalScopeProvider? _scopeProvider;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region ILoggerProvider

        public ILogger CreateLogger(string categoryName)
        {
            return new ArctosLogger(categoryName, _logStore, () => _scopeProvider);
        }

        public void Dispose()
        {
        }

        #endregion ILoggerProvider

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region ISupportExternalScope

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        #endregion ISupportExternalScope
    }
}
