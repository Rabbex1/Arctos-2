namespace Arctos.Wpf.Dialogs
{
  public class DialogBaseVM : BaseVM, IDialogBaseVM
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public bool WasCanceled { get; set; }

    public Action<DialogResult>? OnCLose { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public virtual void OnOpen()
    {
      RaiseDialogOpening();
    }

    #endregion Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events

    public event EventHandler? DialogOpening;
    public void RaiseDialogOpening()
    {
      DialogOpening?.Invoke(this, EventArgs.Empty);
    }

        public event EventHandler<CloseDialogArgs>? DialogClosing;
        public void RaiseDialogClosing(CloseDialogArgs args)
        {
            DialogClosing?.Invoke(this, args);
        }

        #endregion Events
    }
}
