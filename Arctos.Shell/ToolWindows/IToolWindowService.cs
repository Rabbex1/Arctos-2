using Arctos.Shell.Messages;
using Arctos.Wpf.AvalonDock;
using AvalonDock.Layout.Serialization;
using AvalonDock.Layout;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Shell.ToolWindows
{
    public interface IToolWindowService
    {

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ObservableCollection<ToolWindowVM> ToolWindows { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void RegisterToolWindow(ToolWindowVM toolWindow);
        public bool HasToolWindow(string contentId);
        public void ShowToolWindow(string? contentId);

        #endregion Methods

    }
}
