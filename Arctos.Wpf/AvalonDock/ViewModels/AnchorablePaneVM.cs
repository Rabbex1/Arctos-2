using CommunityToolkit.Mvvm.Input;

namespace Arctos.Wpf.AvalonDock
{
    public abstract partial class AnchorablePaneVM(string title) : DockingPaneVM(title), IAnchorablePaneVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region IsVisible
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (!SetProperty(ref _isVisible, value))
                    return;

                if (value)
                    OnShowing();
                else
                    OnHiding();
            }
        }
        #endregion

        #endregion Properties

        public event EventHandler<EventArgs>? Hiding;

        private void RaiseHiding()
        {
            Hiding?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnShowing()
        {
        }

        protected virtual void OnHiding()
        {
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
