namespace Arctos.Wpf.Dialogs
{
  public enum DialogResult
  {
    Ok,
    Cancel,
    Close,
    Yes,
    No
  }

  public class CloseDialogArgs : EventArgs
  {
    public DialogResult Result { get; set; }
  }
}
