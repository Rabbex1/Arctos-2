namespace Arctos.Wpf.Dialogs
{
  public interface IDialogVM : IDialogBaseVM
  {
    void RequestClose();
    bool IsModal { get; }
  }
}
