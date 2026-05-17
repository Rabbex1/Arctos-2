using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Arctos.Shell.Logging
{
    public partial class LogToolWindowView : UserControl
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private const double TimestampColumnWidth = 95;
        private const double LevelColumnWidth = 90;
        private const double ScopeColumnWidth = 150;
        private const double EventColumnWidth = 130;
        private const double CategoryColumnWidth = 240;
        private const double MessageColumnWidth = 520;

        private INotifyCollectionChanged? _currentEntries;
        private ILogViewContext? _currentContext;
        private LogToolWindowVM? _currentViewModel;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public LogToolWindowView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Unloaded += OnUnloaded;
        }

        #endregion Constructor

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Unsubscribe();

            if (e.NewValue is not LogToolWindowVM vm)
                return;

            _currentViewModel = vm;
            _currentViewModel.PropertyChanged += ViewModelPropertyChanged;
            _currentEntries = vm.LogStore.Entries;
            _currentEntries.CollectionChanged += EntriesCollectionChanged;

            AttachContext(vm.CurrentContext);
            ApplyColumnVisibility();
        }

        private void EntriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (DataContext is not LogToolWindowVM vm || !vm.CurrentContext.AutoScroll)
                return;

            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null || e.NewItems.Count == 0)
                return;

            Dispatcher.BeginInvoke(() =>
            {
                if (e.NewItems[e.NewItems.Count - 1] != null)
                    LogList.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
            });
        }

        private void CurrentContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ApplyColumnVisibility();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unsubscribe();
        }

        private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(LogToolWindowVM.CurrentContext))
                return;

            if (_currentViewModel == null)
                return;

            AttachContext(_currentViewModel.CurrentContext);
            ApplyColumnVisibility();
        }

        #endregion Event Handlers

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        private void ApplyColumnVisibility()
        {
            if (_currentContext == null)
                return;

            TimestampColumn.Width = _currentContext.ShowTimestamp ? TimestampColumnWidth : 0;
            LevelColumn.Width = _currentContext.ShowLevel ? LevelColumnWidth : 0;
            ScopeColumn.Width = _currentContext.ShowScope ? ScopeColumnWidth : 0;
            EventColumn.Width = _currentContext.ShowEvent ? EventColumnWidth : 0;
            CategoryColumn.Width = _currentContext.ShowCategory ? CategoryColumnWidth : 0;
            MessageColumn.Width = _currentContext.ShowMessage ? MessageColumnWidth : 0;
        }

        private void AttachContext(ILogViewContext context)
        {
            if (ReferenceEquals(_currentContext, context))
                return;

            if (_currentContext != null)
                _currentContext.PropertyChanged -= CurrentContextPropertyChanged;

            _currentContext = context;
            _currentContext.PropertyChanged += CurrentContextPropertyChanged;
        }

        private void Unsubscribe()
        {
            if (_currentEntries != null)
            {
                _currentEntries.CollectionChanged -= EntriesCollectionChanged;
                _currentEntries = null;
            }

            if (_currentContext != null)
            {
                _currentContext.PropertyChanged -= CurrentContextPropertyChanged;
                _currentContext = null;
            }

            if (_currentViewModel != null)
            {
                _currentViewModel.PropertyChanged -= ViewModelPropertyChanged;
                _currentViewModel = null;
            }
        }

        #endregion Methods
    }
}
