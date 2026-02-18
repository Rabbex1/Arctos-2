using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Arctos.Shell.Messages;
using Arctos.Wpf.AvalonDock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Shell.ToolWindows
{
    public class ToolWindowService : ObservableObject, IToolWindowService, IRecipient<AvalonDockMessage>, IDisposable
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Dictionary<string, ToolWindowVM> _availableToolWindows = new();
        private readonly IMessenger _messenger;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region ToolWindows
        private ObservableCollection<ToolWindowVM> _toolWindows = new();
        public ObservableCollection<ToolWindowVM> ToolWindows
        {
            get => _toolWindows;
            set => SetProperty(ref _toolWindows, value);
        }
        #endregion ToolWindows

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public ToolWindowService(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Register<AvalonDockMessage>(this);
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void RegisterToolWindow(ToolWindowVM toolWindow)
        {
            var vmName = toolWindow.GetType().FullName;
            if (string.IsNullOrWhiteSpace(vmName))
                return;

            if (!_availableToolWindows.TryAdd(vmName, toolWindow))
                return;

            toolWindow.PropertyChanged += ToolWindowPropertyChanged;
        }

        public bool HasToolWindow(string contentId)
        {
            return _availableToolWindows.ContainsKey(contentId);
        }

        public void ShowToolWindow(string? contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return;

            var matches = ToolWindows
                .Where(tb => tb?.GetType().FullName == contentId)
                .ToList();

            if (matches.Count > 0)
            {
                var primary = matches[0];
                for (int i = matches.Count - 1; i >= 1; i--)
                    ToolWindows.Remove(matches[i]);

                primary.IsVisible = true;
                primary.IsActive = true;
                return;
            }

            if (!_availableToolWindows.TryGetValue(contentId, out var toolWindow))
                return;

            toolWindow.IsVisible = true;
            toolWindow.IsActive = true;
            ToolWindows.Add(toolWindow);
        }

        public void Dispose()
        {
            _messenger.Unregister<AvalonDockMessage>(this);
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void ToolWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolWindowVM toolWindow)
                return;

            if (e.PropertyName == nameof(ToolWindowVM.IsActive) ||
                (e.PropertyName == nameof(ToolWindowVM.IsSelected) && !toolWindow.IsActive))
            {
                _messenger.Send(new ToolWindowMessage(this, ToolWindowAction.ActiveToolWindowChanged, [toolWindow]));
            }
        }

        #endregion Event Handlers

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Message Handler

        public void Receive(AvalonDockMessage msg)
        {
            if (msg.Action != AvalonDockAction.AnchorablePaneLoaded)
                return;

            if (msg.Params?[0] is not LayoutSerializationCallbackEventArgs loadEventArgs)
                return;

            var content = DockingPaneVM.GetContentComponents(loadEventArgs.Model.ContentId);
            if (!content.TryGetValue("Class", out var toolWindowClassName))
                return;

            if (!_availableToolWindows.TryGetValue(toolWindowClassName, out var toolWindow))
            {
                msg.Cancel = true;
                return;
            }

            if (ToolWindows.Contains(toolWindow))
                loadEventArgs.Cancel = true;
            else
                ToolWindows.Add(toolWindow);

            loadEventArgs.Content = toolWindow;
            if (loadEventArgs.Model is LayoutAnchorable layoutAnchorable)
            {
                toolWindow.IsVisible = layoutAnchorable.IsVisible;
                toolWindow.IsSelected = layoutAnchorable.IsSelected;
                toolWindow.IsActive = layoutAnchorable.IsActive;
            }
        }

        #endregion Message Handler
    }
}
