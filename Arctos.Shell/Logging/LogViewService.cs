using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Shell.Logging
{
    public sealed class LogViewService : ObservableObject, ILogViewService
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private ILogViewContext _currentContext;
        private ILogViewContext _defaultContext;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ILogViewContext CurrentContext
        {
            get => _currentContext;
            private set => SetProperty(ref _currentContext, value);
        }

        public ILogViewContext DefaultContext
        {
            get => _defaultContext;
            private set => SetProperty(ref _defaultContext, value);
        }

        #endregion Properties

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public LogViewService()
        {
            _defaultContext = new LogViewContext();
            _currentContext = _defaultContext;
        }

        #endregion Constructor

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void SetContext(ILogViewContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            CurrentContext = context;
        }

        public void SetDefaultContext()
        {
            CurrentContext = DefaultContext;
        }

        public void SetDefaultContext(ILogViewContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var defaultWasActive = ReferenceEquals(CurrentContext, DefaultContext);
            DefaultContext = context;

            if (defaultWasActive)
                CurrentContext = DefaultContext;
        }

        #endregion Methods
    }
}
