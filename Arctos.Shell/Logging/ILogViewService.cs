using System.ComponentModel;

namespace Arctos.Shell.Logging
{
    public interface ILogViewService : INotifyPropertyChanged
    {
        ILogViewContext CurrentContext { get; }

        ILogViewContext DefaultContext { get; }

        void SetContext(ILogViewContext context);

        void SetDefaultContext();

        void SetDefaultContext(ILogViewContext context);
    }
}
