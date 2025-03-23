using Arctos.Shell.Toolbar;
using Arctos.Wpf.AvalonDock;

namespace Arctos.Shell.ToolWindows
{
    public class ToolWindowVM : AnchorablePaneVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Toolbar

        private ToolbarVM? _toolBar;
        public ToolbarVM? ToolBar
        {
            get => _toolBar;
            set => SetProperty(ref _toolBar, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public ToolWindowVM(string title)
            : base(title)
        {
        }

        #endregion Constructor
    }
}