using System.ComponentModel;

namespace Arctos.Shell.Logging
{
    public interface ILogViewContext : INotifyPropertyChanged
    {
        bool AutoScroll { get; set; }

        string? CategoryFilter { get; set; }

        string? EventFilter { get; set; }

        bool ShowCategory { get; set; }

        bool ShowCritical { get; set; }

        bool ShowDebug { get; set; }

        bool ShowError { get; set; }

        bool ShowEvent { get; set; }

        bool ShowInformation { get; set; }

        bool ShowLevel { get; set; }

        bool ShowMessage { get; set; }

        bool ShowScope { get; set; }

        bool ShowTimestamp { get; set; }

        bool ShowTrace { get; set; }

        bool ShowWarning { get; set; }

        string? ScopeFilter { get; set; }

        string? TextFilter { get; set; }
    }
}
