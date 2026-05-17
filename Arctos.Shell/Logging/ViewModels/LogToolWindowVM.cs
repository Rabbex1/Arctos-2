using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Arctos.Shell.ToolWindows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Arctos.Shell.Logging
{
    public partial class LogToolWindowVM : ToolWindowVM
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly ILogStore _logStore;
        private readonly ILogViewService _logViewService;
        private ILogViewContext _subscribedContext;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ILogStore LogStore => _logStore;

        public ICollectionView LogsView { get; }

        public ILogViewContext CurrentContext => _logViewService.CurrentContext;

        [ObservableProperty]
        private LogEntry? _selectedEntry;

        #endregion Properties

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public LogToolWindowVM(ILogStore logStore, ILogViewService logViewService)
            : base("Log")
        {
            _logStore = logStore;
            _logViewService = logViewService;
            _subscribedContext = _logViewService.CurrentContext;

            LogsView = CollectionViewSource.GetDefaultView(_logStore.Entries);
            LogsView.Filter = FilterEntry;

            _logViewService.PropertyChanged += LogViewServicePropertyChanged;
            _subscribedContext.PropertyChanged += CurrentContextPropertyChanged;
        }

        #endregion Constructor

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        private void Clear()
        {
            _logStore.Clear();
        }

        [RelayCommand]
        private void CopySelected()
        {
            if (SelectedEntry == null)
                return;

            Clipboard.SetText(FormatEntry(SelectedEntry));
        }

        [RelayCommand]
        private void CopyVisible()
        {
            var visibleEntries = LogsView
                .Cast<LogEntry>()
                .Select(FormatEntry)
                .ToList();

            if (visibleEntries.Count == 0)
                return;

            Clipboard.SetText(string.Join(Environment.NewLine, visibleEntries));
        }

        #endregion Commands

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        private bool FilterEntry(object obj)
        {
            if (obj is not LogEntry entry)
                return false;

            if (!IsLevelVisible(entry.Level))
                return false;

            if (!string.IsNullOrWhiteSpace(CurrentContext.ScopeFilter) &&
                !ContainsIgnoreCase(entry.Scope, CurrentContext.ScopeFilter))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CurrentContext.CategoryFilter) &&
                !ContainsIgnoreCase(entry.Category, CurrentContext.CategoryFilter))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CurrentContext.EventFilter) &&
                !ContainsIgnoreCase(entry.Event, CurrentContext.EventFilter))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CurrentContext.TextFilter) &&
                !MatchesTextFilter(entry, CurrentContext.TextFilter))
            {
                return false;
            }

            return true;
        }

        private bool IsLevelVisible(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => CurrentContext.ShowTrace,
                LogLevel.Debug => CurrentContext.ShowDebug,
                LogLevel.Information => CurrentContext.ShowInformation,
                LogLevel.Warning => CurrentContext.ShowWarning,
                LogLevel.Error => CurrentContext.ShowError,
                LogLevel.Critical => CurrentContext.ShowCritical,
                _ => true
            };
        }

        private static bool MatchesTextFilter(LogEntry entry, string textFilter)
        {
            return ContainsIgnoreCase(entry.Message, textFilter) ||
                   ContainsIgnoreCase(entry.Category, textFilter) ||
                   ContainsIgnoreCase(entry.Event, textFilter) ||
                   ContainsIgnoreCase(entry.Scope, textFilter) ||
                   ContainsIgnoreCase(entry.ExceptionText, textFilter);
        }

        private static bool ContainsIgnoreCase(string? text, string filter)
        {
            return !string.IsNullOrWhiteSpace(text) &&
                   text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CurrentContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            LogsView.Refresh();
            RaisePropertyChanged(nameof(CurrentContext));
        }

        private static string FormatEntry(LogEntry entry)
        {
            var builder = new StringBuilder();
            builder.Append(entry.Timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            builder.Append(" [");
            builder.Append(entry.Level);
            builder.Append("] ");

            if (!string.IsNullOrWhiteSpace(entry.Scope))
            {
                builder.Append(entry.Scope);
                builder.Append(" | ");
            }

            builder.Append(entry.Event);
            builder.Append(" | ");
            builder.Append(entry.Category);
            builder.Append(": ");
            builder.Append(entry.Message);

            if (!string.IsNullOrWhiteSpace(entry.ExceptionText))
            {
                builder.AppendLine();
                builder.Append(entry.ExceptionText);
            }

            return builder.ToString();
        }

        private void LogViewServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ILogViewService.CurrentContext))
                return;

            RaisePropertyChanged(nameof(CurrentContext));
            _subscribedContext.PropertyChanged -= CurrentContextPropertyChanged;
            _subscribedContext = _logViewService.CurrentContext;
            _subscribedContext.PropertyChanged += CurrentContextPropertyChanged;
            LogsView.Refresh();
        }

        #endregion Methods
    }
}
