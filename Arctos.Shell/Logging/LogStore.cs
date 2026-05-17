using System.Collections.ObjectModel;
using System.Windows;

namespace Arctos.Shell.Logging
{
    public sealed class LogStore : ILogStore
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly ObservableCollection<LogEntry> _entries = [];
        private readonly ReadOnlyObservableCollection<LogEntry> _readOnlyEntries;
        private readonly object _syncRoot = new();

        private int _maxEntryCount = 5000;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ReadOnlyObservableCollection<LogEntry> Entries => _readOnlyEntries;

        public int MaxEntryCount
        {
            get => _maxEntryCount;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _maxEntryCount = value;
                TrimIfRequired();
            }
        }

        #endregion Properties

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public LogStore()
        {
            _readOnlyEntries = new ReadOnlyObservableCollection<LogEntry>(_entries);
        }

        #endregion Constructor

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void Add(LogEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            InvokeOnUiThread(() =>
            {
                _entries.Add(entry);
                TrimIfRequired();
            });
        }

        public void Clear()
        {
            InvokeOnUiThread(_entries.Clear);
        }

        private void TrimIfRequired()
        {
            while (_entries.Count > _maxEntryCount)
                _entries.RemoveAt(0);
        }

        private void InvokeOnUiThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                lock (_syncRoot)
                {
                    action();
                }

                return;
            }

            dispatcher.Invoke(() =>
            {
                lock (_syncRoot)
                {
                    action();
                }
            });
        }

        #endregion Methods
    }
}
