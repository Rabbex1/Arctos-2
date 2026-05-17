using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Shell.Logging
{
    public partial class LogViewContext : ObservableObject, ILogViewContext
    {
        [ObservableProperty]
        private bool _autoScroll = true;

        [ObservableProperty]
        private string? _categoryFilter;

        [ObservableProperty]
        private string? _eventFilter;

        [ObservableProperty]
        private bool _showCategory;

        [ObservableProperty]
        private bool _showCritical = true;

        [ObservableProperty]
        private bool _showDebug = true;

        [ObservableProperty]
        private bool _showError = true;

        [ObservableProperty]
        private bool _showEvent = true;

        [ObservableProperty]
        private bool _showInformation = true;

        [ObservableProperty]
        private bool _showLevel = true;

        [ObservableProperty]
        private bool _showMessage = true;

        [ObservableProperty]
        private bool _showScope = true;

        [ObservableProperty]
        private bool _showTimestamp = true;

        [ObservableProperty]
        private bool _showTrace = true;

        [ObservableProperty]
        private bool _showWarning = true;

        [ObservableProperty]
        private string? _scopeFilter;

        [ObservableProperty]
        private string? _textFilter;
    }
}
