
namespace Arctos.Wpf.Dialogs
{
  public interface IDialogBaseVM
  {
    void OnOpen();

    event EventHandler DialogOpening;
    event EventHandler<CloseDialogArgs> DialogClosing;
  }
}
