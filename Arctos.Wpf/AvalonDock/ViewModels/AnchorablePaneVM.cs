using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Arctos.Wpf.AvalonDock;

namespace Arctos.Wpf.AvalonDock
{
    public abstract partial class AnchorablePaneVM : DockingPaneVM, IAnchorablePaneVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region IsVisible
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Consrtuctors

        protected AnchorablePaneVM(string title)
            : base(title)
        { }

        #endregion Consrtuctors

        public event EventHandler<EventArgs>? Hiding;

        private void RaiseHiding()
        {
            Hiding?.Invoke(this, EventArgs.Empty);
        }

        #region Commands
        [RelayCommand]
        public void CloseCommand()
        {
            RaiseHiding();
        }
        #endregion

    }
}