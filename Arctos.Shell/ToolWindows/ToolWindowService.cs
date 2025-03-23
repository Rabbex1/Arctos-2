using System.Collections.ObjectModel;
using System.ComponentModel;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Arctos.Shell.Messages;
using Arctos.Wpf.AvalonDock;
using Arctos.Wpf.ViewModels;
using Arctos.Messenger;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Shell.ToolWindows
{
    public class ToolWindowService : ObservableObject, IToolWindowService, IRecipient<AvalonDockMessage>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Dictionary<string, ToolWindowVM> _availableToolWindows = new();

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

        public ToolWindowService()
        {
            WeakReferenceMessenger.Default.Register<AvalonDockMessage>(this);
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void RegisterToolWindow(ToolWindowVM toolWindow)
        {
            var vmName = toolWindow.GetType().FullName;
            if (string.IsNullOrWhiteSpace(vmName))
                return;
            _availableToolWindows.Add(vmName, toolWindow);
            toolWindow.PropertyChanged += ToolWindowPropertyChanged;
        }

        public bool HasToolWindow(string contentId)
        {
            return _availableToolWindows.ContainsKey(contentId);
        }

        public void ShowToolWindow(string? contentId)
        {
            if(contentId == null) 
                return;

            var toolWindow = ToolWindows.FirstOrDefault(tb => tb.ContentId.Contains("Class=" + contentId));
            if (toolWindow != null)
            {
                toolWindow.IsVisible = true;
                toolWindow.IsActive = true;
                return;
            }
            if (!_availableToolWindows.ContainsKey(contentId))
                return;
            toolWindow = _availableToolWindows[contentId];
            toolWindow.IsVisible = true;
            toolWindow.IsActive = true;
            ToolWindows.Add(toolWindow);
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void ToolWindowPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ToolWindowVM toolWindow)
                return;
            if (e.PropertyName == "IsActive" || e.PropertyName == "IsSelected" && !toolWindow.IsActive)
                WeakReferenceMessenger.Default.Send(new ToolWindowMessage(this, ToolWindowAction.ActiveToolWindowChanged, new object[] { toolWindow } ));
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Message Handler

        public void Receive(AvalonDockMessage msg)
        {
            if (msg.Action != AvalonDockAction.AnchorablePaneLoaded)
                return;

            if (msg.Params?[0] is not LayoutSerializationCallbackEventArgs loadEventArgs)
                return;

            var content = DockingPaneVM.GetContentComponents(loadEventArgs.Model.ContentId);
            if (!content.ContainsKey("Class"))
                return;
            var toolWindowClassName = content["Class"];

            if (!_availableToolWindows.ContainsKey(toolWindowClassName))
            {
                msg.Cancel = true;
                return;
            }
            var toolWindow = _availableToolWindows[toolWindowClassName];
            ToolWindows.Add(toolWindow);
            loadEventArgs.Content = toolWindow;
            if (loadEventArgs.Model is LayoutAnchorable layoutAnchorable)
                toolWindow.IsVisible = layoutAnchorable.IsVisible;
        }

        #endregion Message Handler


    }
}
