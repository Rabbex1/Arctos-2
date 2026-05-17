using System.Collections.ObjectModel;

namespace Arctos.Shell.Logging
{
    public interface ILogStore
    {
        ReadOnlyObservableCollection<LogEntry> Entries { get; }

        int MaxEntryCount { get; set; }

        void Add(LogEntry entry);

        void Clear();
    }
}
